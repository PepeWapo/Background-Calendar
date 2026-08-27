using PepeWapOSx10.Dominio.Interfaces;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Datos;

public sealed class SqliteTareaGuiaRepository(AgendaDbContext contexto) : ITareaGuiaRepository
{
    public Task<IReadOnlyList<TareaGuia>> ObtenerTodasAsync()
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "SELECT id, title, categoria, hecha_hoy, ultima_vez FROM tareas_guia;";

        var tareas = new List<TareaGuia>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            tareas.Add(new TareaGuia(
                Id: lector.GetString(0),
                Title: lector.GetString(1),
                Categoria: lector.GetString(2),
                HechaHoy: lector.GetInt32(3) != 0,
                UltimaVez: lector.IsDBNull(4) ? null : DateOnly.Parse(lector.GetString(4))));
        }

        return Task.FromResult<IReadOnlyList<TareaGuia>>(tareas);
    }

    public Task MarcarHechaAsync(string id, DateOnly fecha)
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText =
            "UPDATE tareas_guia SET hecha_hoy = 1, ultima_vez = $fecha WHERE id = $id;";
        comando.Parameters.AddWithValue("$fecha", fecha.ToString("yyyy-MM-dd"));
        comando.Parameters.AddWithValue("$id", id);
        comando.ExecuteNonQuery();

        return Task.CompletedTask;
    }
}
