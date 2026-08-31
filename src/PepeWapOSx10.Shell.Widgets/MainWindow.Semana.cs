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
    private async Task CargarSemanaAsync(DateOnly inicio)
    {
        try
        {
            var contexto = new AgendaDbContext();
            contexto.Inicializar();

            var repoTareas = new SqliteFlexibleTaskRepository(contexto);
            var fuente = new GoogleCalendarSource();

            var fin = inicio.AddDays(6);
            var fijosRango = await fuente.ObtenerFijosDelRangoAsync(inicio, fin);
            var tareas = await repoTareas.ObtenerTodasAsync();
            var yaAgendadas = await repoTareas.ObtenerYaAgendadasAsync();

            var dias = Enumerable.Range(0, 7).Select(inicio.AddDays).ToList();

            // Solo lectura/preview: nunca se llama MarcarAgendadaAsync acá
            // (ver invariante en el plan) — Semana no debe alterar qué
            // tareas flexibles cuentan como "ya hechas".
            var agendasPorDia = dias
                .Select(dia => Scheduler.ArmarAgenda(FiltrarFijosPorDia(fijosRango, dia), tareas, dia, yaAgendadas))
                .ToList();

            RenderSemana(dias, agendasPorDia);
        }
        catch (Exception ex)
        {
            AgendaSubtitleText.Text = $"No se pudo cargar la semana: {ex.Message}";
        }
    }

    private static IReadOnlyList<FixedEvent> FiltrarFijosPorDia(IReadOnlyList<FixedEvent> fijos, DateOnly fecha)
    {
        var inicioDia = fecha.ToDateTime(TimeOnly.MinValue);
        var finDia = inicioDia.AddDays(1);
        return fijos.Where(f => f.Start < finDia && f.End > inicioDia).ToList();
    }

    private void RenderSemana(IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        var cultura = new CultureInfo("es-AR");
        NavLabelText.Text = FormatearRangoSemana(dias[0], dias[6]);
        AgendaSubtitleText.Text = "Vista semanal";

        // ---- header de días ----
        SemanaHeadGrid.Children.Clear();
        SemanaHeadGrid.ColumnDefinitions.Clear();
        SemanaHeadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(AgendaAxisX) });
        for (var i = 0; i < 7; i++)
            SemanaHeadGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var encabezados = new[] { "L", "M", "M", "J", "V", "S", "D" };
        var hoy = DateOnly.FromDateTime(DateTime.Today);

        for (var i = 0; i < 7; i++)
        {
            var dia = dias[i];
            var esHoy = dia == hoy;
            var esResaltado = _diaResaltado == dia;

            var pila = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            pila.Children.Add(new TextBlock
            {
                Text = encabezados[i],
                FontSize = 10,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextMuted"),
                HorizontalAlignment = HorizontalAlignment.Center,
            });

            FrameworkElement numEl = esHoy
                ? new Border
                {
                    Width = 24,
                    Height = 24,
                    CornerRadius = new CornerRadius(12),
                    Background = (Brush)FindResource("AccentFlexible"),
                    Margin = new Thickness(0, 3, 0, 0),
                    Child = new TextBlock
                    {
                        Text = dia.Day.ToString(),
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                    },
                }
                : new TextBlock
                {
                    Text = dia.Day.ToString(),
                    FontSize = 14,
                    Foreground = (Brush)FindResource("TextPrimary"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 3, 0, 0),
                };
            pila.Children.Add(numEl);

            var celda = new Border
            {
                CornerRadius = new CornerRadius(9),
                Background = esResaltado ? (Brush)FindResource("SelectedTint") : Brushes.Transparent,
                Padding = new Thickness(4, 6, 4, 6),
                Cursor = Cursors.Hand,
                Child = pila,
            };
            Grid.SetColumn(celda, i + 1);

            HabilitarClick(celda);
            celda.MouseLeftButtonUp += (_, _) =>
            {
                if (EsDobleClick($"semana-header:{dia:yyyy-MM-dd}"))
                {
                    _ = CambiarVistaAsync(Vista.Dia, fechaDia: dia);
                }
                else
                {
                    _diaResaltado = _diaResaltado == dia ? null : dia;
                    _semanaResaltadaInicio = null;
                    RenderSemana(dias, agendas);
                }
            };

            SemanaHeadGrid.Children.Add(celda);
        }

        // ---- grilla horaria: 24 filas fijas, comunes a las 7 columnas ----
        SemanaCanvas.Children.Clear();

        var totalMin = 24 * 60;
        SemanaCanvas.Height = totalMin * PixelsPerMinute;
        var colWidth = (SemanaCanvas.Width - AgendaAxisX) / 7.0;

        if (_diaResaltado is { } resaltado)
        {
            var idx = resaltado.DayNumber - dias[0].DayNumber;
            if (idx is >= 0 and < 7)
            {
                var tint = new Rectangle { Width = colWidth, Height = SemanaCanvas.Height, Fill = (Brush)FindResource("SelectedTint") };
                Canvas.SetLeft(tint, AgendaAxisX + idx * colWidth);
                Canvas.SetTop(tint, 0);
                SemanaCanvas.Children.Add(tint);
            }
        }

        for (var h = 0; h < 24; h++)
        {
            var top = h * 60 * PixelsPerMinute;
            AddHourLabel(SemanaCanvas, h.ToString("00"), top);
            AddGridLine(SemanaCanvas, AgendaAxisX, SemanaCanvas.Width - AgendaAxisX, top);
        }

        for (var i = 0; i < 7; i++)
        {
            var left = AgendaAxisX + i * colWidth;
            var ancho = colWidth - 4;

            foreach (var bloque in agendas[i].Where(b => b.Kind != BlockKind.Libre))
            {
                var minutosDelDia = bloque.Start.Hour * 60 + bloque.Start.Minute;
                var top = minutosDelDia * PixelsPerMinute;
                var alto = Math.Max((bloque.End - bloque.Start).TotalMinutes * PixelsPerMinute, 17);
                AddBlock(SemanaCanvas, bloque, left + 2, top, alto, ancho, forzarCompacto: true);
            }

            if (dias[i] == hoy)
            {
                var ahora = DateTime.Now;
                var top = (ahora.Hour * 60 + ahora.Minute) * PixelsPerMinute;
                var linea = new Rectangle { Width = colWidth, Height = 1.5, Fill = (Brush)FindResource("AccentFlexible") };
                Canvas.SetLeft(linea, left);
                Canvas.SetTop(linea, top);
                SemanaCanvas.Children.Add(linea);
            }
        }

        if (SemanaScroll.VerticalOffset == 0)
            SemanaScroll.ScrollToVerticalOffset(6 * 60 * PixelsPerMinute);
    }

    private static string FormatearRangoSemana(DateOnly inicio, DateOnly fin)
    {
        var cultura = new CultureInfo("es-AR");
        string Mes(int m)
        {
            var nombre = cultura.DateTimeFormat.GetMonthName(m);
            return char.ToUpper(nombre[0]) + nombre[1..];
        }

        return inicio.Month == fin.Month
            ? $"{inicio.Day} – {fin.Day} de {Mes(inicio.Month)} {fin.Year}"
            : $"{inicio.Day} de {Mes(inicio.Month)} – {fin.Day} de {Mes(fin.Month)} {fin.Year}";
    }
}
