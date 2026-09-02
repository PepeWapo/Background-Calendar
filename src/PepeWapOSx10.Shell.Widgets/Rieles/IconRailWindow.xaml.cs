using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Contenedor vertical de iconos anclado a un extremo lateral del escritorio
/// (izquierdo o derecho), un icono de ancho por N de largo, con scroll oculto
/// y resalte al pasar el mouse.
/// </summary>
/// <remarks>
/// Muestra un lado de <see cref="RielesDeIconos"/>: un click selecciona, dos
/// ejecutan el acceso directo, y arrastrar un ícono lo reordena dentro del
/// mismo riel o lo muda al otro. El anclaje al fondo del z-order lo orquesta
/// <see cref="EscritorioShell"/> junto con el resto de las ventanas de
/// escritorio, para mantener un orden relativo fijo entre todas.
/// </remarks>
public partial class IconRailWindow : Window
{
    private const double AnchoRiel = 88;
    private const double MargenLateral = 28;

    /// <summary>Alto mínimo, por si el área de trabajo es tan chica que los márgenes se la comen.</summary>
    private const double AltoMinimo = 200;

    private readonly LadoEscritorio _lado;
    private readonly RielesDeIconos _rieles;
    private readonly DetectorDobleClick _dobleClick = new();

    private Point _puntoDeInicio;
    private IconRailItem? _itemPresionado;
    private bool _arrastrando;

    internal IconRailWindow(LadoEscritorio lado, RielesDeIconos rieles)
    {
        InitializeComponent();
        _lado = lado;
        _rieles = rieles;
        IconosControl.ItemsSource = rieles.De(lado);

        var area = SystemParameters.WorkArea;
        Width = AnchoRiel;
        Height = Math.Max(AltoMinimo, area.Height - EspacioEscritorio.MargenSuperior - EspacioEscritorio.MargenInferiorReservado);
        Top = area.Top + EspacioEscritorio.MargenSuperior;
        Left = lado == LadoEscritorio.Izquierda
            ? area.Left + MargenLateral
            : area.Right - MargenLateral - AnchoRiel;

        SourceInitialized += (_, _) => AnclajeEscritorio.OcultarDeAltTab(this);
    }

    private void Iconos_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        _puntoDeInicio = e.GetPosition(null);
        _itemPresionado = (e.OriginalSource as FrameworkElement)?.DataContext as IconRailItem;
        _arrastrando = false;
    }

    private void Iconos_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed || _itemPresionado is null)
            return;

        var actual = e.GetPosition(null);
        if (Math.Abs(actual.X - _puntoDeInicio.X) < SystemParameters.MinimumHorizontalDragDistance &&
            Math.Abs(actual.Y - _puntoDeInicio.Y) < SystemParameters.MinimumVerticalDragDistance)
            return;

        var item = _itemPresionado;
        _itemPresionado = null;
        _arrastrando = true;
        DragDrop.DoDragDrop(IconosControl, item, DragDropEffects.Move);
    }

    /// <summary>
    /// Dos clicks seguidos sobre el mismo ícono lo ejecutan.
    /// </summary>
    /// <remarks>
    /// Se detecta el doble click por cuenta propia
    /// (<see cref="DetectorDobleClick"/>) en vez de usar el
    /// <c>MouseDoubleClick</c> del <c>ListBox</c>: acá el primer click además
    /// activa la ventana (el riel vive al fondo del z-order y nunca tiene el
    /// foco) y reancla todo el grupo, y en el medio de eso WPF perdía la
    /// cuenta y le asignaba <c>ClickCount = 1</c> también al segundo click, así
    /// que el ícono no arrancaba nunca.
    /// </remarks>
    private void Iconos_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        // El botón que se suelta al terminar un arrastre no es un click.
        if (_arrastrando)
        {
            _arrastrando = false;
            return;
        }

        if ((e.OriginalSource as FrameworkElement)?.DataContext is not IconRailItem item)
            return;

        if (_dobleClick.EsDoble(item.Target))
            Ejecutar(item);
    }

    private static void Ejecutar(IconRailItem item)
    {
        try
        {
            Process.Start(new ProcessStartInfo(item.Target) { UseShellExecute = true });
        }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        {
            // el acceso directo puede apuntar a algo desinstalado o movido —
            // no hay mucho más para hacer que no intentarlo.
        }
    }

    private void Iconos_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(typeof(IconRailItem)) ? DragDropEffects.Move : DragDropEffects.None;
        e.Handled = true;
    }

    private void Iconos_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(typeof(IconRailItem)) is not IconRailItem item)
            return;

        _rieles.Mover(item, _lado, IndiceDestino(e.GetPosition(IconosControl)));
        e.Handled = true;
    }

    /// <summary>En qué posición de este riel cae el punto soltado.</summary>
    private int IndiceDestino(Point posicionEnLista)
    {
        var iconos = _rieles.De(_lado);

        for (var i = 0; i < iconos.Count; i++)
        {
            if (IconosControl.ItemContainerGenerator.ContainerFromIndex(i) is not ListBoxItem contenedor)
                continue;

            var centro = contenedor.TranslatePoint(new Point(0, contenedor.ActualHeight / 2), IconosControl);
            if (posicionEnLista.Y < centro.Y)
                return i;
        }

        return iconos.Count;
    }
}
