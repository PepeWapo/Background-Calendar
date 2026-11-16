using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// La taskbar propia del widget: una barra centrada abajo, sobre el escritorio,
/// dentro del espacio que <see cref="EspacioEscritorio.MargenInferiorReservado"/>
/// venía guardando para ella.
/// </summary>
/// <remarks>
/// El menú de inicio, la bandeja y la lista de ventanas de cada programa son
/// popovers propios (<see cref="PopoverBase"/>) que se abren sobre su botón.
/// Los programas en ejecución se enumeran en vivo (<see cref="ProgramasAbiertos"/>)
/// y se dibujan en <see cref="AppsEnEjecucion"/>.
/// </remarks>
public partial class TaskbarWindow : Window
{
    /// <summary>Cada cuánto se vuelve a mirar qué ventanas hay abiertas.</summary>
    private static readonly TimeSpan IntervaloDeRefresco = TimeSpan.FromMilliseconds(800);

    private readonly ProgramasAbiertos _programas = new();
    private readonly DispatcherTimer _refresco = new() { Interval = IntervaloDeRefresco };
    private readonly Dictionary<string, ProgramaAbierto> _porClave = [];
    private string _firmaDeLaBarra = "";

    private bool _modoJuego;

    /// <summary>
    /// Se dispara al prender o apagar el modo juego; el argumento es el estado
    /// nuevo (<c>true</c> = prendido). Quien lo escucha decide qué congelar: la
    /// barra solo conoce el botón.
    /// </summary>
    internal event Action<bool>? ModoJuegoCambiado;

    internal TaskbarWindow()
    {
        InitializeComponent();

        Colocar();
        EspacioEscritorio.SeguirALaPantallaPrincipal(this, Colocar);

        SourceInitialized += (_, _) =>
        {
            AnclajeEscritorio.OcultarDeAltTab(this);
            AnclajeEscritorio.HacerNoActivable(this);
            AnclajeEscritorio.ImpedirMinimizado(this);
        };

        Loaded += (_, _) =>
        {
            // Leer los íconos del Menú Inicio tarda unos segundos: se hace ya,
            // en segundo plano, para que el primer click encuentre la lista lista.
            _ = AppsDeInicio.Obtener();
            ActualizarProgramas();
            _refresco.Start();
        };
        _refresco.Tick += (_, _) => ActualizarProgramas();
        Closed += (_, _) => _refresco.Stop();
    }

    /// <summary>
    /// La ventana ocupa todo el ancho y toda la franja reservada, y la barra se
    /// centra adentro: así crece y se re-centra con cada programa que se abre
    /// sin mover ninguna ventana. Lo que sobra es fondo nulo, así que no se ve
    /// ni recibe clicks.
    /// </summary>
    private void Colocar()
    {
        var area = SystemParameters.WorkArea;
        Width = area.Width;
        Height = EspacioEscritorio.MargenInferiorReservado;
        Left = area.Left;
        Top = area.Bottom - Height;
    }

    // ===================== programas en ejecución =====================

    /// <summary>
    /// Vuelve a leer las ventanas abiertas y reconstruye las pestañas solo si
    /// cambió algo que se ve. Reconstruir en cada tick sería innecesario y
    /// además cancelaría un click a mitad de camino.
    /// </summary>
    private void ActualizarProgramas()
    {
        var programas = _programas.Leer();

        _porClave.Clear();
        foreach (var programa in programas)
            _porClave[programa.Clave] = programa;

        // El título solo se ve en la barra cuando hay una única pestaña con
        // texto; en cualquier otro caso se lee del tooltip/popover, que siempre
        // consultan el dato fresco de _porClave.
        var conTexto = programas.Count == 1;
        var firma = string.Join(';', programas.Select(p =>
            $"{p.Clave}|{p.Ventanas.Count}|{p.EnPrimerPlano}|{p.TodasMinimizadas}|{(conTexto ? p.Ventanas[0].Titulo : "")}"));
        if (firma == _firmaDeLaBarra)
            return;

        _firmaDeLaBarra = firma;
        AppsEnEjecucion.Children.Clear();
        foreach (var programa in programas)
            AppsEnEjecucion.Children.Add(CrearPestana(programa, conTexto));

        // Mientras no haya nada, el separador de la izquierda sobra.
        SeparadorDeApps.Visibility = programas.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
    }

    /// <summary>
    /// Una pestaña por programa. Con un solo programa abierto muestra ícono y
    /// texto; con varios, solo el ícono (los nombres quedan en el tooltip o,
    /// si hay varias instancias, en la lista de <see cref="ListaVentanasPopover"/>).
    /// </summary>
    private UIElement CrearPestana(ProgramaAbierto programa, bool conTexto)
    {
        var contenido = new StackPanel { Orientation = Orientation.Horizontal };

        var icono = new Grid { Width = 20, Height = 20, VerticalAlignment = VerticalAlignment.Center };
        icono.Children.Add(new Image { Source = programa.Icono, Opacity = programa.TodasMinimizadas ? 0.55 : 1 });
        if (programa.Ventanas.Count > 1)
            icono.Children.Add(InsigniaDeCantidad(programa.Ventanas.Count));
        contenido.Children.Add(icono);

        if (conTexto)
        {
            contenido.Children.Add(new TextBlock
            {
                Text = programa.Ventanas.Count == 1 ? programa.Ventanas[0].Titulo : programa.Nombre,
                Foreground = Paleta.TextoPrimario,
                FontSize = 12,
                Margin = new Thickness(8, 0, 2, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 220,
            });
        }

        var boton = new Button { Content = contenido, Style = (Style)FindResource("TaskbarIconButtonStyle"), Margin = new Thickness(2, 0, 2, 0) };
        boton.ToolTipOpening += (_, _) => boton.ToolTip = TextoDeTooltip(programa.Clave);
        boton.ToolTip = TextoDeTooltip(programa.Clave);
        boton.Click += (_, _) => AlClickearPestana(programa.Clave, boton);

        // Línea de acento debajo de la pestaña del programa que tiene el foco.
        var raiz = new Grid();
        raiz.Children.Add(boton);
        raiz.Children.Add(new Border
        {
            Height = 2,
            Width = 14,
            CornerRadius = new CornerRadius(1),
            VerticalAlignment = VerticalAlignment.Bottom,
            Margin = new Thickness(0, 0, 0, 1),
            Background = Paleta.AcentoFlexible,
            IsHitTestVisible = false,
            Visibility = programa.EnPrimerPlano ? Visibility.Visible : Visibility.Collapsed,
        });
        return raiz;
    }

    private static UIElement InsigniaDeCantidad(int cantidad) => new Border
    {
        MinWidth = 11,
        Height = 11,
        CornerRadius = new CornerRadius(6),
        Background = Paleta.AcentoFlexible,
        HorizontalAlignment = HorizontalAlignment.Right,
        VerticalAlignment = VerticalAlignment.Bottom,
        Margin = new Thickness(0, 0, -5, -4),
        Child = new TextBlock
        {
            Text = cantidad.ToString(),
            FontSize = 8,
            FontWeight = FontWeights.Bold,
            Foreground = Brushes.Black,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
        },
    };

    private string TextoDeTooltip(string clave)
    {
        if (!_porClave.TryGetValue(clave, out var programa))
            return "";

        return programa.Ventanas.Count == 1 ? programa.Ventanas[0].Titulo : $"{programa.Nombre} ({programa.Ventanas.Count} ventanas)";
    }

    private void AlClickearPestana(string clave, FrameworkElement boton)
    {
        PopoverBase.CerrarAbierto();
        if (!_porClave.TryGetValue(clave, out var programa))
            return;

        if (programa.Ventanas.Count == 1)
            ProgramasAbiertos.AlternarVentana(programa.Ventanas[0]);
        else
            PopoverBase.Alternar(clave, boton, () => new ListaVentanasPopover(programa));
    }

    // ===================== botones fijos =====================

    /// <summary>
    /// Modo juego: congela el video del wallpaper para que no gaste GPU ni CPU
    /// mientras se juega. El ícono queda resaltado mientras está prendido.
    /// </summary>
    private void BotonModoJuego_Click(object sender, RoutedEventArgs e)
    {
        _modoJuego = !_modoJuego;
        IconoModoJuego.Stroke = _modoJuego ? (Brush)FindResource("AccentFlexible") : (Brush)FindResource("TextMuted");
        BotonModoJuego.ToolTip = _modoJuego ? "Modo juego: activado (clic para reanudar el fondo)" : "Modo juego: congela el fondo animado";
        ModoJuegoCambiado?.Invoke(_modoJuego);
    }

    private void BotonVistaDeTareas_Click(object sender, RoutedEventArgs e)
    {
        PopoverBase.CerrarAbierto();
        AccionesDeSistema.AbrirVistaDeTareas();
    }

    private void BotonSuspender_Click(object sender, RoutedEventArgs e) => AccionesDeSistema.Suspender();

    private void BotonReiniciar_Click(object sender, RoutedEventArgs e) => AccionesDeSistema.Reiniciar();

    private void BotonApagar_Click(object sender, RoutedEventArgs e) => AccionesDeSistema.Apagar();

    private void BotonCerrarSesion_Click(object sender, RoutedEventArgs e) => AccionesDeSistema.CerrarSesion();

    /// <summary>Abre el menú de inicio propio sobre el botón.</summary>
    private void BotonMenuWindows_Click(object sender, RoutedEventArgs e) =>
        PopoverBase.Alternar("inicio", BotonMenuWindows, () => new MenuInicioPopover());

    private void BotonExplorador_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo("explorer.exe") { UseShellExecute = true });
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // no hay mucho más para hacer si ni Explorer arranca.
        }
    }

    /// <summary>
    /// Abre la bandeja propia sobre el botón: los íconos del área de
    /// notificación de Windows en un contenedor del proyecto.
    /// </summary>
    private void BotonBandeja_Click(object sender, RoutedEventArgs e) =>
        PopoverBase.Alternar("bandeja", BotonBandeja, () => new BandejaPopover());
}
