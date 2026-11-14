using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Muestra el video del wallpaper en cada pantalla que no es la principal,
/// sin reproducirlo otra vez.
/// </summary>
/// <remarks>
/// Hay un único <c>MediaPlayer</c>, que dibuja en <see cref="VideoWallpaperWindow"/>
/// sobre la pantalla principal. Cada pantalla extra tiene una
/// <see cref="EspejoWallpaperWindow"/> que le pide a DWM una miniatura en vivo
/// de esa ventana (<c>DwmRegisterThumbnail</c>): la compone la GPU a partir de
/// la superficie que ya existe, así que no hay segundo decode ni copia de
/// frames por la CPU. Es lo que mantiene el costo igual con una pantalla que
/// con tres.
///
/// El conjunto de espejos se reconstruye cada vez que cambia la configuración
/// de pantallas (la tableta digitalizadora entra y sale como una pantalla más).
/// </remarks>
internal sealed class EspejosDeWallpaper : IDisposable
{
    private readonly VideoWallpaperWindow _origen;
    private readonly AnclajeEscritorio _anclaje;
    private readonly List<EspejoWallpaperWindow> _espejos = [];
    private readonly DispatcherTimer _reconstruccion;

    public EspejosDeWallpaper(VideoWallpaperWindow origen, AnclajeEscritorio anclaje)
    {
        _origen = origen;
        _anclaje = anclaje;

        // El wallpaper también se recoloca al cambiar las pantallas: se espera
        // un instante para armar los espejos contra su tamaño ya actualizado.
        _reconstruccion = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(600) };
        _reconstruccion.Tick += (_, _) =>
        {
            _reconstruccion.Stop();
            Reconstruir();
        };

        SystemEvents.DisplaySettingsChanged += AlCambiarLasPantallas;
        Reconstruir();
    }

    private void AlCambiarLasPantallas(object? _, EventArgs __) =>
        _origen.Dispatcher.BeginInvoke(() =>
        {
            _reconstruccion.Stop();
            _reconstruccion.Start();
        });

    private void Reconstruir()
    {
        foreach (var espejo in _espejos)
        {
            _anclaje.Quitar(espejo);
            espejo.Close();
        }
        _espejos.Clear();

        var origen = new WindowInteropHelper(_origen).Handle;
        if (origen == IntPtr.Zero)
            return;

        foreach (var pantalla in PantallasSecundarias())
        {
            var espejo = new EspejoWallpaperWindow(origen, pantalla);
            espejo.Show();
            _espejos.Add(espejo);

            // Justo arriba del wallpaper, en el fondo del grupo.
            _anclaje.Agregar(espejo, indice: 1, clickThrough: true);
        }
    }

    public void Dispose()
    {
        SystemEvents.DisplaySettingsChanged -= AlCambiarLasPantallas;
        _reconstruccion.Stop();

        foreach (var espejo in _espejos)
        {
            _anclaje.Quitar(espejo);
            espejo.Close();
        }
        _espejos.Clear();
    }

    // ===================== monitores =====================

    private static List<RECT> PantallasSecundarias()
    {
        var resultado = new List<RECT>();
        EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, (monitor, _, _, _) =>
        {
            var info = new MONITORINFO { cbSize = Marshal.SizeOf<MONITORINFO>() };
            if (GetMonitorInfo(monitor, ref info) && (info.dwFlags & MONITORINFOF_PRIMARY) == 0)
                resultado.Add(info.rcMonitor);
            return true;
        }, IntPtr.Zero);

        return resultado;
    }

    private delegate bool MonitorEnumProc(IntPtr monitor, IntPtr hdc, IntPtr rect, IntPtr datos);

    [DllImport("user32.dll")]
    private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr recorte, MonitorEnumProc proc, IntPtr datos);

    [DllImport("user32.dll")]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MONITORINFO info);

    private const int MONITORINFOF_PRIMARY = 1;

    [StructLayout(LayoutKind.Sequential)]
    private struct MONITORINFO
    {
        public int cbSize;
        public RECT rcMonitor;
        public RECT rcWork;
        public int dwFlags;
    }
}

[StructLayout(LayoutKind.Sequential)]
internal struct RECT
{
    public int Left, Top, Right, Bottom;
    public readonly int Width => Right - Left;
    public readonly int Height => Bottom - Top;
}

/// <summary>
/// Una pantalla extra tapada con la miniatura en vivo del wallpaper, a
/// pantalla completa, recortada para llenarla sin franjas ni deformación
/// (mismo criterio que el recorte de VLC en la pantalla principal).
/// </summary>
internal sealed class EspejoWallpaperWindow : Window
{
    private readonly IntPtr _origen;
    private readonly RECT _pantalla;
    private IntPtr _miniatura;

    public EspejoWallpaperWindow(IntPtr origen, RECT pantalla)
    {
        _origen = origen;
        _pantalla = pantalla;

        Title = "Wallpaper (espejo)";
        Background = Brushes.Black;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        ShowActivated = false;
        WindowStartupLocation = WindowStartupLocation.Manual;

        SourceInitialized += (_, _) =>
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            AnclajeEscritorio.OcultarDeAltTab(this);
            AnclajeEscritorio.ImpedirMinimizado(this);
            AnclajeEscritorio.RechazarActivacionPorClick(this);

            // En píxeles, directo: WPF trabaja en unidades independientes del
            // DPI y esta pantalla puede tener otra escala que la principal.
            SetWindowPos(hwnd, IntPtr.Zero, _pantalla.Left, _pantalla.Top, _pantalla.Width, _pantalla.Height,
                SWP_NOZORDER | SWP_NOACTIVATE);

            Registrar();
        };

        ContentRendered += (_, _) => Registrar();
        SizeChanged += (_, _) => Actualizar();
        Closed += (_, _) => Soltar();
    }

    private void Registrar()
    {
        if (_miniatura != IntPtr.Zero)
            return;

        var destino = new WindowInteropHelper(this).Handle;
        var hr = DwmRegisterThumbnail(destino, _origen, out _miniatura);
        if (hr != 0)
            _miniatura = IntPtr.Zero;
        else
            Actualizar();
    }

    private void Soltar()
    {
        if (_miniatura == IntPtr.Zero)
            return;

        DwmUnregisterThumbnail(_miniatura);
        _miniatura = IntPtr.Zero;
    }

    /// <summary>
    /// Recorta el origen a la proporción de esta pantalla y lo estira para
    /// llenarla: "cover", centrado.
    /// </summary>
    private void Actualizar()
    {
        if (_miniatura == IntPtr.Zero)
            return;

        var destino = new WindowInteropHelper(this).Handle;
        if (!GetClientRect(destino, out var cliente) || DwmQueryThumbnailSourceSize(_miniatura, out var origen) != 0)
            return;

        if (cliente.Width <= 0 || cliente.Height <= 0 || origen.cx <= 0 || origen.cy <= 0)
            return;

        var recorte = new RECT { Left = 0, Top = 0, Right = origen.cx, Bottom = origen.cy };
        var proporcionDestino = (double)cliente.Width / cliente.Height;
        var proporcionOrigen = (double)origen.cx / origen.cy;

        if (proporcionOrigen > proporcionDestino)
        {
            var ancho = (int)Math.Round(origen.cy * proporcionDestino);
            recorte.Left = (origen.cx - ancho) / 2;
            recorte.Right = recorte.Left + ancho;
        }
        else
        {
            var alto = (int)Math.Round(origen.cx / proporcionDestino);
            recorte.Top = (origen.cy - alto) / 2;
            recorte.Bottom = recorte.Top + alto;
        }

        var propiedades = new DWM_THUMBNAIL_PROPERTIES
        {
            dwFlags = DWM_TNP_RECTDESTINATION | DWM_TNP_RECTSOURCE | DWM_TNP_VISIBLE | DWM_TNP_SOURCECLIENTAREAONLY,
            rcDestination = new RECT { Left = 0, Top = 0, Right = cliente.Width, Bottom = cliente.Height },
            rcSource = recorte,
            fVisible = true,
            fSourceClientAreaOnly = true,
        };
        DwmUpdateThumbnailProperties(_miniatura, ref propiedades);

    }

    // ===================== interop =====================

    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const int DWM_TNP_RECTDESTINATION = 0x1;
    private const int DWM_TNP_RECTSOURCE = 0x2;
    private const int DWM_TNP_VISIBLE = 0x8;
    private const int DWM_TNP_SOURCECLIENTAREAONLY = 0x10;

    [StructLayout(LayoutKind.Sequential)]
    private struct SIZE
    {
        public int cx, cy;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct DWM_THUMBNAIL_PROPERTIES
    {
        public int dwFlags;
        public RECT rcDestination;
        public RECT rcSource;
        public byte opacity;
        [MarshalAs(UnmanagedType.Bool)] public bool fVisible;
        [MarshalAs(UnmanagedType.Bool)] public bool fSourceClientAreaOnly;
    }

    [DllImport("dwmapi.dll")]
    private static extern int DwmRegisterThumbnail(IntPtr destino, IntPtr origen, out IntPtr miniatura);

    [DllImport("dwmapi.dll")]
    private static extern int DwmUnregisterThumbnail(IntPtr miniatura);

    [DllImport("dwmapi.dll")]
    private static extern int DwmUpdateThumbnailProperties(IntPtr miniatura, ref DWM_THUMBNAIL_PROPERTIES propiedades);

    [DllImport("dwmapi.dll")]
    private static extern int DwmQueryThumbnailSourceSize(IntPtr miniatura, out SIZE tamanio);

    [DllImport("user32.dll")]
    private static extern bool GetClientRect(IntPtr hwnd, out RECT rect);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hwnd, IntPtr despuesDe, int x, int y, int cx, int cy, uint flags);
}
