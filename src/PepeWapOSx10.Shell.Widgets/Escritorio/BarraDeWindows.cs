using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Esconde la barra de tareas de Windows mientras la agenda está activa y la
/// devuelve tal cual estaba al soltarla.
/// </summary>
/// <remarks>
/// Dos pasos, porque uno solo no alcanza: poner la barra en auto-ocultar le
/// quita a Explorer la reserva de espacio (el área de trabajo vuelve a ser la
/// pantalla entera), y esconder la ventana evita que asome al acercar el mouse
/// al borde. Explorer la vuelve a mostrar por su cuenta en algunos casos (al
/// reiniciarse, al tocar el borde), por eso un timer corto la re-esconde.
/// <para>
/// Si el proceso muere sin pasar por <see cref="Dispose"/> (kill desde el
/// administrador de tareas), la barra queda escondida hasta reiniciar Explorer.
/// </para>
/// </remarks>
internal sealed class BarraDeWindows : IDisposable
{
    private static readonly string[] Clases = ["Shell_TrayWnd", "Shell_SecondaryTrayWnd"];

    private readonly DispatcherTimer _vigilante = new() { Interval = TimeSpan.FromMilliseconds(300) };
    private readonly uint _estadoOriginal;
    private bool _liberada;

    public BarraDeWindows()
    {
        _estadoOriginal = LeerEstado();
        AppDomain.CurrentDomain.ProcessExit += AlSalir;

        Esconder();
        _vigilante.Tick += (_, _) => Esconder();
        _vigilante.Start();
    }

    private void Esconder()
    {
        if ((LeerEstado() & ABS_AUTOHIDE) == 0)
            EscribirEstado(_estadoOriginal | ABS_AUTOHIDE);

        foreach (var clase in Clases)
        {
            for (var ventana = FindWindowEx(IntPtr.Zero, IntPtr.Zero, clase, null);
                 ventana != IntPtr.Zero;
                 ventana = FindWindowEx(IntPtr.Zero, ventana, clase, null))
            {
                if (IsWindowVisible(ventana))
                    ShowWindow(ventana, SW_HIDE);
            }
        }
    }

    private void AlSalir(object? remitente, EventArgs e) => Liberar();

    private void Liberar()
    {
        if (_liberada)
            return;

        _liberada = true;
        _vigilante.Stop();
        AppDomain.CurrentDomain.ProcessExit -= AlSalir;

        EscribirEstado(_estadoOriginal);
        foreach (var clase in Clases)
        {
            for (var ventana = FindWindowEx(IntPtr.Zero, IntPtr.Zero, clase, null);
                 ventana != IntPtr.Zero;
                 ventana = FindWindowEx(IntPtr.Zero, ventana, clase, null))
            {
                ShowWindow(ventana, SW_SHOWNA);
            }
        }
    }

    public void Dispose() => Liberar();

    // ===================== interop =====================

    private static uint LeerEstado()
    {
        var datos = NuevoAppBarData();
        return SHAppBarMessage(ABM_GETSTATE, ref datos);
    }

    private static void EscribirEstado(uint estado)
    {
        var datos = NuevoAppBarData();
        datos.lParam = (IntPtr)estado;
        SHAppBarMessage(ABM_SETSTATE, ref datos);
    }

    private static APPBARDATA NuevoAppBarData() => new()
    {
        cbSize = (uint)Marshal.SizeOf<APPBARDATA>(),
        hWnd = FindWindow("Shell_TrayWnd", null),
    };

    [StructLayout(LayoutKind.Sequential)]
    private struct APPBARDATA
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public int left, top, right, bottom;
        public IntPtr lParam;
    }

    [DllImport("shell32.dll")]
    private static extern uint SHAppBarMessage(uint dwMessage, ref APPBARDATA pData);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const uint ABM_GETSTATE = 0x4;
    private const uint ABM_SETSTATE = 0xA;
    private const uint ABS_AUTOHIDE = 0x1;
    private const int SW_HIDE = 0;
    private const int SW_SHOWNA = 8;
}
