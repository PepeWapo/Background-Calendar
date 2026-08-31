using Microsoft.Data.Sqlite;

namespace PepeWapOSx10.Datos;

public sealed class AgendaDbContext
{
    private readonly string _connectionString;

    public AgendaDbContext(string? rutaBaseDeDatos = null)
    {
        rutaBaseDeDatos ??= Path.Combine(AppContext.BaseDirectory, "agenda.db");
        _connectionString = $"Data Source={rutaBaseDeDatos}";
    }

    public SqliteConnection AbrirConexion()
    {
        var conexion = new SqliteConnection(_connectionString);
        conexion.Open();
        return conexion;
    }

    public void Inicializar()
    {
        using var conexion = AbrirConexion();

        using (var crearTablas = conexion.CreateCommand())
        {
            crearTablas.CommandText =
                """
                CREATE TABLE IF NOT EXISTS flexible_tasks (
                    id TEXT PRIMARY KEY,
                    title TEXT NOT NULL,
                    duration_min INTEGER NOT NULL,
                    priority INTEGER NOT NULL,
                    dias_permitidos TEXT NULL,
                    hora_min TEXT NULL,
                    hora_max TEXT NULL,
                    recurrente INTEGER NOT NULL DEFAULT 0,
                    scheduled_on TEXT NULL
                );

                CREATE TABLE IF NOT EXISTS tareas_guia (
                    id TEXT PRIMARY KEY,
                    title TEXT NOT NULL,
                    categoria TEXT NOT NULL,
                    hecha_hoy INTEGER NOT NULL DEFAULT 0,
                    ultima_vez TEXT NULL,
                    repeticion TEXT NOT NULL DEFAULT 'Unica'
                );
                """;
            crearTablas.ExecuteNonQuery();
        }

        MigrarRepeticionDeTareasGuia(conexion);
        SembrarFlexibleTasks(conexion);
        SembrarTareasGuia(conexion);
    }

    /// <summary>
    /// Agrega la columna <c>repeticion</c> a bases creadas antes de que la guía
    /// tuviera recurrencia.
    /// </summary>
    /// <remarks>
    /// Las filas que ya existían se pasan a <c>Semanal</c>: son las tareas
    /// sembradas (limpieza, higiene, entrenamiento), que bajo el esquema viejo
    /// se marcaban con un <c>hecha_hoy</c> que nunca se reseteaba. Semanal es la
    /// interpretación más cercana y no degenerada; el usuario puede cambiarla
    /// desde el panel de Guía.
    /// </remarks>
    private static void MigrarRepeticionDeTareasGuia(SqliteConnection conexion)
    {
        if (TieneColumna(conexion, "tareas_guia", "repeticion"))
            return;

        using var migrar = conexion.CreateCommand();
        migrar.CommandText =
            """
            ALTER TABLE tareas_guia ADD COLUMN repeticion TEXT NOT NULL DEFAULT 'Unica';
            UPDATE tareas_guia SET repeticion = 'Semanal';
            """;
        migrar.ExecuteNonQuery();
    }

    private static bool TieneColumna(SqliteConnection conexion, string tabla, string columna)
    {
        using var comando = conexion.CreateCommand();
        // PRAGMA no admite parámetros para el nombre de tabla; el valor es un
        // literal de este mismo archivo, nunca entrada del usuario.
        comando.CommandText = $"PRAGMA table_info({tabla});";

        using var lector = comando.ExecuteReader();
        while (lector.Read())
        {
            if (string.Equals(lector.GetString(1), columna, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static void SembrarFlexibleTasks(SqliteConnection conexion)
    {
        using var contarComando = conexion.CreateCommand();
        contarComando.CommandText = "SELECT COUNT(*) FROM flexible_tasks;";
        var cantidad = (long)contarComando.ExecuteScalar()!;
        if (cantidad > 0)
            return;

        using var insertar = conexion.CreateCommand();
        insertar.CommandText =
            """
            INSERT INTO flexible_tasks
                (id, title, duration_min, priority, dias_permitidos, hora_min, hora_max, recurrente)
            VALUES
                ('platzi', 'Platzi', 60, 1, '0,1,2,3,4', '08:30', '12:30', 1),
                ('proyectos-personales', 'Proyectos personales', 60, 2, '0,1,2,3,4', '08:30', '12:30', 1);
            """;
        insertar.ExecuteNonQuery();
    }

    private static void SembrarTareasGuia(SqliteConnection conexion)
    {
        using var contarComando = conexion.CreateCommand();
        contarComando.CommandText = "SELECT COUNT(*) FROM tareas_guia;";
        var cantidad = (long)contarComando.ExecuteScalar()!;
        if (cantidad > 0)
            return;

        using var insertar = conexion.CreateCommand();
        insertar.CommandText =
            """
            INSERT INTO tareas_guia (id, title, categoria, repeticion)
            VALUES
                ('limpiar-cocina', 'Limpiar cocina', 'Limpieza', 'Semanal'),
                ('sacar-basura', 'Sacar la basura', 'Limpieza', 'Semanal'),
                ('ducha-post-entreno', 'Ducha post-entreno', 'Higiene', 'Semanal'),
                ('entrenar-piernas', 'Entrenar piernas', 'Entrenamiento', 'Semanal'),
                ('estiramiento', 'Estiramiento', 'Entrenamiento', 'Semanal'),
                ('entrenar-pecho', 'Entrenar pecho', 'Entrenamiento', 'Semanal');
            """;
        insertar.ExecuteNonQuery();
    }
}
