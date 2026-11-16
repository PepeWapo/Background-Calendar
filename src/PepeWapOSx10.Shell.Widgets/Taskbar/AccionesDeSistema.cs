using System.Diagnostics;
using System.Runtime.InteropServices;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Acciones de Windows que disparan los botones de la taskbar y del menú de
/// inicio: energía, sesión y vista de tareas. Un solo lugar para que los dos
/// sitios hagan exactamente lo mismo.
/// </summary>
internal static class AccionesDeSistema
{
    private const byte VkTab = 0x09;
    private const byte VkLWin = 0x5B;
    private const uint KeyEventKeyUp = 0x0002;

    private const uint MbYesNo = 0x4;
    private const uint MbIconQuestion = 0x20;
    private const uint MbDefButton2 = 0x100;
    private const uint MbSetForeground = 0x10000;
    private const uint MbTopMost = 0x40000;
    private const int IdYes = 6;

    [DllImport("user32.dll", CharSet = CharSet.Unicode)]
    private static extern int MessageBoxW(IntPtr hWnd, string texto, string titulo, uint tipo);

    [DllImport("powrprof.dll", SetLastError = true)]
    private static extern bool SetSuspendState(bool hibernar, bool forzar, bool deshabilitarEventosDeDespertar);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte vk, byte scan, uint flags, UIntPtr extra);

    /// <summary>
    /// Suspende (no hiberna). Se llama a la API directo: vía <c>rundll32</c> Windows
    /// hiberna si la hibernación está activada, sin importar los argumentos.
    /// </summary>
    internal static void Suspender()
    {
        if (Confirmar("¿Suspender el equipo?"))
            SetSuspendState(false, false, false);
    }

    internal static void Reiniciar()
    {
        if (Confirmar("¿Reiniciar el equipo?"))
            Ejecutar("shutdown.exe", "/r /t 0");
    }

    internal static void Apagar()
    {
        if (Confirmar("¿Apagar el equipo?"))
            Ejecutar("shutdown.exe", "/s /t 0");
    }

    internal static void CerrarSesion()
    {
        if (Confirmar("¿Cerrar la sesión de Windows?"))
            Ejecutar("shutdown.exe", "/l");
    }

    /// <summary>
    /// Pregunta Sí/No antes de una acción que corta el trabajo. Se usa el
    /// MessageBox nativo con TOPMOST: la taskbar es no activable y está al
    /// fondo del z-order, y un diálogo con ella de dueña quedaría escondido.
    /// </summary>
    private static bool Confirmar(string pregunta) =>
        MessageBoxW(IntPtr.Zero, pregunta, "PepeWapOSx10", MbYesNo | MbIconQuestion | MbDefButton2 | MbTopMost | MbSetForeground) == IdYes;

    /// <summary>
    /// Abre la vista de tareas de Windows (la misma que Win+Tab) simulando el atajo:
    /// no hay API pública para abrirla.
    /// </summary>
    internal static void AbrirVistaDeTareas()
    {
        keybd_event(VkLWin, 0, 0, UIntPtr.Zero);
        keybd_event(VkTab, 0, 0, UIntPtr.Zero);
        keybd_event(VkTab, 0, KeyEventKeyUp, UIntPtr.Zero);
        keybd_event(VkLWin, 0, KeyEventKeyUp, UIntPtr.Zero);
    }

    private static void Ejecutar(string archivo, string argumentos)
    {
        try
        {
            Process.Start(new ProcessStartInfo(archivo, argumentos) { UseShellExecute = false, CreateNoWindow = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // sin permiso o sin el ejecutable: no hay nada razonable que hacer desde acá.
        }
    }
}
