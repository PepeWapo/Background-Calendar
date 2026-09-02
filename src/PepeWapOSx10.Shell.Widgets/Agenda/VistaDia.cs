using System.Windows.Controls;
using System.Windows.Input;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// La vista Día: una línea de tiempo vertical con el eje de horas y scroll
/// continuo entre días — al llegar a un extremo se carga el día contiguo y se
/// sigue de largo, sin cortes ni botones de navegación.
/// </summary>
internal sealed class VistaDia
{
    /// <summary>
    /// Tope de días simultáneos en el lienzo. El scroll continuo carga días
    /// indefinidamente, así que hay que descartar por el extremo opuesto para
    /// que el Canvas no crezca sin límite.
    /// </summary>
    private const int MaxDiasCargados = 9;

    /// <summary>Tolerancia en píxeles para dar el scroll por "llegado al extremo".</summary>
    private const double MargenDeExtremo = 1.0;

    private sealed record DiaCargado(DateOnly Fecha, IReadOnlyList<ScheduledBlock> Bloques);

    private readonly ScrollViewer _scroll;
    private readonly Canvas _canvas;
    private readonly TextBlock _encabezadoDia;
    private readonly TextBlock _encabezadoMes;
    private readonly TextBlock _encabezadoAnio;
    private readonly TextBlock _pie;
    private readonly IAgendaService _agenda;
    private readonly Action<DateOnly> _alMostrarDia;

    private readonly List<DiaCargado> _diasCargados = [];
    private bool _cargandoDiaAdyacente;

    public VistaDia(
        ScrollViewer scroll,
        Canvas canvas,
        TextBlock encabezadoDia,
        TextBlock encabezadoMes,
        TextBlock encabezadoAnio,
        TextBlock pie,
        IAgendaService agenda,
        Action<DateOnly> alMostrarDia)
    {
        _scroll = scroll;
        _canvas = canvas;
        _encabezadoDia = encabezadoDia;
        _encabezadoMes = encabezadoMes;
        _encabezadoAnio = encabezadoAnio;
        _pie = pie;
        _agenda = agenda;
        _alMostrarDia = alMostrarDia;

        _scroll.PreviewMouseWheel += AlRodarLaRueda;
        _scroll.ScrollChanged += (_, _) => ActualizarEncabezado();
    }

    /// <summary>Deja el lienzo con un solo día y el scroll cerca de la hora relevante.</summary>
    public async Task MostrarAsync(DateOnly fecha)
    {
        try
        {
            var bloques = await _agenda.ObtenerDiaAsync(fecha);

            _diasCargados.Clear();
            _diasCargados.Add(new DiaCargado(fecha, bloques));
            Render();

            _scroll.UpdateLayout();
            _scroll.ScrollToVerticalOffset(OffsetInicialDe(fecha));
        }
        catch (Exception ex)
        {
            _pie.Text = $"No se pudo cargar la agenda: {ex.Message}";
        }
    }

    private void AlRodarLaRueda(object sender, MouseWheelEventArgs e)
    {
        if (e.Delta > 0 && _scroll.VerticalOffset <= MargenDeExtremo)
            _ = CargarDiaAdyacenteAsync(haciaAtras: true);
        else if (e.Delta < 0 && _scroll.VerticalOffset >= _scroll.ScrollableHeight - MargenDeExtremo)
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

            var dia = new DiaCargado(fecha, await _agenda.ObtenerDiaAsync(fecha));
            var offsetPrevio = _scroll.VerticalOffset;
            var desplazamiento = 0.0;

            if (haciaAtras)
            {
                _diasCargados.Insert(0, dia);
                // Todo lo anterior baja una banda entera: hay que compensar el
                // scroll o el contenido salta bajo el cursor.
                desplazamiento += LineaDeTiempo.AlturaDia;

                if (_diasCargados.Count > MaxDiasCargados)
                    _diasCargados.RemoveAt(_diasCargados.Count - 1);
            }
            else
            {
                _diasCargados.Add(dia);

                if (_diasCargados.Count > MaxDiasCargados)
                {
                    _diasCargados.RemoveAt(0);
                    desplazamiento -= LineaDeTiempo.AlturaDia;
                }
            }

            Render();
            _scroll.UpdateLayout();
            _scroll.ScrollToVerticalOffset(offsetPrevio + desplazamiento);
        }
        catch (Exception ex)
        {
            _pie.Text = $"No se pudo cargar el día contiguo: {ex.Message}";
        }
        finally
        {
            _cargandoDiaAdyacente = false;
        }
    }

    /// <summary>Arranca cerca de la hora actual si es hoy; si no, a la mañana.</summary>
    private double OffsetInicialDe(DateOnly fecha)
    {
        const double MinutosDeContextoAntes = 90;
        const double HoraDeArranqueOtroDia = 7 * 60;

        var minutos = fecha == DateOnly.FromDateTime(DateTime.Today)
            ? DateTime.Now.TimeOfDay.TotalMinutes - MinutosDeContextoAntes
            : HoraDeArranqueOtroDia;

        var maximo = Math.Max(LineaDeTiempo.AlturaDia - _scroll.ViewportHeight, 0);
        return Math.Clamp(LineaDeTiempo.MinutosAPixeles(minutos), 0, maximo);
    }

    /// <summary>El header y el pie siguen al día que ocupa el tope de lo visible.</summary>
    private void ActualizarEncabezado()
    {
        if (_diasCargados.Count == 0)
            return;

        var indice = Math.Clamp((int)(_scroll.VerticalOffset / LineaDeTiempo.AlturaDia), 0, _diasCargados.Count - 1);
        var dia = _diasCargados[indice];

        _encabezadoDia.Text = FormatoEspanol.DiaYNumero(dia.Fecha);
        _encabezadoMes.Text = FormatoEspanol.Mes(dia.Fecha.Month);
        _encabezadoAnio.Text = dia.Fecha.Year.ToString();

        var ocupados = dia.Bloques.Count(b => b.Kind != BlockKind.Libre);
        var libres = dia.Bloques.Count(b => b.Kind == BlockKind.Libre);
        _pie.Text = $"{ocupados} bloques ocupados · {libres} libres";

        _alMostrarDia(dia.Fecha);
    }

    private void Render()
    {
        _canvas.Children.Clear();
        _canvas.Height = _diasCargados.Count * LineaDeTiempo.AlturaDia;

        var anchoContenido = _canvas.Width - LineaDeTiempo.ContenidoX;
        LineaDeTiempo.LineaVertical(_canvas, LineaDeTiempo.EjeX, 0, _canvas.Height, Paleta.BordePanel);

        for (var i = 0; i < _diasCargados.Count; i++)
            RenderBanda(_diasCargados[i], i * LineaDeTiempo.AlturaDia, anchoContenido);

        ActualizarEncabezado();
    }

    private void RenderBanda(DiaCargado dia, double offsetY, double anchoContenido)
    {
        LineaDeTiempo.DibujarHoras(_canvas, offsetY, LineaDeTiempo.ContenidoX, anchoContenido, lineaEnLaPrimera: false);
        RenderSeparadorDeDia(dia.Fecha, offsetY, anchoContenido);

        var inicioDia = dia.Fecha.ToDateTime(TimeOnly.MinValue);
        foreach (var bloque in dia.Bloques)
        {
            // Un bloque puede cruzar la medianoche (p. ej. "Dormir"): se recorta
            // a la banda de su propio día para no pisar la del día vecino.
            var desde = Math.Clamp((bloque.Start - inicioDia).TotalMinutes, 0, 24 * 60);
            var hasta = Math.Clamp((bloque.End - inicioDia).TotalMinutes, 0, 24 * 60);
            if (hasta <= desde)
                continue;

            LineaDeTiempo.DibujarBloque(
                _canvas,
                bloque,
                LineaDeTiempo.ContenidoX,
                offsetY + LineaDeTiempo.MinutosAPixeles(desde),
                Math.Max(LineaDeTiempo.MinutosAPixeles(hasta - desde), LineaDeTiempo.AltoMinimoBloque),
                anchoContenido);
        }

        if (dia.Fecha == DateOnly.FromDateTime(DateTime.Today))
        {
            var ahora = DateTime.Now;
            LineaDeTiempo.DibujarAhora(
                _canvas,
                LineaDeTiempo.EjeX,
                offsetY + LineaDeTiempo.MinutosAPixeles(ahora.TimeOfDay.TotalMinutes),
                anchoContenido,
                ahora);
        }
    }

    private void RenderSeparadorDeDia(DateOnly fecha, double offsetY, double anchoContenido)
    {
        LineaDeTiempo.LineaHorizontal(_canvas, LineaDeTiempo.ContenidoX, anchoContenido, offsetY, 1, Paleta.BordePanel);
        LineaDeTiempo.Posicionar(
            _canvas,
            LineaDeTiempo.Pastilla(FormatoEspanol.FechaLarga(fecha).ToUpperInvariant(), Paleta.TextoApagado, 10, new System.Windows.Thickness(8, 2, 8, 2)),
            LineaDeTiempo.ContenidoX,
            offsetY + 3);
    }
}
