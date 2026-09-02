using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PepeWapOSx10.Dominio;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// La vista Mes: la grilla del calendario con el número de semana ISO al
/// costado y chips de eventos por día. Un click resalta el día (o la semana,
/// si se clickea su número), dos navegan a la vista correspondiente.
/// </summary>
internal sealed class VistaMes
{
    /// <summary>Ancho de la columna del número de semana.</summary>
    private const double AnchoColumnaSemana = 34;

    /// <summary>Cuántos eventos se listan en una celda antes de resumir el resto como "+N más".</summary>
    private const int ChipsPorCelda = 3;

    private readonly Grid _encabezado;
    private readonly Grid _filas;
    private readonly TextBlock _etiquetaDeNavegacion;
    private readonly TextBlock _subtitulo;
    private readonly IAgendaService _agenda;
    private readonly Action<DateOnly> _abrirDia;
    private readonly Action<DateOnly> _abrirSemana;
    private readonly DetectorDobleClick _dobleClick = new();

    private DateOnly? _diaResaltado;
    private DateOnly? _semanaResaltada;

    public VistaMes(
        Grid encabezado,
        Grid filas,
        TextBlock etiquetaDeNavegacion,
        TextBlock subtitulo,
        IAgendaService agenda,
        Action<DateOnly> abrirDia,
        Action<DateOnly> abrirSemana)
    {
        _encabezado = encabezado;
        _filas = filas;
        _etiquetaDeNavegacion = etiquetaDeNavegacion;
        _subtitulo = subtitulo;
        _agenda = agenda;
        _abrirDia = abrirDia;
        _abrirSemana = abrirSemana;
    }

    /// <summary>El primer día del mes que se está mostrando.</summary>
    public DateOnly Mes { get; private set; } = PrimerDiaDe(DateOnly.FromDateTime(DateTime.Today));

    public async Task MostrarAsync(DateOnly mes)
    {
        Mes = PrimerDiaDe(mes);
        _diaResaltado = null;
        _semanaResaltada = null;

        try
        {
            var dias = DiasDeLaGrilla(Mes);
            Render(dias, await _agenda.ObtenerDiasAsync(dias));
        }
        catch (Exception ex)
        {
            _subtitulo.Text = $"No se pudo cargar el mes: {ex.Message}";
        }
    }

    public Task DesplazarAsync(int meses) => MostrarAsync(Mes.AddMonths(meses));

    public Task IrAHoyAsync() => MostrarAsync(DateOnly.FromDateTime(DateTime.Today));

    private static DateOnly PrimerDiaDe(DateOnly fecha) => new(fecha.Year, fecha.Month, 1);

    /// <summary>
    /// Las semanas completas que hacen falta para cubrir el mes: arranca el
    /// lunes de la semana del día 1 y termina el domingo de la del último día.
    /// </summary>
    private static List<DateOnly> DiasDeLaGrilla(DateOnly primerDiaMes)
    {
        var inicio = SemanaIso.Inicio(primerDiaMes);
        var ultimoDiaMes = primerDiaMes.AddDays(DateTime.DaysInMonth(primerDiaMes.Year, primerDiaMes.Month) - 1);
        var fin = ultimoDiaMes.AddDays(6 - SemanaIso.IndiceDeDia(ultimoDiaMes));

        return Enumerable.Range(0, fin.DayNumber - inicio.DayNumber + 1).Select(inicio.AddDays).ToList();
    }

    private void Render(IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        _etiquetaDeNavegacion.Text = $"{FormatoEspanol.Mes(Mes.Month)} {Mes.Year}";
        _subtitulo.Text = "Vista mensual";

        RenderEncabezado();
        RenderFilas(dias, agendas);
    }

    private void RenderEncabezado()
    {
        _encabezado.Children.Clear();
        _encabezado.ColumnDefinitions.Clear();
        ColumnasDeSemana(_encabezado);

        for (var i = 0; i < 7; i++)
        {
            var inicial = new TextBlock
            {
                Text = FormatoEspanol.InicialesDeDia[i],
                FontSize = 10.5,
                FontWeight = FontWeights.SemiBold,
                Foreground = Paleta.TextoApagado,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            Grid.SetColumn(inicial, i + 1);
            _encabezado.Children.Add(inicial);
        }
    }

    private void RenderFilas(IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        _filas.Children.Clear();
        _filas.RowDefinitions.Clear();
        _filas.ColumnDefinitions.Clear();
        ColumnasDeSemana(_filas);

        var cantidadDeFilas = dias.Count / 7;
        for (var f = 0; f < cantidadDeFilas; f++)
            _filas.RowDefinitions.Add(new RowDefinition());

        for (var f = 0; f < cantidadDeFilas; f++)
        {
            var inicioDeFila = dias[f * 7];

            if (_semanaResaltada == inicioDeFila)
                Ubicar(TinteDeFila(), f, columna: 0, columnas: 8);

            Ubicar(CeldaDeSemana(inicioDeFila, dias, agendas), f, columna: 0);

            for (var c = 0; c < 7; c++)
            {
                var indice = f * 7 + c;
                Ubicar(CeldaDeDia(dias[indice], agendas[indice], dias, agendas), f, columna: c + 1);
            }
        }
    }

    private static void ColumnasDeSemana(Grid grilla)
    {
        grilla.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(AnchoColumnaSemana) });
        for (var i = 0; i < 7; i++)
            grilla.ColumnDefinitions.Add(new ColumnDefinition());
    }

    private static Border TinteDeFila() => new()
    {
        Background = Paleta.TinteSeleccion,
        CornerRadius = new CornerRadius(10),
        Margin = new Thickness(0, 2, 0, 2),
    };

    /// <remarks>
    /// El número de semana va dentro de un <c>Border</c> con fondo transparente:
    /// un <c>TextBlock</c> suelto solo es clickeable sobre los glifos del texto,
    /// lo que dejaba un blanco de click de un par de píxeles.
    /// </remarks>
    private Border CeldaDeSemana(
        DateOnly inicioDeFila, IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        var celda = new Border
        {
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand,
            Child = new TextBlock
            {
                Text = ISOWeek.GetWeekOfYear(inicioDeFila.ToDateTime(TimeOnly.MinValue)).ToString(),
                FontSize = 10,
                Foreground = Paleta.TextoTenue,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        celda.AlSeleccionar(
            _dobleClick,
            $"mes-semana:{inicioDeFila:yyyy-MM-dd}",
            simple: () =>
            {
                _semanaResaltada = _semanaResaltada == inicioDeFila ? null : inicioDeFila;
                _diaResaltado = null;
                Render(dias, agendas);
            },
            doble: () => _abrirSemana(inicioDeFila));

        return celda;
    }

    private Border CeldaDeDia(
        DateOnly dia,
        IReadOnlyList<ScheduledBlock> agendaDelDia,
        IReadOnlyList<DateOnly> dias,
        IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        var esDeEsteMes = dia.Month == Mes.Month && dia.Year == Mes.Year;
        var esHoy = dia == DateOnly.FromDateTime(DateTime.Today);

        var contenido = new StackPanel { Margin = new Thickness(4) };
        contenido.Children.Add(new TextBlock
        {
            Text = dia.Day.ToString(),
            FontSize = 12,
            Foreground = esDeEsteMes ? Paleta.TextoPrimario : Paleta.TextoTenue,
        });

        var eventos = agendaDelDia.Where(b => b.Kind != BlockKind.Libre).OrderBy(b => b.Start).ToList();
        foreach (var evento in eventos.Take(ChipsPorCelda))
            contenido.Children.Add(Chip(evento));

        if (eventos.Count > ChipsPorCelda)
        {
            contenido.Children.Add(new TextBlock
            {
                Text = $"+{eventos.Count - ChipsPorCelda} más",
                FontSize = 9,
                Foreground = Paleta.TextoTenue,
                Margin = new Thickness(0, 3, 0, 0),
            });
        }

        var celda = new Border
        {
            CornerRadius = new CornerRadius(10),
            Background = _diaResaltado == dia ? Paleta.TinteSeleccion : Brushes.Transparent,
            BorderBrush = esHoy ? Paleta.AcentoFlexible : Brushes.Transparent,
            BorderThickness = new Thickness(esHoy ? 1.4 : 0),
            Cursor = Cursors.Hand,
            Child = contenido,
        };

        celda.AlSeleccionar(
            _dobleClick,
            $"mes-dia:{dia:yyyy-MM-dd}",
            simple: () =>
            {
                _diaResaltado = _diaResaltado == dia ? null : dia;
                _semanaResaltada = null;
                Render(dias, agendas);
            },
            doble: () => _abrirDia(dia));

        return celda;
    }

    private static StackPanel Chip(ScheduledBlock evento)
    {
        var chip = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 3, 0, 0) };
        chip.Children.Add(new Ellipse
        {
            Width = 5,
            Height = 5,
            Fill = LineaDeTiempo.ColorDe(evento.Kind),
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 4, 0),
        });
        chip.Children.Add(new TextBlock
        {
            Text = evento.Title,
            FontSize = 9.5,
            Foreground = Paleta.TextoPrimario,
            TextTrimming = TextTrimming.CharacterEllipsis,
        });
        return chip;
    }

    private void Ubicar(FrameworkElement elemento, int fila, int columna, int columnas = 1)
    {
        Grid.SetRow(elemento, fila);
        Grid.SetColumn(elemento, columna);
        if (columnas > 1)
            Grid.SetColumnSpan(elemento, columnas);
        _filas.Children.Add(elemento);
    }
}
