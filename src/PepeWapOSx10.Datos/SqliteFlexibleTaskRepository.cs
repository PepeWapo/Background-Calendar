using Microsoft.Data.Sqlite;
using PepeWapOSx10.Dominio.Interfaces;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Datos;

public sealed class SqliteFlexibleTaskRepository(AgendaDbContext contexto) : IFlexibleTaskRepository
{
    public Task<IReadOnlyList<FlexibleTask>> ObtenerTodasAsync()
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText =
            """
            SELECT id, title, duration_min, priority, dias_permitidos, hora_min, hora_max, recurrente
            FROM flexible_tasks;
            """;

        var tareas = new List<FlexibleTask>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            var diasPermitidos = lector.IsDBNull(4)
                ? null
                : (IReadOnlyList<DayOfWeek>)lector.GetString(4)
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(indice => IndiceADia(int.Parse(indice)))
                    .ToList();

            tareas.Add(new FlexibleTask(
                Id: lector.GetString(0),
                Title: lector.GetString(1),
                DurationMin: lector.GetInt32(2),
                Priority: lector.GetInt32(3),
                DiasPermitidos: diasPermitidos,
                HoraMin: lector.IsDBNull(5) ? null : TimeOnly.Parse(lector.GetString(5)),
                HoraMax: lector.IsDBNull(6) ? null : TimeOnly.Parse(lector.GetString(6)),
                Recurrente: lector.GetInt32(7) != 0));
        }

        return Task.FromResult<IReadOnlyList<FlexibleTask>>(tareas);
    }

    public Task<IReadOnlySet<string>> ObtenerYaAgendadasAsync()
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT id FROM flexible_tasks WHERE scheduled_on IS NOT NULL;";

        var ids = new HashSet<string>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
            ids.Add(lector.GetString(0));

        return Task.FromResult<IReadOnlySet<string>>(ids);
    }

    public Task MarcarAgendadaAsync(string id, DateOnly fecha)
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE flexible_tasks SET scheduled_on = $fecha WHERE id = $id;";
        comando.Parameters.AddWithValue("$fecha", fecha.ToString("yyyy-MM-dd"));
        comando.Parameters.AddWithValue("$id", id);
        comando.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    private static DayOfWeek IndiceADia(int indice) => (DayOfWeek)((indice + 1) % 7);
}
