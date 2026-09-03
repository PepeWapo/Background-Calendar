using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Mantiene un grupo de ventanas fijado al fondo del z-order — justo por
/// encima del papel tapiz, por debajo de cualquier ventana de aplicación
/// normal — sin necesidad de <c>Topmost</c>, respetando un orden relativo
/// explícito entre ellas.
/// </summary>
/// <remarks>
/// Se probó primero reparentando al <c>WorkerW</c> que Explorer crea detrás
/// de los íconos del escritorio (el truco clásico de Rainmeter/Wallpaper
/// Engine) para además quedar *detrás* de los íconos reales. Se descartó:
/// en este Windows, el ListView de íconos (<c>SysListView32</c>) captura el
/// mouse en toda su área — tenga o no un ícono dibujado ahí — y se queda con
/// todos los clicks antes de que lleguen a una ventana anclada detrás suyo
/// (confirmado con <c>WindowFromPoint</c> en varios puntos del widget).
/// Este enfoque prioriza que el widget sea realmente usable (clicks, scroll)
/// por sobre ese efecto visual — más aún cuando los íconos reales terminaron
/// viviendo dentro de los rieles (<see cref="IconRailWindow"/>) en vez de
/// competir por el mismo lugar del escritorio.
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
/// </remarks>
internal sealed class AnclajeEscritorio : IDisposable
{
    /// <summary>
    /// Red de seguridad por si algo ajeno le pisa el fondo al grupo (en la
    /// máquina de prueba conviven otras apps que también anclan ahí). El
    /// reanclaje inmediato lo hacen los hooks; esto solo cubre lo que no
    /// dispare ningún evento de ventana.
    /// </summary>
    private static readonly TimeSpan IntervaloDeReafirmacion = TimeSpan.FromSeconds(10);

    private readonly Window[] _deAtrasHaciaAdelante;
    private readonly Window[] _clickThrough;
    private readonly List<IntPtr> _hooks = [];
    private readonly DispatcherTimer _timer;

    // Referencia al delegate del hook nativo: si no se guarda en algún lado,
    // el GC lo puede colectar en cualquier momento (no hay ninguna referencia
    // administrada sosteniéndolo, solo la nativa) y el callback deja de
    // dispararse silenciosamente.
    private readonly WinEventProc _alCambiarDeVentana;

    private bool _reafirmando;

    /// <param name="deAtrasHaciaAdelante">
    /// De la más al fondo (p. ej. el wallpaper animado) a la más al frente
    /// dentro de este grupo (p. ej. el widget principal, justo debajo de la
    /// taskbar y las ventanas normales).
    /// </param>
    /// <param name="clickThrough">
    /// Las ventanas del grupo que además deben dejar pasar todo click; se les
    /// reafirma en cada reanclaje junto con las que ellas mismas hayan abierto
    /// (ver <see cref="HacerClickThroughConSusVentanas"/>).
    /// </param>
    public AnclajeEscritorio(IReadOnlyList<Window> deAtrasHaciaAdelante, IReadOnlyList<Window> clickThrough)
    {
        _deAtrasHaciaAdelante = [.. deAtrasHaciaAdelante];
        _clickThrough = [.. clickThrough];

        Reafirmar();

        _alCambiarDeVentana = (_, _, _, _, _, _, _) => Reafirmar();
        Enganchar(EVENT_SYSTEM_FOREGROUND, EVENT_SYSTEM_FOREGROUND);
        Enganchar(EVENT_SYSTEM_MINIMIZESTART, EVENT_SYSTEM_MINIMIZEEND);

        _timer = new DispatcherTimer { Interval = IntervaloDeReafirmacion };
        _timer.Tick += (_, _) => Reafirmar();
        _timer.Start();
    }

    /// <summary>
    /// Devuelve al grupo al fondo del z-order y vuelve a marcar como
    /// click-through lo que corresponda.
    /// </summary>
    /// <remarks>
    /// Se reafirma tanto periódicamente como al instante cada vez que Windows
    /// cambia qué ventana está en primer plano o minimiza/restaura alguna —
    /// que es cuando en la práctica se nota un salto de z-order si se espera
    /// al próximo tick del timer.
    ///
    /// <c>SetWindowPos(HWND_BOTTOM)</c> deja a la ventana llamada *más atrás
    /// que cualquier otra*: si dos ventanas lo piden, la que lo pidió después
    /// termina debajo de la que lo pidió antes. Por eso acá se ancla en orden
    /// inverso al declarado — la última de <c>deAtrasHaciaAdelante</c> se
    /// ancla primero (queda arriba) y la primera se ancla al final (queda al
    /// fondo de todas).
    /// </remarks>
    private void Reafirmar()
    {
        // Mandar al fondo la ventana activa le saca la activación, y Windows
        // activa entonces la que quede más arriba — lo que dispara otro
        // EVENT_SYSTEM_FOREGROUND en medio de este mismo recorrido. Sin este
        // guardo los dos recorridos se entrelazan y el orden relativo del
        // grupo queda al azar: el wallpaper, que ocupa toda la pantalla, podía
        // terminar anclado por encima de los rieles y del widget y taparlos.
        if (_reafirmando)
            return;

        _reafirmando = true;
        try
        {
            for (var i = _deAtrasHaciaAdelante.Length - 1; i >= 0; i--)
                Anclar(_deAtrasHaciaAdelante[i]);

            foreach (var ventana in _clickThrough)
                HacerClickThroughConSusVentanas(ventana);
        }
        finally
        {
            _reafirmando = false;
        }
    }

    public void Dispose()
    {
        _timer.Stop();

        foreach (var hook in _hooks)
            UnhookWinEvent(hook);
        _hooks.Clear();
    }

    /// <summary>Empuja la ventana al fondo del z-order.</summary>
    public static void Anclar(Window ventana) =>
        SetWindowPos(Handle(ventana), HWND_BOTTOM, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);

    /// <summary>
    /// Saca la ventana del Alt+Tab y de cualquier otro selector de tareas.
    /// </summary>
    /// <remarks>
    /// <c>ShowInTaskbar="False"</c> de WPF solo oculta la barra de tareas —
    /// sigue apareciendo como una ventana independiente en Alt+Tab. Hay que
    /// pedirlo a mano con <c>WS_EX_TOOLWINDOW</c>. Se llama en
    /// <c>SourceInitialized</c>, apenas existe el HWND y antes de que la
    /// ventana se muestre, para que nunca llegue a aparecer en el selector
    /// ni por un instante.
    /// </remarks>
    public static void OcultarDeAltTab(Window ventana)
    {
        var hwnd = Handle(ventana);
        CambiarEstiloExtendido(hwnd, agregar: WS_EX_TOOLWINDOW, quitar: WS_EX_APPWINDOW);
    }

    /// <summary>
    /// Impide que la ventana tome el foco, sin volverla click-through: sigue
    /// recibiendo con normalidad sus propios clicks, hover y menús.
    /// </summary>
    /// <remarks>
    /// Es lo que necesitan los rieles de iconos. Al no tenerlo, un click sobre
    /// un ícono activaba la ventana del riel: Windows la reinserta al tope de
    /// su banda del z-order y dispara <c>EVENT_SYSTEM_FOREGROUND</c>, con lo
    /// que <see cref="Reafirmar"/> la mandaba de vuelta al fondo — y mandar al
    /// fondo la ventana activa le saca la activación, así que Windows activaba
    /// la siguiente y el ciclo se repetía. Ese ir y venir es el que dejaba al
    /// wallpaper (pantalla completa) tapando todo lo demás.
    ///
    /// No alcanza con <see cref="Anclar"/>: eso corrige el z-order *después*
    /// de que la activación ya ocurrió. Esto evita que ocurra.
    /// </remarks>
    public static void HacerNoActivable(Window ventana) =>
        CambiarEstiloExtendido(Handle(ventana), agregar: WS_EX_NOACTIVATE, quitar: 0);

    /// <summary>
    /// Vuelve invisibles al mouse una ventana y todas las que haya abierto:
    /// todo click, hover o scroll las atraviesa y llega a lo que esté debajo.
    /// </summary>
    /// <remarks>
    /// Pensado para el wallpaper animado, que ocupa toda la pantalla y no debe
    /// robarle ni el click derecho al escritorio ni un solo click al resto del
    /// widget.
    ///
    /// Lo de "todas las que haya abierto" no es cosmético: cada
    /// <c>VideoView</c> de LibVLCSharp.WPF crea por dentro una ventana propia
    /// de pantalla completa (su <c>ForegroundWindow</c>, pensada para dibujar
    /// contenido WPF encima del video, que acá no se usa). Esas ventanas no
    /// heredan los estilos del wallpaper y se quedaban con **todo** el input
    /// del proceso: con el wallpaper prendido no respondían ni los rieles de
    /// iconos, ni las pestañas de vista, ni el panel de Tareas. Como LibVLC
    /// las crea y las vuelve a mostrar por su cuenta, no alcanza con marcarlas
    /// una vez al arrancar: se reafirma en cada reanclaje.
    ///
    /// Suma <c>WS_EX_NOACTIVATE</c>: sin esto, cuando Windows necesita activar
    /// alguna ventana (por ejemplo al minimizar todas las demás con "mostrar
    /// escritorio"), puede elegir esta y activarla — y activar una ventana la
    /// reinserta al tope de su banda del z-order sin importar el
    /// <see cref="Anclar"/> previo. Al no poder activarse nunca, jamás salta así.
    /// </remarks>
    public static void HacerClickThroughConSusVentanas(Window ventana)
    {
        var hwnd = Handle(ventana);
        HacerClickThrough(hwnd);

        foreach (var abierta in VentanasAbiertasPor(hwnd))
            HacerClickThrough(abierta);
    }

    private static void HacerClickThrough(IntPtr hwnd) =>
        CambiarEstiloExtendido(hwnd, agregar: WS_EX_TRANSPARENT | WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW, quitar: 0);

    /// <summary>
    /// Las ventanas de este mismo hilo cuyo dueño es <paramref name="duenio"/>.
    /// </summary>
    /// <remarks>
    /// Se recorre el hilo y no todo el sistema: las ventanas que interesan las
    /// crea WPF (o una librería que corre sobre WPF) en el hilo de UI, así que
    /// enumerar el resto del escritorio sería trabajo de más en algo que corre
    /// cada vez que cambia la ventana en primer plano.
    /// </remarks>
    private static List<IntPtr> VentanasAbiertasPor(IntPtr duenio)
    {
        var encontradas = new List<IntPtr>();
        EnumThreadWindows(GetCurrentThreadId(), (hwnd, _) =>
        {
            if (GetWindow(hwnd, GW_OWNER) == duenio)
                encontradas.Add(hwnd);
            return true;
        }, IntPtr.Zero);

        return encontradas;
    }

    private static void CambiarEstiloExtendido(IntPtr hwnd, int agregar, int quitar)
    {
        var estilo = GetWindowLong(hwnd, GWL_EXSTYLE);
        var nuevo = (estilo | agregar) & ~quitar;
        if (nuevo != estilo)
            SetWindowLong(hwnd, GWL_EXSTYLE, nuevo);
    }

    private static IntPtr Handle(Window ventana) => new WindowInteropHelper(ventana).Handle;

    private void Enganchar(uint desde, uint hasta) =>
        _hooks.Add(SetWinEventHook(desde, hasta, IntPtr.Zero, _alCambiarDeVentana, 0, 0, WINEVENT_OUTOFCONTEXT));

    // ===================== interop =====================

    private delegate void WinEventProc(IntPtr hook, uint evento, IntPtr hwnd, int idObjeto, int idHijo, uint hilo, uint momento);

    private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr parametro);

    [DllImport("user32.dll")]
    private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint uFlags);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern bool EnumThreadWindows(uint dwThreadId, EnumWindowsProc lpfn, IntPtr lParam);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr hmodWinEventProc,
        WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, uint dwFlags);

    [DllImport("user32.dll")]
    private static extern bool UnhookWinEvent(IntPtr hWinEventHook);

    private static readonly IntPtr HWND_BOTTOM = new(1);
    private const uint SWP_NOSIZE = 0x0001;
    private const uint SWP_NOMOVE = 0x0002;
    private const uint SWP_NOACTIVATE = 0x0010;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const int WS_EX_NOACTIVATE = 0x08000000;
    private const uint GW_OWNER = 4;
    private const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
    private const uint EVENT_SYSTEM_MINIMIZESTART = 0x0016;
    private const uint EVENT_SYSTEM_MINIMIZEEND = 0x0017;
    private const uint WINEVENT_OUTOFCONTEXT = 0x0000;
}
