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

public partial class MainWindow
{
    private const double AgendaOffsetX = 58;
    private const double AgendaAxisX = 46;
    private const double PixelsPerMinute = 0.72;

    /// <summary>Alto de la banda de un día: las 24 horas completas.</summary>
    private const double AlturaDia = 24 * 60 * PixelsPerMinute;

    /// <summary>
    /// Tope de días simultáneos en el lienzo. El scroll continuo carga días
    /// indefinidamente, así que hay que descartar por el extremo opuesto para
    /// que el Canvas no crezca sin límite.
    /// </summary>
    private const int MaxDiasCargados = 9;

    private sealed record DiaCargado(DateOnly Fecha, IReadOnlyList<ScheduledBlock> Bloques);

    private readonly List<DiaCargado> _diasCargados = [];
    private bool _cargandoDiaAdyacente;

    private async Task CargarDiaAsync(DateOnly fecha)
    {
        try
        {
            var dia = await ObtenerDiaAsync(fecha);

            _diasCargados.Clear();
            _diasCargados.Add(dia);
            RenderDias();

            DiaScroll.UpdateLayout();
            DiaScroll.ScrollToVerticalOffset(OffsetInicialDe(fecha));
        }
        catch (Exception ex)
        {
            AgendaSubtitleText.Text = $"No se pudo cargar la agenda: {ex.Message}";
        }
    }

    private static async Task<DiaCargado> ObtenerDiaAsync(DateOnly fecha)
    {
        var contexto = new AgendaDbContext();
        contexto.Inicializar();

        var repoTareas = new SqliteFlexibleTaskRepository(contexto);
        var fuente = new GoogleCalendarSource();

        var fijos = await fuente.ObtenerFijosDelDiaAsync(fecha);
        var tareas = await repoTareas.ObtenerTodasAsync();
        var yaAgendadas = await repoTareas.ObtenerYaAgendadasAsync();

        var agenda = Scheduler.ArmarAgenda(fijos, tareas, fecha, yaAgendadas);

        // Solo se persiste "ya agendada" cuando se está mirando hoy de verdad.
        // Los días adyacentes que trae el scroll continuo, igual que Semana/Mes,
        // son preview y no deben tocar este flag global (ver plan: invariante de
        // MarcarAgendadaAsync).
        if (fecha == DateOnly.FromDateTime(DateTime.Today))
        {
            foreach (var bloque in agenda.Where(b => b.Kind == BlockKind.Flexible))
                await repoTareas.MarcarAgendadaAsync(bloque.TaskId!, fecha);
        }

        return new DiaCargado(fecha, agenda);
    }

    /// <summary>Arranca cerca de la hora actual si es hoy; si no, a la mañana.</summary>
    private double OffsetInicialDe(DateOnly fecha)
    {
        var minutos = fecha == DateOnly.FromDateTime(DateTime.Today)
            ? DateTime.Now.TimeOfDay.TotalMinutes - 90
            : 7 * 60;

        var maximo = Math.Max(AlturaDia - DiaScroll.ViewportHeight, 0);
        return Math.Clamp(minutos * PixelsPerMinute, 0, maximo);
    }

    // ===================== scroll continuo entre días =====================

    private void DiaScroll_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        const double Margen = 1.0;

        if (e.Delta > 0 && DiaScroll.VerticalOffset <= Margen)
            _ = CargarDiaAdyacenteAsync(haciaAtras: true);
        else if (e.Delta < 0 && DiaScroll.VerticalOffset >= DiaScroll.ScrollableHeight - Margen)
            _ = CargarDiaAdyacenteAsync(haciaAtras: false);
    }

    /// <summary>
    /// Antepone el día anterior o agrega el siguiente al lienzo, manteniendo
    /// fija la posición visual de lo que el usuario ya estaba mirando.
    /// </summary>
    private async Task CargarDiaAdyacenteAsync(bool haciaAtras)
    {
        if (_cargandoDiaAdyacente || _diasCargados.Count == 0)
            return;

        _cargandoDiaAdyacente = true;
        try
        {
            var fecha = haciaAtras
                ? _diasCargados[0].Fecha.AddDays(-1)
                : _diasCargados[^1].Fecha.AddDays(1);

            var dia = await ObtenerDiaAsync(fecha);
            var offsetPrevio = DiaScroll.VerticalOffset;
            var desplazamiento = 0.0;

            if (haciaAtras)
            {
                _diasCargados.Insert(0, dia);
                // Todo lo anterior baja una banda entera: hay que compensar el
                // scroll o el contenido salta bajo el cursor.
                desplazamiento += AlturaDia;

                if (_diasCargados.Count > MaxDiasCargados)
                    _diasCargados.RemoveAt(_diasCargados.Count - 1);
            }
            else
            {
                _diasCargados.Add(dia);

                if (_diasCargados.Count > MaxDiasCargados)
                {
                    _diasCargados.RemoveAt(0);
                    desplazamiento -= AlturaDia;
                }
            }

            RenderDias();
            DiaScroll.UpdateLayout();
            DiaScroll.ScrollToVerticalOffset(offsetPrevio + desplazamiento);
        }
        catch (Exception ex)
        {
            AgendaSubtitleText.Text = $"No se pudo cargar el día contiguo: {ex.Message}";
        }
        finally
        {
            _cargandoDiaAdyacente = false;
        }
    }

    private void DiaScroll_ScrollChanged(object sender, ScrollChangedEventArgs e) =>
        ActualizarSubtituloDia();

    /// <summary>El subtítulo sigue al día que ocupa el tope de lo visible.</summary>
    private void ActualizarSubtituloDia()
    {
        if (_vista != Vista.Dia || _diasCargados.Count == 0)
            return;

        var indice = Math.Clamp((int)(DiaScroll.VerticalOffset / AlturaDia), 0, _diasCargados.Count - 1);
        AgendaSubtitleText.Text = DescribirFecha(_diasCargados[indice].Fecha);
    }

    private static string DescribirFecha(DateOnly fecha)
    {
        var cultura = new CultureInfo("es-AR");
        var nombreDia = cultura.DateTimeFormat.GetDayName(fecha.DayOfWeek);
        return $"{char.ToUpper(nombreDia[0]) + nombreDia[1..]} {fecha.Day} de {cultura.DateTimeFormat.GetMonthName(fecha.Month)}";
    }

    // ===================== render =====================

    private void RenderDias()
    {
        AgendaCanvas.Children.Clear();
        AgendaCanvas.Height = _diasCargados.Count * AlturaDia;

        var anchoBloque = AgendaCanvas.Width - AgendaOffsetX;

        var divisor = new Rectangle { Width = 1, Height = AgendaCanvas.Height, Fill = (Brush)FindResource("PanelBorder") };
        Canvas.SetLeft(divisor, AgendaAxisX);
        Canvas.SetTop(divisor, 0);
        AgendaCanvas.Children.Add(divisor);

        for (var i = 0; i < _diasCargados.Count; i++)
            RenderBandaDia(_diasCargados[i], i * AlturaDia, anchoBloque);

        ActualizarSubtituloDia();
    }

    private void RenderBandaDia(DiaCargado dia, double offsetY, double anchoBloque)
    {
        var inicioDia = dia.Fecha.ToDateTime(TimeOnly.MinValue);

        for (var h = 0; h < 24; h++)
        {
            var top = offsetY + h * 60 * PixelsPerMinute;
            AddHourLabel(AgendaCanvas, h.ToString("00"), top);

            if (h > 0)
                AddGridLine(AgendaCanvas, AgendaOffsetX, anchoBloque, top);
        }

        AddSeparadorDia(dia.Fecha, offsetY, anchoBloque);

        foreach (var bloque in dia.Bloques)
        {
            // Un bloque puede cruzar la medianoche (p. ej. "Dormir"): se recorta
            // a la banda de su propio día para no pisar la del día vecino.
            var desde = Math.Clamp((bloque.Start - inicioDia).TotalMinutes, 0, 24 * 60);
            var hasta = Math.Clamp((bloque.End - inicioDia).TotalMinutes, 0, 24 * 60);
            if (hasta <= desde)
                continue;

            var top = offsetY + desde * PixelsPerMinute;
            var alto = Math.Max((hasta - desde) * PixelsPerMinute, 4);
            AddBlock(AgendaCanvas, bloque, AgendaOffsetX, top, alto, anchoBloque);
        }

        if (dia.Fecha == DateOnly.FromDateTime(DateTime.Today))
        {
            var ahora = DateTime.Now;
            var top = offsetY + ahora.TimeOfDay.TotalMinutes * PixelsPerMinute;
            AddAhoraIndicator(AgendaCanvas, AgendaAxisX, top, anchoBloque, ahora);
        }
    }

    private void AddSeparadorDia(DateOnly fecha, double offsetY, double anchoBloque)
    {
        var linea = new Rectangle { Width = anchoBloque, Height = 1, Fill = (Brush)FindResource("PanelBorder") };
        Canvas.SetLeft(linea, AgendaOffsetX);
        Canvas.SetTop(linea, offsetY);
        AgendaCanvas.Children.Add(linea);

        var etiqueta = new Border
        {
            Background = new SolidColorBrush(Color.FromArgb(0xD9, 0x14, 0x18, 0x22)),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(8, 2, 8, 2),
            Child = new TextBlock
            {
                Text = DescribirFecha(fecha).ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = (Brush)FindResource("TextMuted"),
            },
        };
        Canvas.SetLeft(etiqueta, AgendaOffsetX);
        Canvas.SetTop(etiqueta, offsetY + 3);
        AgendaCanvas.Children.Add(etiqueta);
    }

    private void AddHourLabel(Canvas canvas, string texto, double top, double left = 0, double width = 40)
    {
        var etiqueta = new TextBlock
        {
            Text = texto,
            FontSize = 10.5,
            Foreground = (Brush)FindResource("TextFaint"),
            Width = width,
            TextAlignment = TextAlignment.Right,
        };
        Canvas.SetLeft(etiqueta, left);
        Canvas.SetTop(etiqueta, top - 6);
        canvas.Children.Add(etiqueta);
    }

    private void AddGridLine(Canvas canvas, double left, double ancho, double top)
    {
        var linea = new Rectangle { Width = ancho, Height = 1, Fill = (Brush)FindResource("GridLine") };
        Canvas.SetLeft(linea, left);
        Canvas.SetTop(linea, top);
        canvas.Children.Add(linea);
    }

    private void AddBlock(Canvas canvas, ScheduledBlock bloque, double left, double top, double alto, double ancho, bool forzarCompacto = false)
    {
        var esLibre = bloque.Kind == BlockKind.Libre;
        var compacto = forzarCompacto || alto < 40;

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

        var contenido = new Grid { Margin = new Thickness(compacto ? 10 : 16, 0, compacto ? 10 : 16, 0) };

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
                TextTrimming = TextTrimming.CharacterEllipsis,
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

        Canvas.SetLeft(contenedor, left);
        Canvas.SetTop(contenedor, top);
        canvas.Children.Add(contenedor);
    }

    private void AddAhoraIndicator(Canvas canvas, double left, double top, double ancho, DateTime ahora)
    {
        var linea = new Rectangle { Width = ancho, Height = 1.5, Fill = (Brush)FindResource("AccentFlexible") };
        Canvas.SetLeft(linea, left);
        Canvas.SetTop(linea, top);
        canvas.Children.Add(linea);

        var punto = new Ellipse { Width = 11, Height = 11, Fill = (Brush)FindResource("AccentFlexible") };
        Canvas.SetLeft(punto, left - 5.5);
        Canvas.SetTop(punto, top - 5.5);
        canvas.Children.Add(punto);

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
        Canvas.SetLeft(etiqueta, left + 20);
        Canvas.SetTop(etiqueta, top - 10);
        canvas.Children.Add(etiqueta);
    }
}
