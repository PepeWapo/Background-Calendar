using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Media;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>Una ventana de aplicación abierta, tal como la ve la taskbar.</summary>
internal sealed record VentanaAbierta(IntPtr Hwnd, string Titulo, bool Minimizada, bool EnPrimerPlano);

/// <summary>
/// Un programa en ejecución: todas las ventanas abiertas del mismo ejecutable
/// agrupadas en una sola pestaña.
/// </summary>
internal sealed record ProgramaAbierto(string Clave, string Nombre, ImageSource? Icono, IReadOnlyList<VentanaAbierta> Ventanas)
{
    public bool EnPrimerPlano => Ventanas.Any(v => v.EnPrimerPlano);

    public bool TodasMinimizadas => Ventanas.All(v => v.Minimizada);
}

/// <summary>
/// Enumera las ventanas que Windows muestra como botones en su barra de tareas
/// y las agrupa por programa; también sabe activarlas.
/// </summary>
/// <remarks>
/// Es la misma regla que usa Explorer (la clásica de "Alt+Tab": visible, sin
/// dueño, no <c>TOOLWINDOW</c>, con título), más el filtro de ventanas
/// <c>cloaked</c> que DWM marca para las apps UWP suspendidas y las de otros
/// escritorios virtuales — sin eso aparecerían "fantasmas" de Configuración,
/// Calculadora, etc. que no están realmente a la vista.
///
/// El orden de las pestañas es el de aparición, no el del z-order: si no, cada
/// cambio de foco las reordenaría debajo del mouse.
/// </remarks>
internal sealed class ProgramasAbiertos
{
    private readonly Dictionary<uint, string?> _exePorProceso = [];
    private readonly Dictionary<string, (string Nombre, ImageSource? Icono)> _datosPorExe = [];

    // Orden de aparición de programas y de ventanas (la más vieja primero).
    private readonly List<string> _ordenProgramas = [];
    private readonly Dictionary<IntPtr, long> _ordenVentanas = [];
    private long _contador;

    public IReadOnlyList<ProgramaAbierto> Leer()
    {
        var primerPlano = GetForegroundWindow();
        var porPrograma = new Dictionary<string, List<VentanaAbierta>>();
        var vistas = new HashSet<IntPtr>();

        EnumWindows((hwnd, _) =>
        {
            if (!EsVentanaDeAplicacion(hwnd))
                return true;

            var exe = ExeDe(hwnd);
            if (exe is null)
                return true;

            var titulo = TituloDe(hwnd);
            if (!_ordenVentanas.ContainsKey(hwnd))
                _ordenVentanas[hwnd] = _contador++;

            vistas.Add(hwnd);
            var clave = exe.ToLowerInvariant();
            if (!porPrograma.TryGetValue(clave, out var lista))
                porPrograma[clave] = lista = [];

            lista.Add(new VentanaAbierta(hwnd, titulo, IsIconic(hwnd), hwnd == primerPlano));
            return true;
        }, IntPtr.Zero);

        foreach (var vieja in _ordenVentanas.Keys.Where(h => !vistas.Contains(h)).ToList())
            _ordenVentanas.Remove(vieja);

        _ordenProgramas.RemoveAll(c => !porPrograma.ContainsKey(c));
        foreach (var clave in porPrograma.Keys)
        {
            if (!_ordenProgramas.Contains(clave))
                _ordenProgramas.Add(clave);
        }

        return _ordenProgramas.Select(clave =>
        {
            var ventanas = porPrograma[clave].OrderBy(v => _ordenVentanas[v.Hwnd]).ToList();
            var (nombre, icono) = DatosDe(clave);
            return new ProgramaAbierto(clave, nombre, icono, ventanas);
        }).ToList();
    }

    /// <summary>
    /// Trae la ventana al frente; si ya estaba al frente y a la vista, la minimiza
    /// (el mismo ida y vuelta del botón de Windows).
    /// </summary>
    public static void AlternarVentana(VentanaAbierta ventana)
    {
        var hwnd = ventana.Hwnd;
        if (IsIconic(hwnd))
        {
            ShowWindow(hwnd, SW_RESTORE);
        }
        else if (GetForegroundWindow() == hwnd)
        {
            ShowWindow(hwnd, SW_MINIMIZE);
            return;
        }

        Traer(hwnd);
    }

    /// <summary>Trae la ventana al frente, restaurándola si estaba minimizada.</summary>
    public static void Activar(IntPtr hwnd)
    {
        if (IsIconic(hwnd))
            ShowWindow(hwnd, SW_RESTORE);

        Traer(hwnd);
    }

    private static void Traer(IntPtr hwnd)
    {
        // Windows solo deja pasar al frente a quien acaba de recibir input; una
        // pulsación de Alt "inocua" lo habilita aun si el foco anterior era de
        // otro proceso (la taskbar es NOACTIVATE y nunca lo tuvo).
        keybd_event(VK_MENU, 0, 0, UIntPtr.Zero);
        keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        SetForegroundWindow(hwnd);
    }

    // ===================== filtro =====================

    private bool EsVentanaDeAplicacion(IntPtr hwnd)
    {
        if (!IsWindowVisible(hwnd))
            return false;

        if (DwmGetWindowAttribute(hwnd, DWMWA_CLOAKED, out var oculta, sizeof(int)) == 0 && oculta != 0)
            return false;

        var estilo = GetWindowLong(hwnd, GWL_EXSTYLE);
        var tieneDuenio = GetWindow(hwnd, GW_OWNER) != IntPtr.Zero;
        if ((estilo & WS_EX_TOOLWINDOW) != 0 && (estilo & WS_EX_APPWINDOW) == 0)
            return false;
        if (tieneDuenio && (estilo & WS_EX_APPWINDOW) == 0)
            return false;

        if (GetWindowTextLength(hwnd) == 0)
            return false;

        GetWindowThreadProcessId(hwnd, out var proceso);
        if (proceso == (uint)Environment.ProcessId)
            return false;

        var clase = ClaseDe(hwnd);
        return clase is not ("Progman" or "WorkerW" or "Shell_TrayWnd" or "Shell_SecondaryTrayWnd" or "Windows.UI.Core.CoreWindow");
    }

    // ===================== datos del programa =====================

    /// <summary>
    /// El ejecutable real de la ventana. Las apps UWP viven dentro de
    /// <c>ApplicationFrameHost</c>: el programa de verdad es el de la
    /// <c>CoreWindow</c> hija.
    /// </summary>
    private string? ExeDe(IntPtr hwnd)
    {
        GetWindowThreadProcessId(hwnd, out var proceso);
        var exe = ExePorProceso(proceso);

        if (exe is not null && Path.GetFileName(exe).Equals("ApplicationFrameHost.exe", StringComparison.OrdinalIgnoreCase))
        {
            uint real = 0;
            EnumChildWindows(hwnd, (hijo, _) =>
            {
                GetWindowThreadProcessId(hijo, out var p);
                if (p != proceso && ClaseDe(hijo) == "Windows.UI.Core.CoreWindow")
                {
                    real = p;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            if (real != 0)
                exe = ExePorProceso(real) ?? exe;
        }

        return exe;
    }

    private string? ExePorProceso(uint proceso)
    {
        if (_exePorProceso.TryGetValue(proceso, out var cacheada))
            return cacheada;

        string? ruta = null;
        var manejador = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, proceso);
        if (manejador != IntPtr.Zero)
        {
            try
            {
                var buffer = new StringBuilder(1024);
                var tamano = buffer.Capacity;
                if (QueryFullProcessImageName(manejador, 0, buffer, ref tamano))
                    ruta = buffer.ToString();
            }
            finally
            {
                CloseHandle(manejador);
            }
        }

        _exePorProceso[proceso] = ruta;
        return ruta;
    }

    private (string Nombre, ImageSource? Icono) DatosDe(string exe)
    {
        if (_datosPorExe.TryGetValue(exe, out var datos))
            return datos;

        string nombre;
        try
        {
            nombre = FileVersionInfo.GetVersionInfo(exe).FileDescription is { Length: > 0 } descripcion
                ? descripcion
                : Path.GetFileNameWithoutExtension(exe);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            nombre = Path.GetFileNameWithoutExtension(exe);
        }

        datos = (nombre, AccesosDirectos.ExtraerIcono(exe));
        _datosPorExe[exe] = datos;
        return datos;
    }

    private static string TituloDe(IntPtr hwnd)
    {
        var buffer = new StringBuilder(GetWindowTextLength(hwnd) + 1);
        GetWindowText(hwnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    private static string ClaseDe(IntPtr hwnd)
    {
        var buffer = new StringBuilder(128);
        GetClassName(hwnd, buffer, buffer.Capacity);
        return buffer.ToString();
    }

    // ===================== interop =====================

    private delegate bool EnumWindowsProc(IntPtr hwnd, IntPtr parametro);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool EnumChildWindows(IntPtr hwndPadre, EnumWindowsProc lpfn, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsIconic(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindow(IntPtr hWnd, uint uCmd);

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("dwmapi.dll")]
    private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out int pvAttribute, int cbAttribute);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern bool QueryFullProcessImageName(IntPtr hProcess, uint dwFlags, StringBuilder lpExeName, ref int lpdwSize);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    private const int SW_MINIMIZE = 6;
    private const int SW_RESTORE = 9;
    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TOOLWINDOW = 0x00000080;
    private const int WS_EX_APPWINDOW = 0x00040000;
    private const uint GW_OWNER = 4;
    private const int DWMWA_CLOAKED = 14;
    private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
    private const byte VK_MENU = 0x12;
    private const uint KEYEVENTF_KEYUP = 0x0002;
}
