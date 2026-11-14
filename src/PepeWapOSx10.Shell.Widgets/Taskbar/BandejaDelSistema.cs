using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>Un ícono del área de notificación de Windows.</summary>
/// <param name="Toolbar">El ToolbarWindow32 de Explorer donde vive el ícono.</param>
/// <param name="Indice">Posición del botón dentro de esa barra.</param>
internal sealed record IconoDeBandeja(string Texto, ImageSource? Icono, IntPtr Toolbar, int Indice, bool EnLaBarra);

/// <summary>
/// Lee los íconos del área de notificación que Explorer mantiene y les
/// reenvía los clicks, para mostrarlos en un contenedor propio.
/// </summary>
/// <remarks>
/// Windows no tiene API pública para esto: los íconos viven como botones de
/// dos <c>ToolbarWindow32</c> dentro de Explorer (los visibles en la barra y los
/// del desborde), y cada botón guarda un puntero a una estructura interna
/// (<c>TRAYDATA</c>: ventana dueña, id, mensaje, <c>HICON</c>). Se leen con
/// <c>TB_GETBUTTON</c> sobre memoria reservada dentro del proceso de Explorer —
/// el truco clásico de las réplicas de bandeja.
///
/// Los clicks <b>no</b> se mandan a la ventana dueña con el mensaje de
/// callback: según la versión del protocolo (<c>NOTIFYICON_VERSION_4</c> o no)
/// cambia el formato de wParam/lParam, y Explorer es el único que la sabe. En
/// cambio se le simula el click al botón de la barra de Explorer y es él quien
/// arma la notificación correcta — por eso la barra real tiene que seguir viva
/// (hoy lo está: la taskbar propia convive con la de Windows).
///
/// Es frágil por construcción: depende de la estructura interna de Windows 10.
/// Si no encuentra algo, devuelve una lista vacía en vez de fallar.
/// </remarks>
internal static class BandejaDelSistema
{
    public static IReadOnlyList<IconoDeBandeja> Leer()
    {
        var iconos = new List<IconoDeBandeja>();
        foreach (var (toolbar, enLaBarra) in BuscarBarras())
        {
            try
            {
                LeerBarra(toolbar, enLaBarra, iconos);
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
            {
                // una barra ilegible no tiene que tirar abajo a la otra.
            }
        }

        return iconos;
    }

    /// <summary>
    /// Hace un click de verdad (mueve el cursor, clickea y lo devuelve) sobre el
    /// ícono de la bandeja real.
    /// </summary>
    /// <remarks>
    /// Se probó antes mandarle el click al <c>ToolbarWindow32</c> con
    /// <c>WM_LBUTTONDOWN/UP</c> (con y sin <c>WM_MOUSEMOVE</c>, con
    /// <c>PostMessage</c> y <c>SendMessage</c>): los mensajes llegan pero
    /// Explorer no dispara nada, porque el control espera input real. Y mandar
    /// el mensaje de callback directo a la ventana dueña obliga a conocer la
    /// versión del protocolo del ícono (<c>NOTIFYICON_VERSION_4</c> o no), que
    /// solo Explorer sabe. Un click físico lo resuelve él.
    ///
    /// Los íconos del desborde solo existen en pantalla con su flyout abierto:
    /// se lo abre antes (el mismo botón de la flecha que usaba la versión
    /// anterior de la taskbar) y se espera a que aparezca. Por eso es
    /// <c>async</c>.
    /// </remarks>
    public static async Task Click(IconoDeBandeja icono, bool derecho)
    {
        if (!icono.EnLaBarra)
        {
            AbrirDesborde();
            await Task.Delay(300);
        }

        var rect = RectDelBoton(icono);
        if (rect is null)
            return;

        var punto = new Punto { X = (rect.Value.Left + rect.Value.Right) / 2, Y = (rect.Value.Top + rect.Value.Bottom) / 2 };
        if (!ClientToScreen(icono.Toolbar, ref punto))
            return;

        GetCursorPos(out var anterior);
        SetCursorPos(punto.X, punto.Y);
        await Task.Delay(60);

        mouse_event(derecho ? MOUSEEVENTF_RIGHTDOWN : MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
        mouse_event(derecho ? MOUSEEVENTF_RIGHTUP : MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);

        // El cursor vuelve a donde estaba, salvo que sea un click derecho: el
        // menú que abre la app se posiciona donde esté el cursor al soltar.
        await Task.Delay(120);
        if (!derecho)
            SetCursorPos(anterior.X, anterior.Y);
    }

    private static void AbrirDesborde()
    {
        var barra = FindWindow("Shell_TrayWnd", null);
        var notificacion = FindWindowEx(barra, IntPtr.Zero, "TrayNotifyWnd", null);
        if (notificacion == IntPtr.Zero)
            return;

        // 1502: id histórico del botón "Mostrar íconos ocultos" (Windows no lo
        // documenta); si no está, el único "Button" que haya ahí.
        var flecha = GetDlgItem(notificacion, 1502);
        if (flecha == IntPtr.Zero)
            flecha = FindWindowEx(notificacion, IntPtr.Zero, "Button", null);
        if (flecha != IntPtr.Zero)
            SendMessage(flecha, BM_CLICK, IntPtr.Zero, IntPtr.Zero);
    }

    // ===================== lectura =====================

    private static IEnumerable<(IntPtr Toolbar, bool EnLaBarra)> BuscarBarras()
    {
        var barra = FindWindow("Shell_TrayWnd", null);
        if (barra != IntPtr.Zero)
        {
            var notificacion = FindWindowEx(barra, IntPtr.Zero, "TrayNotifyWnd", null);
            var paginador = FindWindowEx(notificacion, IntPtr.Zero, "SysPager", null);
            var visibles = FindWindowEx(paginador != IntPtr.Zero ? paginador : notificacion, IntPtr.Zero, "ToolbarWindow32", null);
            if (visibles != IntPtr.Zero)
                yield return (visibles, true);
        }

        var desborde = FindWindow("NotifyIconOverflowWindow", null);
        var oculta = FindWindowEx(desborde, IntPtr.Zero, "ToolbarWindow32", null);
        if (oculta != IntPtr.Zero)
            yield return (oculta, false);
    }

    private static void LeerBarra(IntPtr toolbar, bool enLaBarra, List<IconoDeBandeja> destino)
    {
        GetWindowThreadProcessId(toolbar, out var proceso);
        var explorer = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, proceso);
        if (explorer == IntPtr.Zero)
            return;

        // Memoria remota para que Explorer escriba la respuesta de cada mensaje.
        var remoto = VirtualAllocEx(explorer, IntPtr.Zero, 4096, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        if (remoto == IntPtr.Zero)
        {
            CloseHandle(explorer);
            return;
        }

        try
        {
            var cantidad = (int)SendMessage(toolbar, TB_BUTTONCOUNT, IntPtr.Zero, IntPtr.Zero);
            for (var i = 0; i < cantidad; i++)
            {
                if (SendMessage(toolbar, TB_GETBUTTON, i, remoto) == IntPtr.Zero)
                    continue;

                var boton = Leer(explorer, remoto, 32);
                if (boton is null)
                    continue;

                var idComando = BitConverter.ToInt32(boton, 4);
                var estado = boton[8];
                if ((estado & TBSTATE_HIDDEN) != 0)
                    continue;

                var datos = Leer(explorer, (IntPtr)BitConverter.ToInt64(boton, 16), 40);
                if (datos is null)
                    continue;

                var ventanaDuenia = (IntPtr)BitConverter.ToInt64(datos, 0);
                var hIcono = (IntPtr)BitConverter.ToInt64(datos, 24);

                var texto = TextoDelBoton(toolbar, explorer, remoto, idComando);
                if (string.IsNullOrWhiteSpace(texto))
                    texto = NombreDelProceso(ventanaDuenia);

                destino.Add(new IconoDeBandeja(texto, Convertir(hIcono), toolbar, i, enLaBarra));
            }
        }
        finally
        {
            VirtualFreeEx(explorer, remoto, 0, MEM_RELEASE);
            CloseHandle(explorer);
        }
    }

    private static string TextoDelBoton(IntPtr toolbar, IntPtr explorer, IntPtr remoto, int idComando)
    {
        var largo = (int)SendMessage(toolbar, TB_GETBUTTONTEXTW, idComando, remoto);
        if (largo <= 0)
            return "";

        var bytes = Leer(explorer, remoto, Math.Min(largo * 2, 1024));
        return bytes is null ? "" : Encoding.Unicode.GetString(bytes).TrimEnd('\0');
    }

    private static string NombreDelProceso(IntPtr ventana)
    {
        if (ventana == IntPtr.Zero)
            return "";

        GetWindowThreadProcessId(ventana, out var proceso);
        try
        {
            return Process.GetProcessById((int)proceso).ProcessName;
        }
        catch (ArgumentException)
        {
            return "";
        }
    }

    /// <summary>
    /// Los HICON son handles globales de la sesión: el de Explorer sirve tal
    /// cual en este proceso, no hace falta copiarlo.
    /// </summary>
    private static ImageSource? Convertir(IntPtr hIcono)
    {
        if (hIcono == IntPtr.Zero)
            return null;

        try
        {
            var imagen = Imaging.CreateBitmapSourceFromHIcon(hIcono, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            imagen.Freeze();
            return imagen;
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or COMException)
        {
            // el ícono puede haberse destruido entre la lectura y acá.
            return null;
        }
    }

    private static (int Left, int Top, int Right, int Bottom)? RectDelBoton(IconoDeBandeja icono)
    {
        GetWindowThreadProcessId(icono.Toolbar, out var proceso);
        var explorer = OpenProcess(PROCESS_VM_OPERATION | PROCESS_VM_READ | PROCESS_QUERY_INFORMATION, false, proceso);
        if (explorer == IntPtr.Zero)
            return null;

        var remoto = VirtualAllocEx(explorer, IntPtr.Zero, 4096, MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);
        try
        {
            if (remoto == IntPtr.Zero || SendMessage(icono.Toolbar, TB_GETITEMRECT, icono.Indice, remoto) == IntPtr.Zero)
                return null;

            var bytes = Leer(explorer, remoto, 16);
            return bytes is null
                ? null
                : (BitConverter.ToInt32(bytes, 0), BitConverter.ToInt32(bytes, 4), BitConverter.ToInt32(bytes, 8), BitConverter.ToInt32(bytes, 12));
        }
        finally
        {
            if (remoto != IntPtr.Zero)
                VirtualFreeEx(explorer, remoto, 0, MEM_RELEASE);
            CloseHandle(explorer);
        }
    }

    private static byte[]? Leer(IntPtr proceso, IntPtr direccion, int cantidad)
    {
        var bytes = new byte[cantidad];
        return ReadProcessMemory(proceso, direccion, bytes, cantidad, out var leidos) && leidos == cantidad ? bytes : null;
    }

    // ===================== interop =====================

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindow(string lpClassName, string? lpWindowName);

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string? lpszWindow);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, int wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDlgItem(IntPtr hDlg, int nIDDlgItem);

    [DllImport("user32.dll")]
    private static extern bool ClientToScreen(IntPtr hWnd, ref Punto lpPoint);

    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out Punto lpPoint);

    [DllImport("user32.dll")]
    private static extern bool SetCursorPos(int x, int y);

    [DllImport("user32.dll")]
    private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

    [StructLayout(LayoutKind.Sequential)]
    private struct Punto
    {
        public int X;
        public int Y;
    }

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

    [DllImport("kernel32.dll")]
    private static extern IntPtr OpenProcess(uint dwDesiredAccess, bool bInheritHandle, uint dwProcessId);

    [DllImport("kernel32.dll")]
    private static extern bool CloseHandle(IntPtr hObject);

    [DllImport("kernel32.dll")]
    private static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

    [DllImport("kernel32.dll")]
    private static extern bool VirtualFreeEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint dwFreeType);

    [DllImport("kernel32.dll")]
    private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int nSize, out int lpNumberOfBytesRead);

    private const uint TB_GETBUTTON = 0x0417;
    private const uint TB_BUTTONCOUNT = 0x0418;
    private const uint TB_GETITEMRECT = 0x041D;
    private const uint TB_GETBUTTONTEXTW = 0x044B;
    private const byte TBSTATE_HIDDEN = 0x08;

    private const uint BM_CLICK = 0x00F5;
    private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
    private const uint MOUSEEVENTF_LEFTUP = 0x0004;
    private const uint MOUSEEVENTF_RIGHTDOWN = 0x0008;
    private const uint MOUSEEVENTF_RIGHTUP = 0x0010;

    private const uint PROCESS_VM_OPERATION = 0x0008;
    private const uint PROCESS_VM_READ = 0x0010;
    private const uint PROCESS_QUERY_INFORMATION = 0x0400;
    private const uint MEM_COMMIT = 0x1000;
    private const uint MEM_RESERVE = 0x2000;
    private const uint MEM_RELEASE = 0x8000;
    private const uint PAGE_READWRITE = 0x04;
}
