using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// La taskbar propia del widget: una barra centrada abajo, sobre el escritorio,
/// dentro del espacio que <see cref="EspacioEscritorio.MargenInferiorReservado"/>
/// venía guardando para ella.
/// </summary>
/// <remarks>
/// A propósito no reimplementa lo que Windows ya resuelve bien: el menú de
/// Windows y la bandeja del sistema son los reales (invocados desde acá, no
/// reconstruidos), así que nunca pueden quedar con menos funciones que el
/// Windows de abajo. Pendiente: programas en ejecución y la posibilidad de
/// anclarlos, que van a vivir en <see cref="AppsEnEjecucion"/>.
/// </remarks>
public partial class TaskbarWindow : Window
{
    private const double AnchoBarra = 260;

    internal TaskbarWindow()
    {
        InitializeComponent();

        // La ventana es más alta que la barra que dibuja: ocupa toda la franja
        // reservada y centra la barra adentro. Lo que sobra es fondo nulo, así
        // que no se ve ni recibe clicks.
        var area = SystemParameters.WorkArea;
        Width = AnchoBarra;
        Height = EspacioEscritorio.MargenInferiorReservado;
        Left = area.Left + (area.Width - Width) / 2;
        Top = area.Bottom - Height;

        SourceInitialized += (_, _) =>
        {
            AnclajeEscritorio.OcultarDeAltTab(this);
            AnclajeEscritorio.HacerNoActivable(this);
        };
    }

    /// <summary>
    /// Abre el Start Menu real de Windows en vez de construir uno propio: así
    /// nunca le falta nada de lo que Windows ya trae.
    /// </summary>
    /// <remarks>
    /// El Win key lo procesa un hook global del shell, no la ventana con
    /// foco — funciona igual aunque esta ventana tenga <c>WS_EX_NOACTIVATE</c>
    /// y nunca pueda activarse.
    /// </remarks>
    private void BotonMenuWindows_Click(object sender, RoutedEventArgs e)
    {
        keybd_event(VK_LWIN, 0, 0, UIntPtr.Zero);
        keybd_event(VK_LWIN, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
    }

    private void BotonExplorador_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // no hay mucho más para hacer si ni Explorer arranca.
        }
    }

    /// <summary>
    /// Despliega la bandeja real de Windows (flecha "mostrar íconos ocultos")
    /// en vez de replicar los íconos: Windows no tiene una API pública para
    /// leerlos con fidelidad, y cualquier réplica se desincroniza del estado
    /// real (mic en uso, notificaciones nuevas, etc.).
    /// </summary>
    /// <remarks>
    /// Se probaron primero UI Automation (<c>InvokePattern</c>: no soportado
    /// por este control clásico de Win32; <c>LegacyIAccessiblePattern</c>:
    /// directamente ausente del UIAutomationClient que trae .NET acá) y un
    /// click físico por coordenadas (<c>SetCursorPos</c> + <c>mouse_event</c>):
    /// funciona, pero si otra ventana tapa esa esquina de la pantalla en ese
    /// instante, el click le llega a esa ventana en vez de a la bandeja.
    /// <c>SendMessage(BM_CLICK)</c> directo al hwnd del botón no tiene ese
    /// problema: le llega al control sin importar qué haya arriba tapándolo.
    /// </remarks>
    private void BotonBandeja_Click(object sender, RoutedEventArgs e)
    {
        var barraDeWindows = FindWindow("Shell_TrayWnd", null);
        if (barraDeWindows == IntPtr.Zero)
            return;

        var bandeja = FindWindowEx(barraDeWindows, IntPtr.Zero, "TrayNotifyWnd", null);
        if (bandeja == IntPtr.Zero)
            return;

        // "1502" es el id de control histórico del botón "Mostrar íconos
        // ocultos" dentro de TrayNotifyWnd — confirmado a mano, Windows no lo
        // documenta. Si no aparece (otra build de Windows), se prueba
        // cualquier "Button" que haya ahí: hoy solo vive ese.
        var flechaIconosOcultos = GetDlgItem(bandeja, 1502);
        if (flechaIconosOcultos == IntPtr.Zero)
            flechaIconosOcultos = FindWindowEx(bandeja, IntPtr.Zero, "Button", null);
        if (flechaIconosOcultos == IntPtr.Zero)
            return;

        SendMessage(flechaIconosOcultos, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
    }

    // ===================== interop =====================

    private const int VK_LWIN = 0x5B;
    private const int KEYEVENTF_KEYUP = 0x0002;
    private const uint BM_CLICK = 0x00F5;

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
}
