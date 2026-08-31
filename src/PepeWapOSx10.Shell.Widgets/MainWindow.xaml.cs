using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PepeWapOSx10.Datos;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

public partial class MainWindow : Window
{
    private enum Vista { Dia, Semana, Mes }

    private Vista _vista = Vista.Dia;
    private DateOnly _semanaInicio = InicioDeSemana(DateOnly.FromDateTime(DateTime.Today));
    private DateOnly _mesActual = new(DateTime.Today.Year, DateTime.Today.Month, 1);
    private DateOnly? _diaResaltado;
    private DateOnly? _semanaResaltadaInicio;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        // DragMove() lanza InvalidOperationException si el botón primario ya
        // no está apretado cuando llega acá (puede pasar si otro handler
        // demoró el despacho del evento).
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    /// <summary>
    /// Marca un elemento como interactivo frente al arrastre de la ventana.
    /// </summary>
    /// <remarks>
    /// El <c>Grid</c> raíz maneja <c>MouseLeftButtonDown</c> para permitir
    /// arrastrar el widget desde cualquier zona vacía. <see cref="Window.DragMove"/>
    /// entra en un loop modal de movimiento que captura el mouse y consume el
    /// <c>MouseLeftButtonUp</c> siguiente, por lo que los handlers de click de
    /// las celdas (que viven en el evento de <em>up</em>) nunca se ejecutaban.
    /// Marcar el <em>down</em> como manejado evita que llegue a la raíz.
    ///
    /// Se engancha al evento burbujeante y no al <c>Preview</c>: el de vista
    /// previa tunelea desde la raíz hacia abajo, así que un contenedor le
    /// robaría el mouse-down a los botones que tenga adentro (el ✎ de la guía)
    /// y nunca dispararían su <c>Click</c>. Burbujeando, los controles que ya
    /// manejan el down —botones— lo consumen primero, y este handler solo actúa
    /// cuando el click cayó en el fondo del contenedor.
    /// </remarks>
    private static void HabilitarClick(FrameworkElement elemento) =>
        elemento.MouseLeftButtonDown += (_, e) => e.Handled = true;

    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern uint GetDoubleClickTime();

    private string? _ultimoClickClave;
    private DateTime _ultimoClickInstante = DateTime.MinValue;

    /// <summary>
    /// Detecta el doble click por cuenta propia, sin usar
    /// <see cref="MouseButtonEventArgs.ClickCount"/>.
    /// </summary>
    /// <remarks>
    /// El primer click resalta, y resaltar vuelve a construir las celdas de la
    /// vista. WPF no le asigna <c>ClickCount = 2</c> a un click que cae sobre un
    /// elemento distinto del anterior, así que el segundo click de un doble
    /// click siempre llegaba como <c>ClickCount = 1</c> y nunca navegaba.
    /// </remarks>
    /// <param name="clave">Identidad lógica de lo clickeado (la celda, no el control).</param>
    private bool EsDobleClick(string clave)
    {
        var ahora = DateTime.UtcNow;
        var esDoble = _ultimoClickClave == clave
                      && (ahora - _ultimoClickInstante).TotalMilliseconds <= GetDoubleClickTime();

        // Un doble click cierra la secuencia: el tercer click vuelve a contar como simple.
        _ultimoClickClave = esDoble ? null : clave;
        _ultimoClickInstante = esDoble ? DateTime.MinValue : ahora;
        return esDoble;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BuildCalendar(DateTime.Today);
        await CargarGuiaAsync();
        await CargarDiaAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    // ===================== navegación entre vistas =====================

    private void TabVista_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton { Tag: string tag })
            return;

        var nueva = tag switch
        {
            "Semana" => Vista.Semana,
            "Mes" => Vista.Mes,
            _ => Vista.Dia,
        };

        _ = CambiarVistaAsync(nueva);
    }

    private void NavPrev_Click(object sender, RoutedEventArgs e)
    {
        _diaResaltado = null;
        _semanaResaltadaInicio = null;

        if (_vista == Vista.Semana)
        {
            _semanaInicio = _semanaInicio.AddDays(-7);
            _ = CargarSemanaAsync(_semanaInicio);
        }
        else if (_vista == Vista.Mes)
        {
            _mesActual = _mesActual.AddMonths(-1);
            _ = CargarMesAsync(_mesActual);
        }
    }

    private void NavNext_Click(object sender, RoutedEventArgs e)
    {
        _diaResaltado = null;
        _semanaResaltadaInicio = null;

        if (_vista == Vista.Semana)
        {
            _semanaInicio = _semanaInicio.AddDays(7);
            _ = CargarSemanaAsync(_semanaInicio);
        }
        else if (_vista == Vista.Mes)
        {
            _mesActual = _mesActual.AddMonths(1);
            _ = CargarMesAsync(_mesActual);
        }
    }

    private void NavHoy_Click(object sender, RoutedEventArgs e)
    {
        _diaResaltado = null;
        _semanaResaltadaInicio = null;

        if (_vista == Vista.Semana)
        {
            _semanaInicio = InicioDeSemana(DateOnly.FromDateTime(DateTime.Today));
            _ = CargarSemanaAsync(_semanaInicio);
        }
        else if (_vista == Vista.Mes)
        {
            _mesActual = new DateOnly(DateTime.Today.Year, DateTime.Today.Month, 1);
            _ = CargarMesAsync(_mesActual);
        }
    }

    private async Task CambiarVistaAsync(Vista vista, DateOnly? fechaDia = null, DateOnly? semanaInicio = null)
    {
        _vista = vista;
        _diaResaltado = null;
        _semanaResaltadaInicio = null;
        ActualizarUiDeVista(vista);

        switch (vista)
        {
            case Vista.Dia:
                await CargarDiaAsync(fechaDia ?? DateOnly.FromDateTime(DateTime.Today));
                break;
            case Vista.Semana:
                if (semanaInicio is { } s)
                    _semanaInicio = s;
                await CargarSemanaAsync(_semanaInicio);
                break;
            case Vista.Mes:
                await CargarMesAsync(_mesActual);
                break;
        }
    }

    private void ActualizarUiDeVista(Vista vista)
    {
        TabDia.IsChecked = vista == Vista.Dia;
        TabSemana.IsChecked = vista == Vista.Semana;
        TabMes.IsChecked = vista == Vista.Mes;

        DiaScroll.Visibility = vista == Vista.Dia ? Visibility.Visible : Visibility.Collapsed;
        SemanaPanel.Visibility = vista == Vista.Semana ? Visibility.Visible : Visibility.Collapsed;
        MesPanel.Visibility = vista == Vista.Mes ? Visibility.Visible : Visibility.Collapsed;

        AgendaNavRow.Visibility = vista == Vista.Dia ? Visibility.Collapsed : Visibility.Visible;
        NavHintText.Text = vista switch
        {
            Vista.Semana => "click resalta el día · doble click abre su vista Día",
            Vista.Mes => "click resalta día/semana · doble click navega",
            _ => string.Empty,
        };
    }

    private static DateOnly InicioDeSemana(DateOnly fecha) => fecha.AddDays(-((int)fecha.DayOfWeek + 6) % 7);

    // ===================== CALENDARIO (mini panel izquierdo) =====================

    private void BuildCalendar(DateTime hoy)
    {
        var cultura = new CultureInfo("es-AR");
        var nombreMes = cultura.DateTimeFormat.GetMonthName(hoy.Month);
        CalendarMonthText.Text = char.ToUpper(nombreMes[0]) + nombreMes[1..];
        CalendarYearText.Text = hoy.Year.ToString();

        CalendarDaysGrid.Children.Clear();
        CalendarDaysGrid.RowDefinitions.Clear();
        CalendarDaysGrid.ColumnDefinitions.Clear();

        for (var i = 0; i < 7; i++)
            CalendarDaysGrid.ColumnDefinitions.Add(new ColumnDefinition());

        CalendarDaysGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var encabezados = new[] { "L", "M", "M", "J", "V", "S", "D" };
        for (var i = 0; i < 7; i++)
        {
            var encabezado = new TextBlock
            {
                Text = encabezados[i],
                FontSize = 10.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextMuted"),
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 0, 0, 7),
            };
            Grid.SetRow(encabezado, 0);
            Grid.SetColumn(encabezado, i);
            CalendarDaysGrid.Children.Add(encabezado);
        }

        var primerDiaMes = new DateTime(hoy.Year, hoy.Month, 1);
        var offset = ((int)primerDiaMes.DayOfWeek + 6) % 7;
        var diasEnMes = DateTime.DaysInMonth(hoy.Year, hoy.Month);
        var filas = (int)Math.Ceiling((offset + diasEnMes) / 7.0);

        for (var f = 0; f < filas; f++)
            CalendarDaysGrid.RowDefinitions.Add(new RowDefinition());

        for (var dia = 1; dia <= diasEnMes; dia++)
        {
            var indice = offset + dia - 1;
            var fila = indice / 7;
            var columna = indice % 7;
            var esHoy = dia == hoy.Day;

            FrameworkElement celda = esHoy
                ? new Border
                {
                    Width = 26,
                    Height = 26,
                    CornerRadius = new CornerRadius(13),
                    Background = (Brush)FindResource("AccentFlexible"),
                    Child = new TextBlock
                    {
                        Text = dia.ToString(),
                        FontSize = 12,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                }
                : new TextBlock
                {
                    Text = dia.ToString(),
                    FontSize = 12,
                    Foreground = (Brush)FindResource("TextPrimary"),
                    Width = 26,
                    Height = 26,
                    TextAlignment = TextAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                };

            Grid.SetRow(celda, fila + 1);
            Grid.SetColumn(celda, columna);
            CalendarDaysGrid.Children.Add(celda);
        }

        var nombreDia = cultura.DateTimeFormat.GetDayName(hoy.DayOfWeek);
        CalendarTodayText.Text = $"{char.ToUpper(nombreDia[0]) + nombreDia[1..]} {hoy.Day}";
    }

    // ===================== GUIA (TareaGuia) =====================
    // La guía siempre refleja "hoy" real, independientemente de qué fecha
    // esté mirando la vista Día/Semana/Mes — por eso se carga una sola vez
    // en Window_Loaded y no depende de la fecha navegada.

    private SqliteTareaGuiaRepository? _repoGuia;
    private IReadOnlyList<TareaGuia> _tareasGuia = [];

    private async Task CargarGuiaAsync()
    {
        try
        {
            var contexto = new AgendaDbContext();
            contexto.Inicializar();
            _repoGuia = new SqliteTareaGuiaRepository(contexto);
            await RefrescarGuiaAsync();
        }
        catch (Exception ex)
        {
            // El panel de guía no es crítico para la agenda, pero fallar en
            // silencio escondía problemas de esquema/migración.
            GuiaSummaryText.Text = $"No se pudo cargar la guía: {ex.Message}";
        }
    }

    private async Task RefrescarGuiaAsync()
    {
        if (_repoGuia is null)
            return;

        _tareasGuia = await _repoGuia.ObtenerTodasAsync();
        RenderGuia(_tareasGuia);
    }

    private async void GuiaAgregar_Click(object sender, RoutedEventArgs e)
    {
        if (_repoGuia is null)
            return;

        var nueva = PedirTarea(tarea: null);
        if (nueva is null)
            return;

        await _repoGuia.CrearAsync(nueva);
        await RefrescarGuiaAsync();
    }

    /// <summary>Abre el diálogo de alta/edición y devuelve la tarea, o <c>null</c> si se canceló.</summary>
    private TareaGuia? PedirTarea(TareaGuia? tarea)
    {
        var dialogo = new EditarTareaGuiaWindow(tarea, _tareasGuia.Select(t => t.Categoria))
        {
            Owner = this,
        };

        return dialogo.ShowDialog() == true ? dialogo.Resultado : null;
    }

    private void RenderGuia(IReadOnlyList<TareaGuia> tareas)
    {
        GuiaPanel.Children.Clear();

        var pendientes = tareas.Count(t => !t.Hecha);
        GuiaSummaryText.Text = tareas.Count == 0
            ? "sin tareas · usá + para agregar"
            : $"{pendientes} de {tareas.Count} pendientes";

        foreach (var grupo in tareas.GroupBy(t => t.Categoria))
        {
            var encabezado = new TextBlock
            {
                Text = grupo.Key.ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("TextMuted"),
                Margin = new Thickness(0, 14, 0, 10),
            };
            GuiaPanel.Children.Add(encabezado);

            foreach (var tarea in grupo)
                GuiaPanel.Children.Add(CrearFilaTarea(tarea));
        }
    }

    private FrameworkElement CrearFilaTarea(TareaGuia tarea)
    {
        var fila = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10),
            Cursor = Cursors.Hand,
            Background = Brushes.Transparent,
        };
        fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var casilla = new Border
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(5),
            BorderBrush = tarea.Hecha ? (Brush)FindResource("AccentFlexible") : (Brush)FindResource("TextFaint"),
            BorderThickness = new Thickness(1.6),
            Background = tarea.Hecha ? (Brush)FindResource("AccentFlexible") : Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 1, 11, 0),
        };
        if (tarea.Hecha)
        {
            casilla.Child = new Path
            {
                Data = Geometry.Parse("M4 9.5 L7.5 13 L14 5.5"),
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            };
        }
        Grid.SetColumn(casilla, 0);

        var textos = new StackPanel();
        textos.Children.Add(new TextBlock
        {
            Text = tarea.Title,
            FontSize = 13,
            Foreground = tarea.Hecha ? (Brush)FindResource("TextMuted") : (Brush)FindResource("TextPrimary"),
            TextDecorations = tarea.Hecha ? TextDecorations.Strikethrough : null,
            TextWrapping = TextWrapping.Wrap,
        });
        textos.Children.Add(new TextBlock
        {
            Text = DescribirEstado(tarea),
            FontSize = 10.5,
            Foreground = (Brush)FindResource("TextFaint"),
            Margin = new Thickness(0, 2, 0, 0),
        });
        Grid.SetColumn(textos, 1);

        var editar = new Button
        {
            Content = "✎",
            Style = (Style)FindResource("NavArrowButtonStyle"),
            Width = 22,
            Height = 22,
            FontSize = 12,
            VerticalAlignment = VerticalAlignment.Top,
            ToolTip = "Editar tarea",
        };
        editar.Click += async (_, _) => await EditarTareaAsync(tarea);
        Grid.SetColumn(editar, 2);

        fila.Children.Add(casilla);
        fila.Children.Add(textos);
        fila.Children.Add(editar);

        HabilitarClick(fila);
        fila.MouseLeftButtonUp += async (_, _) => await AlternarTareaAsync(tarea);

        return fila;
    }

    private async Task AlternarTareaAsync(TareaGuia tarea)
    {
        if (_repoGuia is null)
            return;

        if (tarea.Hecha)
            await _repoGuia.DesmarcarAsync(tarea.Id);
        else
            await _repoGuia.MarcarHechaAsync(tarea.Id, DateOnly.FromDateTime(DateTime.Today));

        await RefrescarGuiaAsync();
    }

    private async Task EditarTareaAsync(TareaGuia tarea)
    {
        if (_repoGuia is null)
            return;

        var editada = PedirTarea(tarea);
        if (editada is null)
            return;

        await _repoGuia.ActualizarAsync(editada);
        await RefrescarGuiaAsync();
    }

    private static string DescribirEstado(TareaGuia tarea)
    {
        var cadencia = tarea.Repeticion switch
        {
            Repeticion.Semanal => "semanal",
            Repeticion.Mensual => "mensual",
            _ => "una vez",
        };

        if (!tarea.Hecha)
            return $"{cadencia} · {DescribirUltimaVez(tarea.UltimaVez)}";

        var vigencia = tarea.Repeticion switch
        {
            Repeticion.Semanal => "hecha esta semana",
            Repeticion.Mensual => "hecha este mes",
            _ => "hecha",
        };

        return $"{cadencia} · {vigencia}";
    }

    private static string DescribirUltimaVez(DateOnly? ultimaVez)
    {
        if (ultimaVez is null)
            return "sin registro";

        var dias = DateOnly.FromDateTime(DateTime.Today).DayNumber - ultimaVez.Value.DayNumber;
        return dias switch
        {
            0 => "hoy",
            1 => "ayer",
            _ => $"última vez hace {dias} días",
        };
    }
}
