using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PepeWapOSx10.Calendario;
using PepeWapOSx10.Datos;
using PepeWapOSx10.Dominio;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

public partial class MainWindow : Window
{
    private const double AgendaOffsetX = 58;
    private const double AgendaAxisX = 46;
    private const double PixelsPerMinute = 0.72;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Root_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => DragMove();

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        BuildCalendar(DateTime.Today);
        await CargarAgendaAsync();
    }

    private async Task CargarAgendaAsync()
    {
        try
        {
            var contexto = new AgendaDbContext();
            contexto.Inicializar();

            var repoTareas = new SqliteFlexibleTaskRepository(contexto);
            var repoGuia = new SqliteTareaGuiaRepository(contexto);
            var fuente = new GoogleCalendarSource();

            var fecha = DateOnly.FromDateTime(DateTime.Today);
            var fijos = await fuente.ObtenerFijosDelDiaAsync(fecha);
            var tareas = await repoTareas.ObtenerTodasAsync();
            var yaAgendadas = await repoTareas.ObtenerYaAgendadasAsync();

            var (inicioDia, finDia) = Scheduler.InferirVentanaDia(fijos, fecha);
            var agenda = Scheduler.ArmarAgenda(fijos, tareas, fecha, yaAgendadas);

            foreach (var bloque in agenda.Where(b => b.Kind == BlockKind.Flexible))
                await repoTareas.MarcarAgendadaAsync(bloque.TaskId!, fecha);

            RenderAgenda(agenda, inicioDia, finDia, fecha);

            var guia = await repoGuia.ObtenerTodasAsync();
            RenderGuia(guia, repoGuia, fecha);
        }
        catch (Exception ex)
        {
            AgendaSubtitleText.Text = $"No se pudo cargar la agenda: {ex.Message}";
        }
    }

    // ===================== CALENDARIO =====================

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

    // ===================== AGENDA =====================

    private void RenderAgenda(IReadOnlyList<ScheduledBlock> bloques, DateTime inicioDia, DateTime finDia, DateOnly fecha)
    {
        AgendaCanvas.Children.Clear();

        var cultura = new CultureInfo("es-AR");
        var nombreDia = cultura.DateTimeFormat.GetDayName(fecha.DayOfWeek);
        AgendaSubtitleText.Text =
            $"{char.ToUpper(nombreDia[0]) + nombreDia[1..]} {fecha.Day} de {cultura.DateTimeFormat.GetMonthName(fecha.Month)} · {inicioDia:HH:mm} – {finDia:HH:mm}";

        var libres = bloques.Count(b => b.Kind == BlockKind.Libre);
        CalendarSummaryText.Text = $"{bloques.Count} bloques hoy · {libres} libres";

        var totalMinutos = Math.Max((finDia - inicioDia).TotalMinutes, 1);
        AgendaCanvas.Height = totalMinutos * PixelsPerMinute;
        var anchoBloque = AgendaCanvas.Width - AgendaOffsetX;

        var divisor = new Rectangle { Width = 1, Height = AgendaCanvas.Height, Fill = (Brush)FindResource("PanelBorder") };
        Canvas.SetLeft(divisor, AgendaAxisX);
        Canvas.SetTop(divisor, 0);
        AgendaCanvas.Children.Add(divisor);

        var cursorHora = new DateTime(inicioDia.Year, inicioDia.Month, inicioDia.Day, inicioDia.Hour, 0, 0);
        if (cursorHora <= inicioDia)
            cursorHora = cursorHora.AddHours(1);

        while (cursorHora < finDia)
        {
            var top = (cursorHora - inicioDia).TotalMinutes * PixelsPerMinute;
            AddHourLabel(cursorHora.ToString("HH"), top);
            AddGridLine(AgendaOffsetX, anchoBloque, top);
            cursorHora = cursorHora.AddHours(1);
        }

        foreach (var bloque in bloques)
        {
            var top = (bloque.Start - inicioDia).TotalMinutes * PixelsPerMinute;
            var alto = Math.Max((bloque.End - bloque.Start).TotalMinutes * PixelsPerMinute, 4);
            AddBlock(bloque, top, alto, anchoBloque);
        }

        var ahora = DateTime.Now;
        if (fecha == DateOnly.FromDateTime(DateTime.Today) && ahora >= inicioDia && ahora <= finDia)
        {
            var top = (ahora - inicioDia).TotalMinutes * PixelsPerMinute;
            AddAhoraIndicator(top, anchoBloque, ahora);
        }
    }

    private void AddHourLabel(string texto, double top)
    {
        var etiqueta = new TextBlock
        {
            Text = texto,
            FontSize = 10.5,
            Foreground = (Brush)FindResource("TextFaint"),
            Width = 40,
            TextAlignment = TextAlignment.Right,
        };
        Canvas.SetLeft(etiqueta, 0);
        Canvas.SetTop(etiqueta, top - 6);
        AgendaCanvas.Children.Add(etiqueta);
    }

    private void AddGridLine(double left, double ancho, double top)
    {
        var linea = new Rectangle { Width = ancho, Height = 1, Fill = (Brush)FindResource("GridLine") };
        Canvas.SetLeft(linea, left);
        Canvas.SetTop(linea, top);
        AgendaCanvas.Children.Add(linea);
    }

    private void AddBlock(ScheduledBlock bloque, double top, double alto, double ancho)
    {
        var esLibre = bloque.Kind == BlockKind.Libre;
        var compacto = alto < 40;

        var contenedor = new Grid { Width = ancho, Height = alto };

        var fondo = new Rectangle
        {
            Width = ancho,
            Height = alto,
            RadiusX = compacto ? 10 : 14,
            RadiusY = compacto ? 10 : 14,
            Fill = esLibre ? Brushes.Transparent : (Brush)FindResource("BlockBackground"),
            Stroke = esLibre ? new SolidColorBrush(Color.FromArgb(0x1A, 0xED, 0xEF, 0xF2)) : (Brush)FindResource("PanelBorder"),
            StrokeThickness = 1,
        };
        if (esLibre)
            fondo.StrokeDashArray = [3, 3];
        contenedor.Children.Add(fondo);

        var dotBrush = bloque.Kind switch
        {
            BlockKind.Fixed => (Brush)FindResource("AccentFijo"),
            BlockKind.Flexible => (Brush)FindResource("AccentFlexible"),
            _ => (Brush)FindResource("AccentLibre"),
        };

        var contenido = new Grid { Margin = new Thickness(16, 0, 16, 0) };

        if (compacto)
        {
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var dot = new Ellipse { Width = 6, Height = 6, Fill = dotBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) };
            Grid.SetColumn(dot, 0);

            var titulo = new TextBlock
            {
                Text = bloque.Title,
                FontSize = 11.5,
                FontWeight = esLibre ? FontWeights.Normal : FontWeights.SemiBold,
                FontStyle = esLibre ? FontStyles.Italic : FontStyles.Normal,
                Foreground = esLibre ? (Brush)FindResource("TextFaint") : (Brush)FindResource("TextPrimary"),
                VerticalAlignment = VerticalAlignment.Center,
            };
            Grid.SetColumn(titulo, 1);

            var hora = new TextBlock
            {
                Text = $"{bloque.Start:HH:mm}–{bloque.End:HH:mm}",
                FontSize = 10.5,
                Foreground = (Brush)FindResource("TextMuted"),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right,
            };
            Grid.SetColumn(hora, 3);

            contenido.Children.Add(dot);
            contenido.Children.Add(titulo);
            contenido.Children.Add(hora);
        }
        else
        {
            var pila = new StackPanel { VerticalAlignment = VerticalAlignment.Center };

            var filaTitulo = new StackPanel { Orientation = Orientation.Horizontal };
            filaTitulo.Children.Add(new Ellipse { Width = 7, Height = 7, Fill = dotBrush, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 10, 0) });
            filaTitulo.Children.Add(new TextBlock
            {
                Text = bloque.Title,
                FontSize = alto > 150 ? 15 : 13.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextPrimary"),
            });

            var duracion = bloque.End - bloque.Start;
            var textoDuracion = duracion.Hours > 0 ? $"{duracion.Hours}h {duracion.Minutes}m" : $"{duracion.Minutes}m";

            var subtitulo = new TextBlock
            {
                Text = esLibre
                    ? $"Libre · {textoDuracion} sin asignar"
                    : $"{bloque.Start:HH:mm} – {bloque.End:HH:mm}",
                FontSize = 11.5,
                FontStyle = esLibre ? FontStyles.Italic : FontStyles.Normal,
                Foreground = esLibre ? (Brush)FindResource("TextFaint") : (Brush)FindResource("TextMuted"),
                Margin = new Thickness(esLibre ? 0 : 17, 4, 0, 0),
            };

            pila.Children.Add(filaTitulo);
            pila.Children.Add(subtitulo);
            contenido.Children.Add(pila);
        }

        contenedor.Children.Add(contenido);

        Canvas.SetLeft(contenedor, AgendaOffsetX);
        Canvas.SetTop(contenedor, top);
        AgendaCanvas.Children.Add(contenedor);
    }

    private void AddAhoraIndicator(double top, double ancho, DateTime ahora)
    {
        var linea = new Rectangle { Width = ancho, Height = 1.5, Fill = (Brush)FindResource("AccentFlexible") };
        Canvas.SetLeft(linea, AgendaAxisX);
        Canvas.SetTop(linea, top);
        AgendaCanvas.Children.Add(linea);

        var punto = new Ellipse { Width = 11, Height = 11, Fill = (Brush)FindResource("AccentFlexible") };
        Canvas.SetLeft(punto, AgendaAxisX - 5.5);
        Canvas.SetTop(punto, top - 5.5);
        AgendaCanvas.Children.Add(punto);

        var etiqueta = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0xD9, 0x14, 0x18, 0x22)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(7, 2, 7, 2),
            Child = new TextBlock
            {
                Text = $"AHORA · {ahora:HH:mm}",
                FontSize = 10.5,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("AccentFlexible"),
            },
        };
        Canvas.SetLeft(etiqueta, AgendaOffsetX + 8);
        Canvas.SetTop(etiqueta, top - 10);
        AgendaCanvas.Children.Add(etiqueta);
    }

    // ===================== GUIA (TareaGuia) =====================

    private void RenderGuia(IReadOnlyList<TareaGuia> tareas, SqliteTareaGuiaRepository repo, DateOnly fecha)
    {
        GuiaPanel.Children.Clear();

        var pendientes = tareas.Count(t => !t.HechaHoy);
        GuiaSummaryText.Text = $"{pendientes} de {tareas.Count} pendientes";

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
                GuiaPanel.Children.Add(CrearFilaTarea(tarea, repo, fecha));
        }
    }

    private FrameworkElement CrearFilaTarea(TareaGuia tarea, SqliteTareaGuiaRepository repo, DateOnly fecha)
    {
        var fila = new Grid
        {
            Margin = new Thickness(0, 0, 0, 10),
            Cursor = tarea.HechaHoy ? Cursors.Arrow : Cursors.Hand,
            Background = Brushes.Transparent,
        };
        fila.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        fila.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

        var casilla = new Border
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(5),
            BorderBrush = tarea.HechaHoy ? (Brush)FindResource("AccentFlexible") : (Brush)FindResource("TextFaint"),
            BorderThickness = new Thickness(1.6),
            Background = tarea.HechaHoy ? (Brush)FindResource("AccentFlexible") : Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 1, 11, 0),
        };
        if (tarea.HechaHoy)
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
            Foreground = tarea.HechaHoy ? (Brush)FindResource("TextMuted") : (Brush)FindResource("TextPrimary"),
            TextDecorations = tarea.HechaHoy ? TextDecorations.Strikethrough : null,
        });
        textos.Children.Add(new TextBlock
        {
            Text = tarea.HechaHoy ? "hecha hoy" : DescribirUltimaVez(tarea.UltimaVez),
            FontSize = 10.5,
            Foreground = (Brush)FindResource("TextFaint"),
            Margin = new Thickness(0, 2, 0, 0),
        });
        Grid.SetColumn(textos, 1);

        fila.Children.Add(casilla);
        fila.Children.Add(textos);

        if (!tarea.HechaHoy)
        {
            fila.MouseLeftButtonUp += async (_, _) =>
            {
                await repo.MarcarHechaAsync(tarea.Id, fecha);
                var actualizadas = await repo.ObtenerTodasAsync();
                RenderGuia(actualizadas, repo, fecha);
            };
        }

        return fila;
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
