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
                    ultima_vez TEXT NULL
                );
                """;
            crearTablas.ExecuteNonQuery();
        }

        SembrarFlexibleTasks(conexion);
        SembrarTareasGuia(conexion);
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
            INSERT INTO tareas_guia (id, title, categoria)
            VALUES
                ('limpiar-cocina', 'Limpiar cocina', 'Limpieza'),
                ('sacar-basura', 'Sacar la basura', 'Limpieza'),
                ('ducha-post-entreno', 'Ducha post-entreno', 'Higiene'),
                ('entrenar-piernas', 'Entrenar piernas', 'Entrenamiento'),
                ('estiramiento', 'Estiramiento', 'Entrenamiento'),
                ('entrenar-pecho', 'Entrenar pecho', 'Entrenamiento');
            """;
        insertar.ExecuteNonQuery();
    }
}
