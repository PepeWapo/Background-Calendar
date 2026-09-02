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
///
/// También se evaluó reparentar el wallpaper animado (que sí es completamente
/// click-through, así que en teoría no necesitaba recibir clicks) al mismo
/// <c>WorkerW</c> en blanco que usan Rainmeter/Wallpaper Engine. Se descartó
/// por dos motivos, en dos intentos separados: (1) sin convertir la ventana a
/// <c>WS_CHILD</c> de verdad (sacándole <c>WS_POPUP</c>, forzando
/// <c>SWP_FRAMECHANGED</c>), DWM la seguía componiendo como ventana top-level
/// independiente y tapaba todo; (2) corregido eso, en este Windows
/// <c>SHELLDLL_DefView</c> (los íconos) resultó ser hijo directo de
/// <c>Progman</c> — el mensaje <c>WM_SPAWN_WORKERW</c> no separa los íconos a
/// su propio <c>WorkerW</c> como debería, un comportamiento de Explorer que
/// varió entre builds de Windows y quedó roto en esta instalación. Por ahora
/// el wallpaper animado se resigna a tapar los íconos reales del escritorio.
/// </summary>
internal static class AnclajeEscritorio
{
    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    private delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    private static readonly IntPtr HWND_BOTTOM = new(1);
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
    private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;

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
    /// Vuelve la ventana invisible al mouse: todo click, hover o scroll la
    /// atraviesa y llega a lo que esté debajo (el escritorio real). Pensado
    /// para el wallpaper animado, que ocupa toda la pantalla y no debe robarle
    /// el click derecho (menú contextual del escritorio) a Explorer.
    ///
    /// Suma <c>WS_EX_NOACTIVATE</c>: sin esto, cuando Windows necesita
    /// activar alguna ventana (por ejemplo al minimizar todas las demás con
    /// "mostrar escritorio"), puede elegir esta y activarla — y activar una
    /// ventana la reinserta al tope de su banda del z-order sin importar el
    /// <see cref="Anclar"/> previo, dejándola por encima de todo hasta el
    /// próximo reanclaje. Al no poder activarse nunca, jamás puede saltar así.
    /// </summary>
    public static void HacerClickThrough(Window ventana)
    {
        var hwnd = new WindowInteropHelper(ventana).Handle;
        var exStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, exStyle | WS_EX_TRANSPARENT | WS_EX_NOACTIVATE);
    }

    // Referencias a los delegates de los hooks nativos: si no se guardan en
    // algún lado, el GC los puede colectar en cualquier momento (no hay
    // ninguna referencia administrada sosteniéndolos, solo la nativa) y el
    // callback deja de dispararse silenciosamente.
    private static readonly List<WinEventProc> _callbacksDeHooksActivos = [];

    /// <summary>
    /// Reafirma el fondo del z-order para un grupo de ventanas, respetando un
    /// orden relativo explícito entre ellas, tanto periódicamente (cada 10s,
    /// red de seguridad por si algo ajeno les pisa el fondo) como al instante
    /// cada vez que Windows cambia qué ventana está en primer plano o
    /// minimiza/restaura alguna — que es cuando en la práctica se nota un
    /// salto de z-order si se espera al próximo tick del timer.
    /// </summary>
    /// <remarks>
    /// <c>SetWindowPos(HWND_BOTTOM)</c> deja a la ventana llamada *más atrás
    /// que cualquier otra*: si dos ventanas lo piden, la que lo pidió después
    /// termina debajo de la que lo pidió antes. Por eso acá se ancla en
    /// orden inverso al declarado — la última de <paramref name="ventanasDeAtrasHaciaAdelante"/>
    /// se ancla primero (queda arriba) y la primera se ancla al final
    /// (queda al fondo de todas).
    /// </remarks>
    /// <param name="ventanasDeAtrasHaciaAdelante">
    /// De la más al fondo (p.ej. el wallpaper animado) a la más al frente
    /// dentro de este grupo (p.ej. el widget principal, justo debajo de la
    /// taskbar y las ventanas normales).
    /// </param>
    public static DispatcherTimer IniciarReanclajePeriodico(params Window[] ventanasDeAtrasHaciaAdelante)
    {
        void ReanclarTodas()
        {
            for (var i = ventanasDeAtrasHaciaAdelante.Length - 1; i >= 0; i--)
                Anclar(ventanasDeAtrasHaciaAdelante[i]);
        }

        ReanclarTodas();

        WinEventProc alCambiar = (_, _, _, _, _, _, _) => ReanclarTodas();
        _callbacksDeHooksActivos.Add(alCambiar);
        SetWinEventHook(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, alCambiar, 0, 0, WINEVENT_OUTOFCONTEXT);
        SetWinEventHook(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZEEND, IntPtr.Zero, alCambiar, 0, 0, WINEVENT_OUTOFCONTEXT);

        var timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(10),
        };
        timer.Tick += (_, _) => ReanclarTodas();
        timer.Start();
        return timer;
    }
}
