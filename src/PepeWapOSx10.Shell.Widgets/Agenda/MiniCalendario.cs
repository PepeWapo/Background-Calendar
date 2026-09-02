using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using PepeWapOSx10.Dominio;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// El calendario chico del panel izquierdo: el mes de la fecha que está
/// mirando la agenda, con el día de hoy marcado y selección de día.
/// </summary>
/// <remarks>
/// Hasta ahora sus celdas eran <c>Border</c>s decorativos, sin cursor ni
/// handler: mostraba el mes pero no se podía seleccionar nada, que es lo que
/// se reportó como "el selector de Calendario no funciona". Ahora usa el mismo
/// gesto que las vistas Semana y Mes — un click resalta el día, dos lo abren
/// en la vista Día (<see cref="Interaccion.AlSeleccionar"/>).
/// </remarks>
internal sealed class MiniCalendario
{
    private const double LadoCelda = 26;

    private readonly TextBlock _mesTexto;
    private readonly TextBlock _anioTexto;
    private readonly Grid _grilla;
    private readonly Action<DateOnly> _abrirDia;
    private readonly DetectorDobleClick _dobleClick = new();

    private DateOnly _mesVisible = DateOnly.FromDateTime(DateTime.Today);
    private DateOnly? _diaResaltado;
    private bool _yaRenderizado;

    public MiniCalendario(TextBlock mesTexto, TextBlock anioTexto, Grid grilla, Action<DateOnly> abrirDia)
    {
        _mesTexto = mesTexto;
        _anioTexto = anioTexto;
        _grilla = grilla;
        _abrirDia = abrirDia;
    }

    /// <summary>
    /// Muestra el mes al que pertenece <paramref name="fecha"/>.
    /// </summary>
    /// <remarks>
    /// Si ya se está mostrando ese mes no hace nada: la vista Día lo llama en
    /// cada cambio de scroll para mantenerlo sincronizado con el día que está
    /// arriba, y volver a dibujar la grilla en cada gesto además borraría el
    /// día que el usuario acababa de seleccionar.
    /// </remarks>
    public void Mostrar(DateOnly fecha)
    {
        if (_yaRenderizado && _mesVisible.Year == fecha.Year && _mesVisible.Month == fecha.Month)
            return;

        _mesVisible = fecha;
        _diaResaltado = null;
        Render();
    }

    private void Render()
    {
        _yaRenderizado = true;
        _mesTexto.Text = FormatoEspanol.Mes(_mesVisible.Month);
        _anioTexto.Text = _mesVisible.Year.ToString();

        _grilla.Children.Clear();
        _grilla.RowDefinitions.Clear();
        _grilla.ColumnDefinitions.Clear();

        for (var i = 0; i < 7; i++)
            _grilla.ColumnDefinitions.Add(new ColumnDefinition());

        _grilla.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (var i = 0; i < 7; i++)
            Ubicar(Encabezado(FormatoEspanol.InicialesDeDia[i]), fila: 0, columna: i);

        var primerDia = new DateOnly(_mesVisible.Year, _mesVisible.Month, 1);
        var offset = SemanaIso.IndiceDeDia(primerDia);
        var diasEnMes = DateTime.DaysInMonth(_mesVisible.Year, _mesVisible.Month);

        for (var f = 0; f < (int)Math.Ceiling((offset + diasEnMes) / 7.0); f++)
            _grilla.RowDefinitions.Add(new RowDefinition());

        for (var numero = 1; numero <= diasEnMes; numero++)
        {
            var indice = offset + numero - 1;
            Ubicar(Celda(primerDia.AddDays(numero - 1)), fila: indice / 7 + 1, columna: indice % 7);
        }
    }

    private static TextBlock Encabezado(string inicial) => new()
    {
        Text = inicial,
        FontSize = 10.5,
        FontWeight = FontWeights.SemiBold,
        Foreground = Paleta.TextoApagado,
        HorizontalAlignment = HorizontalAlignment.Center,
        Margin = new Thickness(0, 0, 0, 7),
    };

    /// <remarks>
    /// Todas las celdas son el mismo <c>Border</c> de 26x26 y solo cambian de
    /// color: cuando el día de hoy era un <c>Border</c> y el resto
    /// <c>TextBlock</c>s sueltos, el círculo quedaba un par de píxeles
    /// desalineado respecto a los números planos de al lado.
    /// </remarks>
    private Border Celda(DateOnly dia)
    {
        var esHoy = dia == DateOnly.FromDateTime(DateTime.Today);
        var esResaltado = _diaResaltado == dia;

        var celda = new Border
        {
            Width = LadoCelda,
            Height = LadoCelda,
            CornerRadius = new CornerRadius(LadoCelda / 2),
            Background = esHoy ? Paleta.AcentoFlexible : esResaltado ? Paleta.TinteSeleccion : Brushes.Transparent,
            Cursor = Cursors.Hand,
            Child = new TextBlock
            {
                Text = dia.Day.ToString(),
                FontSize = 12,
                FontWeight = esHoy ? FontWeights.Bold : FontWeights.Normal,
                Foreground = esHoy ? Brushes.Black : Paleta.TextoPrimario,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            },
        };

        celda.AlSeleccionar(
            _dobleClick,
            $"mini:{dia:yyyy-MM-dd}",
            simple: () =>
            {
                _diaResaltado = _diaResaltado == dia ? null : dia;
                Render();
            },
            doble: () => _abrirDia(dia));

        return celda;
    }

    private void Ubicar(FrameworkElement elemento, int fila, int columna)
    {
        Grid.SetRow(elemento, fila);
        Grid.SetColumn(elemento, columna);
        _grilla.Children.Add(elemento);
    }
}
