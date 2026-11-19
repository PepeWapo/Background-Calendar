using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Intercepta la tecla Windows suelta (apretar y soltar sin otra tecla en el
/// medio) para abrir el menú de inicio propio en vez del de Windows.
/// </summary>
/// <remarks>
/// Las combinaciones (Win+D, Win+E, Win+Tab…) pasan intactas. Para que Windows
/// no abra su menú al soltar la tecla se inyecta una tecla sin asignar
/// (<c>VK 0xFF</c>) antes del KeyUp: Windows cree que hubo una combinación y
/// cancela el menú, y el KeyUp real sigue su camino, así que la tecla no queda
/// "pegada".
/// </remarks>
internal sealed class TeclaWindows : IDisposable
{
    private readonly Action _alSoltarSola;
    private readonly Dispatcher _dispatcher = Dispatcher.CurrentDispatcher;
    // El delegado se guarda en un campo: si el GC lo junta, el hook revienta.
    private readonly LowLevelKeyboardProc _callback;
    private IntPtr _hook;
    private bool _winApretada;
    private bool _winSolo;

    public TeclaWindows(Action alSoltarSola)
    {
        _alSoltarSola = alSoltarSola;
        _callback = Procesar;

        using var proceso = Process.GetCurrentProcess();
        using var modulo = proceso.MainModule!;
        _hook = SetWindowsHookEx(WH_KEYBOARD_LL, _callback, GetModuleHandle(modulo.ModuleName), 0);
    }

    private IntPtr Procesar(int codigo, IntPtr wParam, IntPtr lParam)
    {
        if (codigo >= 0)
        {
            var tecla = Marshal.PtrToStructure<KBDLLHOOKSTRUCT>(lParam);
            var inyectada = (tecla.flags & LLKHF_INJECTED) != 0;

            if (!inyectada)
            {
                var abajo = (int)wParam is WM_KEYDOWN or WM_SYSKEYDOWN;
                var esWin = tecla.vkCode is VK_LWIN or VK_RWIN;

                if (esWin && abajo)
                {
                    // El auto-repeat manda KeyDown repetidos: solo el primero arma.
                    if (!_winApretada)
                    {
                        _winApretada = true;
                        _winSolo = true;
                    }
                }
                else if (esWin)
                {
                    _winApretada = false;
                    if (_winSolo)
                    {
                        _winSolo = false;
                        keybd_event(VK_SIN_ASIGNAR, 0, 0, UIntPtr.Zero);
                        keybd_event(VK_SIN_ASIGNAR, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
                        _dispatcher.BeginInvoke(_alSoltarSola);
                    }
                }
                else if (abajo)
                {
                    _winSolo = false;
                }
            }
        }

        return CallNextHookEx(_hook, codigo, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hook == IntPtr.Zero)
            return;

        UnhookWindowsHookEx(_hook);
        _hook = IntPtr.Zero;
    }

    // ===================== interop =====================

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    private struct KBDLLHOOKSTRUCT
    {
        public uint vkCode;
        public uint scanCode;
        public uint flags;
        public uint time;
        public IntPtr dwExtraInfo;
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll")]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll")]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);

    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private const uint LLKHF_INJECTED = 0x10;
    private const uint VK_LWIN = 0x5B;
    private const uint VK_RWIN = 0x5C;
    private const byte VK_SIN_ASIGNAR = 0xFF;
    private const uint KEYEVENTF_KEYUP = 0x0002;
}
