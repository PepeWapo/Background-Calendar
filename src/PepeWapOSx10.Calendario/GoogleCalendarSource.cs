using System.Text.Json;
using Ical.Net;
using Ical.Net.DataTypes;
using Ical.Net.Evaluation;
using PepeWapOSx10.Dominio.Interfaces;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Calendario;

public sealed class GoogleCalendarSource : ICalendarSource
{
    private static readonly HttpClient Http = new();

    private readonly string _icsUrl;

    public GoogleCalendarSource(string? icsUrl = null)
    {
        _icsUrl = icsUrl ?? CargarIcsUrlDesdeConfig();
    }

    public Task<IReadOnlyList<FixedEvent>> ObtenerFijosDelDiaAsync(DateOnly fecha) =>
        ObtenerFijosDelRangoAsync(fecha, fecha);

    public async Task<IReadOnlyList<FixedEvent>> ObtenerFijosDelRangoAsync(DateOnly desde, DateOnly hasta)
    {
        // ConfigureAwait(false) para que el parseo de acá abajo no vuelva al
        // hilo que haya llamado: bajar el ICS es espera, pero recorrer todos
        // los eventos y expandir sus repeticiones es CPU, y hecho sobre el hilo
        // de UI congelaba el widget entero en cada refresco.
        var contenido = await Http.GetStringAsync(_icsUrl).ConfigureAwait(false);
        var calendario = Calendar.Load(contenido) ?? throw new InvalidOperationException("No se pudo parsear el ICS.");

        var inicioRango = desde.ToDateTime(TimeOnly.MinValue);
        var finRango = hasta.ToDateTime(TimeOnly.MinValue).AddDays(1);

        // Se arranca la búsqueda un par de días antes para no perder eventos
        // multi-día (ej. Tattoo terminando de madrugada) que empezaron antes
        // de la fecha pedida pero siguen vigentes ese día.
        var inicioBusqueda = new CalDateTime(inicioRango.AddDays(-2));
        var options = new EvaluationOptions();

        var eventos = new List<FixedEvent>();

        foreach (var componente in calendario.Events)
        {
            foreach (var ocurrencia in componente.GetOccurrences(inicioBusqueda, options))
            {
                if (ocurrencia.Period.StartTime is not { } inicioCal)
                    continue;

                var inicio = inicioCal.Value;
                if (inicio >= finRango)
                    break;

                if (ocurrencia.Period.EffectiveEndTime is not { } finCal)
                    continue;

                var fin = finCal.Value;
                if (fin <= inicioRango)
                    continue;

                eventos.Add(new FixedEvent(componente.Summary ?? "(sin título)", inicio, fin));
            }
        }

        return eventos.OrderBy(e => e.Start).ToList();
    }

    private static string CargarIcsUrlDesdeConfig()
    {
        var rutaConfig = Path.Combine(AppContext.BaseDirectory, "config.json");
        var json = File.ReadAllText(rutaConfig);
        using var documento = JsonDocument.Parse(json);
        return documento.RootElement.GetProperty("ics_url").GetString()
            ?? throw new InvalidOperationException("config.json no tiene la clave 'ics_url'.");
    }
}
