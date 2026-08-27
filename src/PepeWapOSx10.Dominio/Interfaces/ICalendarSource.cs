using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Dominio.Interfaces;

public interface ICalendarSource
{
    Task<IReadOnlyList<FixedEvent>> ObtenerFijosDelDiaAsync(DateOnly fecha);
}
