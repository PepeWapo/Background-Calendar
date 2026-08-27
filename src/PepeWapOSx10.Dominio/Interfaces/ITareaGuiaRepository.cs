using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Dominio.Interfaces;

public interface ITareaGuiaRepository
{
    Task<IReadOnlyList<TareaGuia>> ObtenerTodasAsync();

    Task MarcarHechaAsync(string id, DateOnly fecha);
}
