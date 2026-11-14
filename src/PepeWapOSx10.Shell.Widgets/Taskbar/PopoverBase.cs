using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Ventana flotante que se abre sobre un botón de la taskbar, dentro de un
/// contenedor con los estilos del proyecto (el mismo tono de los menús
/// contextuales), y se cierra al perder el foco.
/// </summary>
/// <remarks>
/// Es una ventana propia y no un <see cref="System.Windows.Controls.Primitives.Popup"/>
/// porque la taskbar es <c>NOACTIVATE</c> y vive al fondo del z-order: un
/// Popup hereda esa posición y el menú de inicio necesita foco de teclado para
/// el buscador. Esta ventana es <c>Topmost</c> y activable.
///
/// Solo hay un popover a la vez, y volver a tocar el botón que lo abrió lo
/// cierra: el click sobre la taskbar no le saca el foco (la taskbar nunca lo
/// toma), así que ese ida y vuelta lo resuelve <see cref="Alternar"/>.
/// </remarks>
internal abstract class PopoverBase : Window
{
    private const double MargenSombra = 14;
    private const double SeparacionDelBoton = 8;

    private static PopoverBase? _abierto;

    private readonly object _clave;
    private readonly Border _raiz;
    private bool _cerrando;

    protected PopoverBase(object clave, UIElement contenido)
    {
        _clave = clave;

        WindowStyle = WindowStyle.None;
        AllowsTransparency = true;
        Background = null;
        ResizeMode = ResizeMode.NoResize;
        ShowInTaskbar = false;
        Topmost = true;
        SizeToContent = SizeToContent.WidthAndHeight;
        WindowStartupLocation = WindowStartupLocation.Manual;

        _raiz = new Border
        {
            Margin = new Thickness(MargenSombra),
            Padding = new Thickness(8),
            CornerRadius = new CornerRadius(14),
            Background = (Brush)Application.Current.Resources["MenuBackground"],
            BorderBrush = (Brush)Application.Current.Resources["MenuBorder"],
            BorderThickness = new Thickness(1),
            Effect = new DropShadowEffect { BlurRadius = 18, ShadowDepth = 2, Opacity = 0.45, Color = Colors.Black },
            Child = contenido,
        };
        Content = _raiz;

        SourceInitialized += (_, _) => AnclajeEscritorio.OcultarDeAltTab(this);
        Deactivated += (_, _) => Cerrar();
        Closed += (_, _) =>
        {
            if (_abierto == this)
                _abierto = null;
        };
        PreviewKeyDown += (_, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Escape)
                Cerrar();
        };
    }

    /// <summary>
    /// Abre el popover creado por <paramref name="crear"/> sobre
    /// <paramref name="ancla"/>; si ya había uno abierto con la misma
    /// <paramref name="clave"/> lo cierra y no abre nada (segundo click en el botón).
    /// </summary>
    public static void Alternar(object clave, FrameworkElement ancla, Func<PopoverBase> crear)
    {
        var abierto = _abierto;
        abierto?.Cerrar();
        if (abierto is not null && Equals(abierto._clave, clave))
            return;

        var nuevo = crear();
        _abierto = nuevo;
        nuevo.MostrarSobre(ancla);
    }

    /// <summary>Cierra el popover abierto, si hay uno.</summary>
    public static void CerrarAbierto() => _abierto?.Cerrar();

    protected void Cerrar()
    {
        if (_cerrando)
            return;

        _cerrando = true;
        Close();
    }

    /// <summary>
    /// Se llama con la ventana ya visible y activa: un buen lugar para
    /// darle el foco a un campo de texto.
    /// </summary>
    protected virtual void AlMostrar()
    {
    }

    private void MostrarSobre(FrameworkElement ancla)
    {
        // La posición se calcula antes de mostrar, con el tamaño que va a tener:
        // si no, la ventana aparece un cuadro en (0,0) y salta.
        _raiz.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        var tamano = _raiz.DesiredSize;

        var origen = PresentationSource.FromVisual(ancla)?.CompositionTarget?.TransformFromDevice ?? Matrix.Identity;
        var arribaIzquierda = origen.Transform(ancla.PointToScreen(new Point(0, 0)));
        var abajoDerecha = origen.Transform(ancla.PointToScreen(new Point(ancla.ActualWidth, ancla.ActualHeight)));

        var area = SystemParameters.WorkArea;
        var centro = (arribaIzquierda.X + abajoDerecha.X) / 2;
        Left = Math.Clamp(centro - tamano.Width / 2, area.Left, Math.Max(area.Left, area.Right - tamano.Width));
        // El borde visible del contenedor (sin el margen de la sombra) queda a
        // SeparacionDelBoton por encima del botón.
        Top = arribaIzquierda.Y - SeparacionDelBoton + MargenSombra - tamano.Height;

        Show();
        Activate();
        AlMostrar();
    }
}
