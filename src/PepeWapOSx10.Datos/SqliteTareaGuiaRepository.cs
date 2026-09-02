using Microsoft.Data.Sqlite;
using PepeWapOSx10.Dominio;
using PepeWapOSx10.Dominio.Interfaces;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Datos;

public sealed class SqliteTareaGuiaRepository(AgendaDbContext contexto) : ITareaGuiaRepository
{
    public Task<IReadOnlyList<TareaGuia>> ObtenerTodasAsync()
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText =
            "SELECT id, title, categoria, ultima_vez, repeticion FROM tareas_guia ORDER BY categoria, title;";

        var hoy = DateOnly.FromDateTime(DateTime.Today);
        var tareas = new List<TareaGuia>();
        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            var ultimaVez = lector.IsDBNull(3) ? (DateOnly?)null : DateOnly.Parse(lector.GetString(3));
            var repeticion = LeerRepeticion(lector.IsDBNull(4) ? null : lector.GetString(4));

            tareas.Add(new TareaGuia(
                Id: lector.GetString(0),
                Title: lector.GetString(1),
                Categoria: lector.GetString(2),
                Hecha: SigueHecha(ultimaVez, repeticion, hoy),
                UltimaVez: ultimaVez,
                Repeticion: repeticion));
        }

        return Task.FromResult<IReadOnlyList<TareaGuia>>(tareas);
    }

    /// <summary>
    /// Decide si una tarea sigue contando como hecha en el período vigente.
    /// </summary>
    /// <remarks>
    /// No hay ningún reset programado ni tarea de medianoche: el estado se
    /// deriva de <c>ultima_vez</c> cada vez que se lee, así que una tarea
    /// semanal/mensual vuelve sola a pendiente en cuanto cambia el período.
    /// </remarks>
    public static bool SigueHecha(DateOnly? ultimaVez, Repeticion repeticion, DateOnly hoy)
    {
        if (ultimaVez is not { } fecha)
            return false;

        return repeticion switch
        {
            Repeticion.Semanal => SemanaIso.Inicio(fecha) == SemanaIso.Inicio(hoy),
            Repeticion.Mensual => fecha.Year == hoy.Year && fecha.Month == hoy.Month,
            _ => true,
        };
    }

    private static Repeticion LeerRepeticion(string? valor) =>
        Enum.TryParse<Repeticion>(valor, ignoreCase: true, out var repeticion) ? repeticion : Repeticion.Unica;

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

    public Task DesmarcarAsync(string id)
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "UPDATE tareas_guia SET hecha_hoy = 0, ultima_vez = NULL WHERE id = $id;";
        comando.Parameters.AddWithValue("$id", id);
        comando.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task CrearAsync(TareaGuia tarea)
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText =
            """
            INSERT INTO tareas_guia (id, title, categoria, hecha_hoy, ultima_vez, repeticion)
            VALUES ($id, $title, $categoria, 0, NULL, $repeticion);
            """;
        AgregarDatos(comando, tarea);
        comando.Parameters.AddWithValue("$id", tarea.Id);
        comando.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task ActualizarAsync(TareaGuia tarea)
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText =
            """
            UPDATE tareas_guia
            SET title = $title, categoria = $categoria, repeticion = $repeticion
            WHERE id = $id;
            """;
        AgregarDatos(comando, tarea);
        comando.Parameters.AddWithValue("$id", tarea.Id);
        comando.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    public Task EliminarAsync(string id)
    {
        using var conexion = contexto.AbrirConexion();
        using var comando = conexion.CreateCommand();
        comando.CommandText = "DELETE FROM tareas_guia WHERE id = $id;";
        comando.Parameters.AddWithValue("$id", id);
        comando.ExecuteNonQuery();

        return Task.CompletedTask;
    }

    private static void AgregarDatos(SqliteCommand comando, TareaGuia tarea)
    {
        comando.Parameters.AddWithValue("$title", tarea.Title);
        comando.Parameters.AddWithValue("$categoria", tarea.Categoria);
        comando.Parameters.AddWithValue("$repeticion", tarea.Repeticion.ToString());
    }
}
