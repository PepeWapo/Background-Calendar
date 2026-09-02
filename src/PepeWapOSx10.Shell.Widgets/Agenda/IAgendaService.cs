using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Arma la agenda ya resuelta (fijos + flexibles + huecos) para las vistas.
/// </summary>
/// <remarks>
/// Las vistas dependen de esta abstracción y no de <c>Datos</c>/<c>Calendario</c>:
/// antes cada una construía a mano el <c>AgendaDbContext</c>, los repositorios
/// y el <c>GoogleCalendarSource</c>, con lo cual la UI conocía la
/// infraestructura concreta y el mismo bloque de cableado estaba copiado tres
/// veces. Acá lo único que ve la UI es "dame el día" / "dame estos días".
/// </remarks>
internal interface IAgendaService
{
    /// <summary>
    /// La agenda de un día concreto. Si es hoy, además persiste qué tareas
    /// flexibles quedaron agendadas — es la única entrada que lo hace.
    /// </summary>
    Task<IReadOnlyList<ScheduledBlock>> ObtenerDiaAsync(DateOnly fecha);

    /// <summary>
    /// La agenda de varios días, en el mismo orden en que se pidieron. Es de
    /// solo lectura: nunca marca tareas flexibles como agendadas.
    /// </summary>
    Task<IReadOnlyList<IReadOnlyList<ScheduledBlock>>> ObtenerDiasAsync(IReadOnlyList<DateOnly> dias);
}
