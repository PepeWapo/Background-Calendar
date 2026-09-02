using System.Windows;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

public partial class MainWindow
{
    private DispatcherTimer? _reanclajeTimer;

    /// <summary>
    /// Ancla y reancla periódicamente, en un único orden fijo, todas las
    /// ventanas de escritorio que ya existan a esta altura (el wallpaper
    /// animado puede no existir si no hay carpeta configurada). De atrás
    /// hacia adelante: wallpaper animado, rieles, este widget — justo debajo
    /// de la taskbar y las ventanas normales, que no participan de este
    /// anclaje y siempre quedan por encima.
    /// </summary>
    private void AnclarTodoElEscritorio()
    {
        Window[] ventanasDeAtrasHaciaAdelante = _wallpaper is null
            ? [_rielIzquierdo!, _rielDerecho!, this]
            : [_wallpaper, _rielIzquierdo!, _rielDerecho!, this];

        _reanclajeTimer = AnclajeEscritorio.IniciarReanclajePeriodico(ventanasDeAtrasHaciaAdelante);
    }
}
