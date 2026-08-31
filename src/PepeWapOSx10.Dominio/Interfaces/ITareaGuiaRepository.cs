using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Dominio.Interfaces;

public interface ITareaGuiaRepository
{
    Task<IReadOnlyList<TareaGuia>> ObtenerTodasAsync();

    Task MarcarHechaAsync(string id, DateOnly fecha);

    /// <summary>Deshace un marcado (por ejemplo, un click por error).</summary>
    Task DesmarcarAsync(string id);

    Task CrearAsync(TareaGuia tarea);

    /// <summary>Actualiza título, categoría y repetición. No toca el historial.</summary>
    Task ActualizarAsync(TareaGuia tarea);

    Task EliminarAsync(string id);
}
