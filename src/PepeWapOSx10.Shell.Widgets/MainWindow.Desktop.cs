using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

public partial class MainWindow
{
    private DispatcherTimer? _reanclajeTimer;

    private void AnclarAlEscritorio() => AnclajeEscritorio.Anclar(this);

    private void IniciarReanclajePeriodico() =>
        _reanclajeTimer = AnclajeEscritorio.IniciarReanclajePeriodico(this);
}
