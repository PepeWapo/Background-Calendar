using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Dominio.Interfaces;

public interface IFlexibleTaskRepository
{
    Task<IReadOnlyList<FlexibleTask>> ObtenerTodasAsync();

    Task<IReadOnlySet<string>> ObtenerYaAgendadasAsync();

    Task MarcarAgendadaAsync(string id, DateOnly fecha);
}
