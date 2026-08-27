using System.Text.Json;
using PepeWapOSx10.Calendario;
using PepeWapOSx10.Datos;
using PepeWapOSx10.Dominio;
using PepeWapOSx10.Dominio.Modelos;

var contexto = new AgendaDbContext();
contexto.Inicializar();

var repositorioTareas = new SqliteFlexibleTaskRepository(contexto);
var fuenteCalendario = new GoogleCalendarSource();

var fecha = DateOnly.FromDateTime(DateTime.Today);

var fijos = await fuenteCalendario.ObtenerFijosDelDiaAsync(fecha);
var tareasLibres = await repositorioTareas.ObtenerTodasAsync();
var yaAgendadas = await repositorioTareas.ObtenerYaAgendadasAsync();

var agenda = Scheduler.ArmarAgenda(fijos, tareasLibres, fecha, yaAgendadas);

foreach (var bloque in agenda.Where(b => b.Kind == BlockKind.Flexible))
    await repositorioTareas.MarcarAgendadaAsync(bloque.TaskId!, fecha);

var opciones = new JsonSerializerOptions { WriteIndented = true };
Console.WriteLine($"Agenda del {fecha:yyyy-MM-dd}:");
Console.WriteLine(JsonSerializer.Serialize(agenda, opciones));
