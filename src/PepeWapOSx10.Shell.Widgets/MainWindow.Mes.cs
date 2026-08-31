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
    private async Task CargarMesAsync(DateOnly mes)
    {
        try
        {
            var contexto = new AgendaDbContext();
            contexto.Inicializar();

            var repoTareas = new SqliteFlexibleTaskRepository(contexto);
            var fuente = new GoogleCalendarSource();

            var primerDiaMes = new DateOnly(mes.Year, mes.Month, 1);
            var gridInicio = InicioDeSemana(primerDiaMes);
            var diasEnMes = DateTime.DaysInMonth(mes.Year, mes.Month);
            var ultimoDiaMes = primerDiaMes.AddDays(diasEnMes - 1);
            var offsetFinal = ((int)ultimoDiaMes.DayOfWeek + 6) % 7;
            var gridFin = ultimoDiaMes.AddDays(6 - offsetFinal);

            var fijosRango = await fuente.ObtenerFijosDelRangoAsync(gridInicio, gridFin);
            var tareas = await repoTareas.ObtenerTodasAsync();
            var yaAgendadas = await repoTareas.ObtenerYaAgendadasAsync();

            var totalDias = gridFin.DayNumber - gridInicio.DayNumber + 1;
            var dias = Enumerable.Range(0, totalDias).Select(gridInicio.AddDays).ToList();

            // Solo lectura/preview, igual que Semana: nunca marca tareas
            // flexibles como agendadas (ver invariante en el plan).
            var agendasPorDia = dias
                .Select(dia => Scheduler.ArmarAgenda(FiltrarFijosPorDia(fijosRango, dia), tareas, dia, yaAgendadas))
                .ToList();

            RenderMes(mes, dias, agendasPorDia);
        }
        catch (Exception ex)
        {
            AgendaSubtitleText.Text = $"No se pudo cargar el mes: {ex.Message}";
        }
    }

    private void RenderMes(DateOnly mesActual, IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        var cultura = new CultureInfo("es-AR");
        var nombreMes = cultura.DateTimeFormat.GetMonthName(mesActual.Month);
        NavLabelText.Text = $"{char.ToUpper(nombreMes[0]) + nombreMes[1..]} {mesActual.Year}";
        AgendaSubtitleText.Text = "Vista mensual";

        // ---- header de días de la semana ----
        MesHeadGrid.Children.Clear();
        MesHeadGrid.ColumnDefinitions.Clear();
        MesHeadGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
        for (var i = 0; i < 7; i++)
            MesHeadGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var encabezados = new[] { "L", "M", "M", "J", "V", "S", "D" };
        for (var i = 0; i < 7; i++)
        {
            var tb = new TextBlock
            {
                Text = encabezados[i],
                FontSize = 10.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = (Brush)FindResource("TextMuted"),
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetColumn(tb, i + 1);
            MesHeadGrid.Children.Add(tb);
        }

        // ---- filas: numero de semana + 7 celdas de dia ----
        MesRowsGrid.Children.Clear();
        MesRowsGrid.RowDefinitions.Clear();
        MesRowsGrid.ColumnDefinitions.Clear();
        MesRowsGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(34) });
        for (var i = 0; i < 7; i++)
            MesRowsGrid.ColumnDefinitions.Add(new ColumnDefinition());

        var numFilas = dias.Count / 7;
        for (var f = 0; f < numFilas; f++)
            MesRowsGrid.RowDefinitions.Add(new RowDefinition());

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        for (var f = 0; f < numFilas; f++)
        {
            var filaInicio = dias[f * 7];

            if (_semanaResaltadaInicio == filaInicio)
            {
                var tintFila = new Border
                {
                    Background = (Brush)FindResource("SelectedTint"),
                    CornerRadius = new CornerRadius(10),
                    Margin = new Thickness(0, 2, 0, 2),
                };
                Grid.SetRow(tintFila, f);
                Grid.SetColumn(tintFila, 0);
                Grid.SetColumnSpan(tintFila, 8);
                MesRowsGrid.Children.Add(tintFila);
            }

            var numSemana = ISOWeek.GetWeekOfYear(filaInicio.ToDateTime(TimeOnly.MinValue));

            // El número de semana va dentro de un Border con fondo transparente:
            // un TextBlock suelto solo es clickeable sobre los glifos del texto,
            // lo que dejaba un blanco de click de un par de píxeles.
            var celdaSemana = new Border
            {
                Background = Brushes.Transparent,
                Cursor = Cursors.Hand,
                Child = new TextBlock
                {
                    Text = numSemana.ToString(),
                    FontSize = 10,
                    Foreground = (Brush)FindResource("TextFaint"),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                },
            };
            HabilitarClick(celdaSemana);
            celdaSemana.MouseLeftButtonUp += (_, _) =>
            {
                if (EsDobleClick($"mes-semana:{filaInicio:yyyy-MM-dd}"))
                {
                    _ = CambiarVistaAsync(Vista.Semana, semanaInicio: filaInicio);
                }
                else
                {
                    _semanaResaltadaInicio = _semanaResaltadaInicio == filaInicio ? null : filaInicio;
                    _diaResaltado = null;
                    RenderMes(mesActual, dias, agendas);
                }
            };
            Grid.SetRow(celdaSemana, f);
            Grid.SetColumn(celdaSemana, 0);
            MesRowsGrid.Children.Add(celdaSemana);

            for (var c = 0; c < 7; c++)
            {
                var dia = dias[f * 7 + c];
                var esDeEsteMes = dia.Month == mesActual.Month && dia.Year == mesActual.Year;
                var esHoy = dia == hoy;
                var esResaltado = _diaResaltado == dia;

                var cellStack = new StackPanel { Margin = new Thickness(4) };
                cellStack.Children.Add(new TextBlock
                {
                    Text = dia.Day.ToString(),
                    FontSize = 12,
                    Foreground = esDeEsteMes ? (Brush)FindResource("TextPrimary") : (Brush)FindResource("TextFaint"),
                });

                var eventosDia = agendas[f * 7 + c].Where(b => b.Kind != BlockKind.Libre).OrderBy(b => b.Start).ToList();
                foreach (var ev in eventosDia.Take(3))
                {
                    var chip = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 0) };
                    chip.Children.Add(new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Fill = ev.Kind == BlockKind.Fixed ? (Brush)FindResource("AccentFijo") : (Brush)FindResource("AccentFlexible"),
                        VerticalAlignment = VerticalAlignment.Center,
                        Margin = new Thickness(0, 0, 4, 0),
                    });
                    chip.Children.Add(new TextBlock
                    {
                        Text = ev.Title,
                        FontSize = 9.5,
                        Foreground = (Brush)FindResource("TextPrimary"),
                        TextTrimming = TextTrimming.CharacterEllipsis,
                    });
                    cellStack.Children.Add(chip);
                }

                if (eventosDia.Count > 3)
                {
                    cellStack.Children.Add(new TextBlock
                    {
                        Text = $"+{eventosDia.Count - 3} más",
                        FontSize = 9,
                        Foreground = (Brush)FindResource("TextFaint"),
                        Margin = new Thickness(0, 3, 0, 0),
                    });
                }

                var celda = new Border
                {
                    CornerRadius = new CornerRadius(10),
                    Background = esResaltado ? (Brush)FindResource("SelectedTint") : Brushes.Transparent,
                    BorderBrush = esHoy ? (Brush)FindResource("AccentFlexible") : Brushes.Transparent,
                    BorderThickness = new Thickness(esHoy ? 1.4 : 0),
                    Cursor = Cursors.Hand,
                    Child = cellStack,
                };

                HabilitarClick(celda);
                celda.MouseLeftButtonUp += (_, _) =>
                {
                    if (EsDobleClick($"mes-dia:{dia:yyyy-MM-dd}"))
                    {
                        _ = CambiarVistaAsync(Vista.Dia, fechaDia: dia);
                    }
                    else
                    {
                        _diaResaltado = _diaResaltado == dia ? null : dia;
                        _semanaResaltadaInicio = null;
                        RenderMes(mesActual, dias, agendas);
                    }
                };

                Grid.SetRow(celda, f);
                Grid.SetColumn(celda, c + 1);
                MesRowsGrid.Children.Add(celda);
            }
        }
    }
}
