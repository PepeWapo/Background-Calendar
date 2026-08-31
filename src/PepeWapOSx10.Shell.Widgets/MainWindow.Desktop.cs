using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Ancla la ventana a la capa del escritorio (mismo truco que usan Rainmeter
/// y Wallpaper Engine): la reparenta al <c>WorkerW</c> que Explorer crea
/// detrás de los iconos del escritorio. Con eso el widget queda siempre por
/// debajo de cualquier ventana de aplicación normal y por encima del papel
/// tapiz/iconos, sin necesidad de <c>Topmost</c> ni de pelear el z-order a
/// mano.
/// </summary>
public partial class MainWindow
{
    private const uint WM_SPAWN_WORKER = 0x052C;

    private DispatcherTimer? _reanclajeTimer;

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
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    private void AnclarAlEscritorio()
    {
        var workerW = ObtenerWorkerWDelEscritorio();
        if (workerW == IntPtr.Zero)
            return;

        var miHwnd = new WindowInteropHelper(this).Handle;
        if (GetParent(miHwnd) == workerW)
            return; // ya está anclado; evita un SetParent (y el parpadeo) de más

        SetParent(miHwnd, workerW);
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

    /// <summary>
    /// Si Explorer reinicia (crash, "Reiniciar" desde el Administrador de
    /// tareas), Windows recrea el WorkerW y la ventana queda huérfana —
    /// reintentar el anclaje cada tanto la recupera sola.
    /// </summary>
    private void IniciarReanclajePeriodico()
    {
        _reanclajeTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10),
        };
        _reanclajeTimer.Tick += (_, _) => AnclarAlEscritorio();
        _reanclajeTimer.Start();
    }
}
