using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PepeWapOSx10.Shell.Widgets;

internal enum LadoEscritorio { Izquierda, Derecha }

/// <summary>
/// Contenedor vertical de iconos anclado a un extremo lateral del escritorio
/// (izquierdo o derecho), un icono de ancho por N de largo, con scroll oculto
/// y resalte al pasar el mouse. El anclaje al fondo del z-order lo orquesta
/// <see cref="MainWindow"/> junto con el resto de las ventanas de escritorio
/// (<see cref="AnclajeEscritorio"/>), para mantener un orden relativo fijo
/// entre todas.
///
/// Click selecciona (selección nativa del <c>ListBox</c>), doble click
/// ejecuta el acceso directo, y arrastrar un ícono lo reordena dentro del
/// mismo riel o lo muda al otro — <see cref="RielIconos"/> persiste el
/// resultado para que sobreviva al próximo arranque.
/// </summary>
public partial class IconRailWindow : Window
{
    private const double AnchoRiel = 88;
    private const double MargenLateral = 28;

    private readonly LadoEscritorio _lado;
    private readonly ObservableCollection<IconRailItem> _iconos;
    private IconRailWindow? _otroRiel;

    private Point _puntoDeInicio;
    private IconRailItem? _itemPresionado;

    internal IconRailWindow(LadoEscritorio lado, IEnumerable<IconRailItem> iconos)
    {
        InitializeComponent();
        _lado = lado;
        _iconos = new ObservableCollection<IconRailItem>(iconos);
        IconosControl.ItemsSource = _iconos;

        var area = SystemParameters.WorkArea;
        Width = AnchoRiel;
        Height = Math.Max(200, area.Height - EspacioEscritorio.MargenSuperior - EspacioEscritorio.MargenInferiorReservado);
        Top = area.Top + EspacioEscritorio.MargenSuperior;
        Left = _lado == LadoEscritorio.Izquierda
            ? area.Left + MargenLateral
            : area.Right - MargenLateral - AnchoRiel;

        SourceInitialized += (_, _) => AnclajeEscritorio.OcultarDeAltTab(this);
    }

    /// <summary>Se llama después de construir los dos rieles, para que cada uno sepa a dónde mudar un ícono arrastrado.</summary>
    internal void ConectarConElOtroRiel(IconRailWindow otroRiel) => _otroRiel = otroRiel;

    private void IconosControl_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (IconosControl.SelectedItem is not IconRailItem item)
            return;

        try
        {
            Process.Start(new ProcessStartInfo(item.Target) { UseShellExecute = true });
        }
        catch
        {
            // el acceso directo puede apuntar a algo desinstalado o movido —
            // no hay mucho más para hacer que no intentarlo.
        }
    }

    private void IconosControl_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _puntoDeInicio = e.GetPosition(null);
        _itemPresionado = (e.OriginalSource as FrameworkElement)?.DataContext as IconRailItem;
    }

    private void IconosControl_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _itemPresionado is null)
            return;

        var actual = e.GetPosition(null);
        if (Math.Abs(actual.X - _puntoDeInicio.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(actual.Y - _puntoDeInicio.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        var item = _itemPresionado;
        _itemPresionado = null;
        DragDrop.DoDragDrop(IconosControl, new IconRailDragData(item, _iconos), DragDropEffects.Move);
    }

    private void IconosControl_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(IconRailDragData)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void IconosControl_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(IconRailDragData)) is not IconRailDragData datos)
            return;

        var origenEraEsteRiel = ReferenceEquals(datos.Origen, _iconos);
        var indiceOrigen = datos.Origen.IndexOf(datos.Item);
        var indiceDestino = CalcularIndiceDestino(e.GetPosition(IconosControl));

        datos.Origen.Remove(datos.Item);
        if (origenEraEsteRiel && indiceOrigen < indiceDestino)
            indiceDestino--; // se corrió un lugar al sacarlo de más arriba en la misma lista.

        _iconos.Insert(Math.Clamp(indiceDestino, 0, _iconos.Count), datos.Item);

        var (izquierda, derecha) = _lado == LadoEscritorio.Izquierda ? (_iconos, _otroRiel?._iconos) : (_otroRiel?._iconos, _iconos);
        if (izquierda is not null && derecha is not null)
            RielIconos.GuardarOrden(izquierda, derecha);

        e.Handled = true;
    }

    private int CalcularIndiceDestino(Point posicionEnLista)
    {
        for (var i = 0; i < _iconos.Count; i++)
        {
            if (IconosControl.ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem contenedor)
                continue;

            var centro = contenedor.TranslatePoint(new Point(0, contenedor.ActualHeight / 2), IconosControl);
            if (posicionEnLista.Y < centro.Y)
                return i;
        }
        return _iconos.Count;
    }

    private sealed record IconRailDragData(IconRailItem Item, ObservableCollection<IconRailItem> Origen);
}
