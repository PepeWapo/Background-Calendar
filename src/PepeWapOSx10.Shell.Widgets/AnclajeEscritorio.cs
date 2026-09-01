using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Fija una ventana al fondo del z-order — justo por encima del papel
/// tapiz, por debajo de cualquier ventana de aplicación normal — sin
/// necesidad de <c>Topmost</c>.
///
/// Se probó primero reparentando al <c>WorkerW</c> que Explorer crea detrás
/// de los íconos del escritorio (el truco clásico de Rainmeter/Wallpaper
/// Engine) para además quedar *detrás* de los íconos reales. Se descartó:
/// en este Windows, el ListView de íconos (<c>SysListView32</c>) captura el
/// mouse en toda su área — tenga o no un ícono dibujado ahí — y se queda con
/// todos los clicks antes de que lleguen a una ventana anclada detrás suyo
/// (confirmado con <c>WindowFromPoint</c> en varios puntos del widget).
/// Este enfoque prioriza que el widget sea realmente usable (clicks, scroll)
/// por sobre ese efecto visual — más aún cuando los íconos reales van a
/// terminar viviendo dentro de los rieles (<see cref="IconRailWindow"/>) en
/// vez de competir por el mismo lugar del escritorio.
/// </summary>
internal static class AnclajeEscritorio
{
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    private static readonly IntPtr HWND_BOTTOM = new(1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;

    /// <summary>
    /// Empuja la ventana al fondo del z-order. Otras ventanas que también
    /// se fijan ahí (una wallpaper animada, por ejemplo) pueden reclamar esa
    /// posición en cualquier momento — por eso hace falta reafirmarlo
    /// periódicamente, no solo una vez al arrancar.
    /// </summary>
    public static void Anclar(Window ventana)
    {
        var hwnd = new WindowInteropHelper(ventana).Handle;
        SetWindowPos(hwnd, HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    /// <summary>
    /// Saca la ventana del Alt+Tab y de cualquier otro selector de tareas.
    /// <c>ShowInTaskbar="False"</c> de WPF solo oculta la barra de tareas —
    /// sigue apareciendo como una ventana independiente en Alt+Tab. Hay que
    /// pedirlo a mano con <c>WS_EX_TOOLWINDOW</c>. Se llama en
    /// <c>SourceInitialized</c>, apenas existe el HWND y antes de que la
    /// ventana se muestre, para que nunca llegue a aparecer en el selector
    /// ni por un instante.
    /// </summary>
    public static void OcultarDeAltTab(Window ventana)
    {
        var hwnd = new WindowInteropHelper(ventana).Handle;
        var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, (exStyle | WS_EX_TOOLWINDOW) & ~WS_EX_APPWINDOW);
    }

    /// <summary>
    /// Reafirma la posición al fondo del z-order cada tanto, por si otra
    /// ventana (wallpaper animada, otra instancia, etc.) la desplaza.
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
}
