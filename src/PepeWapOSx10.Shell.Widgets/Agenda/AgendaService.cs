using PepeWapOSx10.Calendario;
using PepeWapOSx10.Datos;
using PepeWapOSx10.Dominio;
using PepeWapOSx10.Dominio.Interfaces;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <inheritdoc cref="IAgendaService"/>
internal sealed class AgendaService : IAgendaService
{
    /// <summary>
    /// Cuánto se reusa la última descarga del ICS antes de volver a pedirlo.
    /// </summary>
    /// <remarks>
    /// El calendario se lee bajando y parseando el ICS entero en cada consulta.
    /// El scroll continuo de la vista Día pide un día nuevo cada vez que el
    /// usuario llega a un extremo, así que sin caché una tarde de scroll son
    /// decenas de descargas del mismo archivo. Una ventana corta mantiene la
    /// agenda fresca (un evento nuevo aparece al minuto) sin repetir la
    /// descarga a cada gesto.
    /// </remarks>
    private static readonly TimeSpan VigenciaDelCalendario = TimeSpan.FromMinutes(1);

    /// <summary>
    /// Cuántos rangos distintos se recuerdan antes de vaciar la caché. El
    /// widget queda abierto días enteros y el scroll continuo pide un rango
    /// nuevo por día visitado, así que sin tope la caché crecería sola. Vaciar
    /// entera en vez de descartar la entrada más vieja alcanza: lo único que
    /// cuesta es una descarga.
    /// </summary>
    private const int MaximoDeRangosEnCache = 64;

    private readonly IFlexibleTaskRepository _tareas;
    private readonly ICalendarSource _calendario;
    private readonly Dictionary<(DateOnly Desde, DateOnly Hasta), (DateTime Momento, IReadOnlyList<FixedEvent> Eventos)> _cache = [];

    public AgendaService(AgendaDbContext contexto)
        : this(new SqliteFlexibleTaskRepository(contexto), new GoogleCalendarSource())
    {
    }

    public AgendaService(IFlexibleTaskRepository tareas, ICalendarSource calendario)
    {
        _tareas = tareas;
        _calendario = calendario;
    }

    public async Task<IReadOnlyList<ScheduledBlock>> ObtenerDiaAsync(DateOnly fecha)
    {
        var fijos = await FijosDelRangoAsync(fecha, fecha);
        var agenda = await ArmarAsync([fecha], fijos);

        // Solo se persiste "ya agendada" cuando se está mirando hoy de verdad.
        // Los días adyacentes que trae el scroll continuo, igual que Semana/Mes,
        // son preview y no deben tocar este flag global (ver plan: invariante de
        // MarcarAgendadaAsync).
        if (fecha == Hoy)
        {
            foreach (var bloque in agenda[0].Where(b => b.Kind == BlockKind.Flexible))
                await _tareas.MarcarAgendadaAsync(bloque.TaskId!, fecha);
        }

        return agenda[0];
    }

    public async Task<IReadOnlyList<IReadOnlyList<ScheduledBlock>>> ObtenerDiasAsync(IReadOnlyList<DateOnly> dias)
    {
        if (dias.Count == 0)
            return [];

        var fijos = await FijosDelRangoAsync(dias.Min(), dias.Max());
        return await ArmarAsync(dias, fijos);
    }

    private static DateOnly Hoy => DateOnly.FromDateTime(DateTime.Today);

    private async Task<IReadOnlyList<IReadOnlyList<ScheduledBlock>>> ArmarAsync(
        IReadOnlyList<DateOnly> dias, IReadOnlyList<FixedEvent> fijos)
    {
        var tareas = await _tareas.ObtenerTodasAsync();
        var yaAgendadas = await _tareas.ObtenerYaAgendadasAsync();

        return dias
            .Select(dia => (IReadOnlyList<ScheduledBlock>)Scheduler.ArmarAgenda(DelDia(fijos, dia), tareas, dia, yaAgendadas))
            .ToList();
    }

    /// <summary>Los eventos fijos que tocan un día, incluidos los que vienen del día anterior.</summary>
    private static IReadOnlyList<FixedEvent> DelDia(IReadOnlyList<FixedEvent> fijos, DateOnly fecha)
    {
        var inicioDia = fecha.ToDateTime(TimeOnly.MinValue);
        var finDia = inicioDia.AddDays(1);
        return fijos.Where(f => f.Start < finDia && f.End > inicioDia).ToList();
    }

    private async Task<IReadOnlyList<FixedEvent>> FijosDelRangoAsync(DateOnly desde, DateOnly hasta)
    {
        var clave = (desde, hasta);
        if (_cache.TryGetValue(clave, out var guardado) && DateTime.UtcNow - guardado.Momento < VigenciaDelCalendario)
            return guardado.Eventos;

        var eventos = await _calendario.ObtenerFijosDelRangoAsync(desde, hasta);

        if (_cache.Count >= MaximoDeRangosEnCache)
            _cache.Clear();

        _cache[clave] = (DateTime.UtcNow, eventos);
        return eventos;
    }
}
