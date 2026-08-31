using System.Windows;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

internal enum LadoEscritorio { Izquierda, Derecha }

/// <summary>
/// Contenedor vertical de iconos anclado a un extremo lateral del escritorio
/// (izquierdo o derecho), un icono de ancho por N de largo, con scroll oculto
/// y resalte al pasar el mouse. Mismo mecanismo de anclaje al fondo del
/// escritorio que usa <see cref="MainWindow"/> (<see cref="AnclajeEscritorio"/>).
/// </summary>
public partial class IconRailWindow : Window
{
    private const double AnchoRiel = 88;
    private const double MargenLateral = 28;

    private readonly LadoEscritorio _lado;
    private DispatcherTimer? _reanclajeTimer;

    internal IconRailWindow(LadoEscritorio lado, IReadOnlyList<IconRailItem> iconos)
    {
        InitializeComponent();
        _lado = lado;
        IconosControl.ItemsSource = iconos;

        var area = SystemParameters.WorkArea;
        Width = AnchoRiel;
        Height = Math.Max(200, area.Height - EspacioEscritorio.MargenSuperior - EspacioEscritorio.MargenInferiorReservado);
        Top = area.Top + EspacioEscritorio.MargenSuperior;
        Left = _lado == LadoEscritorio.Izquierda
            ? area.Left + MargenLateral
            : area.Right - MargenLateral - AnchoRiel;

        SourceInitialized += (_, _) => AnclajeEscritorio.OcultarDeAltTab(this);
        Loaded += IconRailWindow_Loaded;
    }

    private void IconRailWindow_Loaded(object sender, RoutedEventArgs e)
    {
        AnclajeEscritorio.Anclar(this);
        _reanclajeTimer = AnclajeEscritorio.IniciarReanclajePeriodico(this);
    }
}
