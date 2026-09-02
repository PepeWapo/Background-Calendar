using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PepeWapOSx10.Dominio;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// La vista Semana: una grilla de 24 horas × 7 días, con los bloques del mismo
/// horario alineados en la misma fila entre columnas. Un click resalta el día,
/// dos lo abren en la vista Día.
/// </summary>
internal sealed class VistaSemana
{
    /// <summary>Aire entre columnas para que dos bloques contiguos no se toquen.</summary>
    private const double SeparacionEntreColumnas = 4;

    /// <summary>Alto mínimo de un bloque en semana: más chico que esto no entra ni el título.</summary>
    private const double AltoMinimoBloque = 17;

    private readonly Grid _encabezado;
    private readonly ScrollViewer _scroll;
    private readonly Canvas _canvas;
    private readonly TextBlock _etiquetaDeNavegacion;
    private readonly TextBlock _subtitulo;
    private readonly IAgendaService _agenda;
    private readonly Action<DateOnly> _abrirDia;
    private readonly DetectorDobleClick _dobleClick = new();

    private DateOnly? _diaResaltado;

    public VistaSemana(
        Grid encabezado,
        ScrollViewer scroll,
        Canvas canvas,
        TextBlock etiquetaDeNavegacion,
        TextBlock subtitulo,
        IAgendaService agenda,
        Action<DateOnly> abrirDia)
    {
        _encabezado = encabezado;
        _scroll = scroll;
        _canvas = canvas;
        _etiquetaDeNavegacion = etiquetaDeNavegacion;
        _subtitulo = subtitulo;
        _agenda = agenda;
        _abrirDia = abrirDia;
    }

    /// <summary>El lunes de la semana que se está mostrando.</summary>
    public DateOnly Inicio { get; private set; } = SemanaIso.Inicio(DateOnly.FromDateTime(DateTime.Today));

    public async Task MostrarAsync(DateOnly inicioDeSemana)
    {
        Inicio = SemanaIso.Inicio(inicioDeSemana);
        _diaResaltado = null;

        try
        {
            var dias = Enumerable.Range(0, 7).Select(Inicio.AddDays).ToList();
            Render(dias, await _agenda.ObtenerDiasAsync(dias));
        }
        catch (Exception ex)
        {
            _subtitulo.Text = $"No se pudo cargar la semana: {ex.Message}";
        }
    }

    public Task DesplazarAsync(int semanas) => MostrarAsync(Inicio.AddDays(7 * semanas));

    public Task IrAHoyAsync() => MostrarAsync(DateOnly.FromDateTime(DateTime.Today));

    private void Render(IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        _etiquetaDeNavegacion.Text = FormatoEspanol.RangoDeSemana(dias[0], dias[6]);
        _subtitulo.Text = "Vista semanal";

        RenderEncabezado(dias, agendas);
        RenderGrilla(dias, agendas);

        if (_scroll.VerticalOffset == 0)
            _scroll.ScrollToVerticalOffset(LineaDeTiempo.MinutosAPixeles(6 * 60));
    }

    private void RenderEncabezado(IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        _encabezado.Children.Clear();
        _encabezado.ColumnDefinitions.Clear();
        _encabezado.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(LineaDeTiempo.EjeX) });
        for (var i = 0; i < 7; i++)
            _encabezado.ColumnDefinitions.Add(new ColumnDefinition());

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        for (var i = 0; i < 7; i++)
        {
            var dia = dias[i];
            var celda = CeldaDeDia(dia, FormatoEspanol.InicialesDeDia[i], esHoy: dia == hoy);

            celda.AlSeleccionar(
                _dobleClick,
                $"semana-header:{dia:yyyy-MM-dd}",
                simple: () =>
                {
                    _diaResaltado = _diaResaltado == dia ? null : dia;
                    Render(dias, agendas);
                },
                doble: () => _abrirDia(dia));

            Grid.SetColumn(celda, i + 1);
            _encabezado.Children.Add(celda);
        }
    }

    private Border CeldaDeDia(DateOnly dia, string inicial, bool esHoy)
    {
        var pila = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
        pila.Children.Add(new TextBlock
        {
            Text = inicial,
            FontSize = 10,
            FontWeight = FontWeights.SemiBold,
            Foreground = Paleta.TextoApagado,
            HorizontalAlignment = HorizontalAlignment.Center,
        });
        pila.Children.Add(esHoy
            ? new Border
            {
                Width = 24,
                Height = 24,
                CornerRadius = new CornerRadius(12),
                Background = Paleta.AcentoFlexible,
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
                Foreground = Paleta.TextoPrimario,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 3, 0, 0),
            });

        return new Border
        {
            CornerRadius = new CornerRadius(9),
            Background = _diaResaltado == dia ? Paleta.TinteSeleccion : Brushes.Transparent,
            Padding = new Thickness(4, 6, 4, 6),
            Cursor = Cursors.Hand,
            Child = pila,
        };
    }

    /// <remarks>
    /// Las 24 filas son fijas y comunes a las siete columnas: así el mismo
    /// horario cae siempre a la misma altura y la semana se lee de un vistazo,
    /// en vez de que cada día empiece donde arranque su primer evento.
    /// </remarks>
    private void RenderGrilla(IReadOnlyList<DateOnly> dias, IReadOnlyList<IReadOnlyList<ScheduledBlock>> agendas)
    {
        _canvas.Children.Clear();
        _canvas.Height = LineaDeTiempo.AlturaDia;

        var anchoColumna = (_canvas.Width - LineaDeTiempo.EjeX) / 7.0;

        if (_diaResaltado is { } resaltado)
        {
            var indice = resaltado.DayNumber - dias[0].DayNumber;
            if (indice is >= 0 and < 7)
                LineaDeTiempo.Rectangulo(_canvas, LineaDeTiempo.EjeX + indice * anchoColumna, 0, anchoColumna, _canvas.Height, Paleta.TinteSeleccion);
        }

        LineaDeTiempo.DibujarHoras(_canvas, 0, LineaDeTiempo.EjeX, _canvas.Width - LineaDeTiempo.EjeX, lineaEnLaPrimera: true);

        var hoy = DateOnly.FromDateTime(DateTime.Today);

        for (var i = 0; i < 7; i++)
        {
            var left = LineaDeTiempo.EjeX + i * anchoColumna;

            foreach (var bloque in agendas[i].Where(b => b.Kind != BlockKind.Libre))
            {
                var top = LineaDeTiempo.MinutosAPixeles(bloque.Start.Hour * 60 + bloque.Start.Minute);
                var alto = Math.Max(LineaDeTiempo.MinutosAPixeles((bloque.End - bloque.Start).TotalMinutes), AltoMinimoBloque);
                LineaDeTiempo.DibujarBloque(
                    _canvas, bloque, left + SeparacionEntreColumnas / 2, top, alto,
                    anchoColumna - SeparacionEntreColumnas, forzarCompacto: true);
            }

            if (dias[i] == hoy)
            {
                var ahora = DateTime.Now;
                LineaDeTiempo.LineaHorizontal(
                    _canvas, left, anchoColumna,
                    LineaDeTiempo.MinutosAPixeles(ahora.Hour * 60 + ahora.Minute), 1.5, Paleta.AcentoFlexible);
            }
        }
    }
}
