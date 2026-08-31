using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Ancla una ventana a la capa del escritorio (mismo truco que usan Rainmeter
/// y Wallpaper Engine): la reparenta al <c>WorkerW</c> que Explorer crea
/// detrás de los iconos del escritorio. Con eso la ventana queda siempre por
/// debajo de cualquier ventana de aplicación normal y por encima del papel
/// tapiz/iconos, sin necesidad de <c>Topmost</c> ni de pelear el z-order a
/// mano. Lo usan tanto <see cref="MainWindow"/> como <see cref="IconRailWindow"/>.
/// </summary>
internal static class AnclajeEscritorio
{
    private const uint WM_SPAWN_WORKER = 0x052C;

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindow(string? lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags, uint uTimeout, out IntPtr lpdwResult);

    [DllImport("user32.dll")]
    private static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

    [DllImport("user32.dll")]
    private static extern IntPtr GetParent(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOZORDER = 0x0004;
    private const uint SWP_FRAMECHANGED = 0x0020;
    private const int SW_HIDE = 0;
    private const int SW_SHOW = 5;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    public static void Anclar(Window ventana)
    {
        var workerW = ObtenerWorkerWDelEscritorio();
        if (workerW == IntPtr.Zero)
            return;

        var hwnd = new WindowInteropHelper(ventana).Handle;
        if (GetParent(hwnd) == workerW)
            return; // ya está anclada; evita un SetParent (y el parpadeo) de más

        SetParent(hwnd, workerW);
        SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        ForzarRepintado(ventana);
    }

    /// <summary>
    /// Ocultar y volver a mostrar una ventana reparentada fuerza a DWM a
    /// reconectar su superficie de composición. Sin esto, una ventana con
    /// capas (<c>AllowsTransparency</c>) recién anclada al escritorio queda
    /// "invisible" en pantalla aunque Win32 la reporte visible y en la
    /// posición correcta — más notorio cuando hay varias ventanas
    /// ancladas seguidas, donde la última en anclarse pierde la reconexión.
    /// </summary>
    public static void ForzarRepintado(Window ventana)
    {
        var hwnd = new WindowInteropHelper(ventana).Handle;
        ShowWindow(hwnd, SW_HIDE);
        ShowWindow(hwnd, SW_SHOW);
    }

    /// <summary>
    /// Saca la ventana del Alt+Tab y de cualquier otro selector de tareas.
    /// <c>ShowInTaskbar="False"</c> de WPF solo oculta la barra de tareas —
    /// sigue apareciendo como una ventana independiente en Alt+Tab (y, al
    /// estar ancladas al escritorio, no deberían aparecer ahí como si fueran
    /// aplicaciones sueltas). Hay que pedirlo a mano con
    /// <c>WS_EX_TOOLWINDOW</c>. Se llama en <c>SourceInitialized</c>, apenas
    /// existe el HWND y antes de que la ventana se muestre, para que nunca
    /// llegue a aparecer en el selector ni por un instante.
    /// </summary>
    public static void OcultarDeAltTab(Window ventana)
    {
        var hwnd = new WindowInteropHelper(ventana).Handle;
        var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
    }

    /// <summary>
    /// Si Explorer reinicia (crash, "Reiniciar" desde el Administrador de
    /// tareas), Windows recrea el WorkerW y la ventana queda huérfana —
    /// reintentar el anclaje cada tanto la recupera sola.
    /// </summary>
    public static DispatcherTimer IniciarReanclajePeriodico(Window ventana)
    {
        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10),
        };
        timer.Tick += (_, _) => Anclar(ventana);
        timer.Start();
        return timer;
    }

    /// <summary>
    /// Progman entiende un mensaje no documentado que le pide separar los
    /// iconos del escritorio en su propio <c>WorkerW</c> — es exactamente lo
    /// que hacen Rainmeter y Wallpaper Engine para poder vivir "detrás" de
    /// las ventanas normales sin tocar el explorer.exe real.
    /// </summary>
    private static IntPtr ObtenerWorkerWDelEscritorio()
    {
        var progman = FindWindow("Progman", null);
        SendMessageTimeout(progman, WM_SPAWN_WORKER, IntPtr.Zero, IntPtr.Zero, 0, 1000, out _);

        var workerW = IntPtr.Zero;
        EnumWindows((topHandle, _) =>
        {
            var defView = FindWindowEx(topHandle, IntPtr.Zero, "SHELLDLL_DefView", null);
            if (defView != IntPtr.Zero)
                workerW = FindWindowEx(IntPtr.Zero, topHandle, "WorkerW", null);

            return true;
        }, IntPtr.Zero);

        return workerW;
    }
}
