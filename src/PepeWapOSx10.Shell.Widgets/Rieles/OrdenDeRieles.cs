using System.IO;
using System.Text.Json;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Persiste en <c>orden.json</c> qué ícono va en qué riel y en qué posición,
/// para que el acomodo sobreviva a un reinicio de la app.
/// </summary>
internal static class OrdenDeRieles
{
    private static readonly string Ruta = Path.Combine(AccesosDirectos.Carpeta, "orden.json");

    private static readonly JsonSerializerOptions Formato = new() { WriteIndented = true };

    public sealed record Reparto(List<string> Izquierda, List<string> Derecha);

    /// <summary>
    /// Cruza lo que dice <c>orden.json</c> con lo que realmente hay en la
    /// carpeta: descarta entradas de archivos que ya no existen, y reparte los
    /// accesos nuevos (recién migrados, o primera vez que corre la app)
    /// alternando entre lados.
    /// </summary>
    public static Reparto ReconciliarCon(IReadOnlySet<string> enDisco)
    {
        var guardado = Leer();
        var izquierda = guardado.Izquierda.Where(enDisco.Contains).ToList();
        var derecha = guardado.Derecha.Where(enDisco.Contains).ToList();

        var yaUbicados = izquierda.Concat(derecha).ToHashSet();
        foreach (var nombre in enDisco.Where(n => !yaUbicados.Contains(n)).OrderBy(n => n))
        {
            if (izquierda.Count <= derecha.Count)
                izquierda.Add(nombre);
            else
                derecha.Add(nombre);
        }

        return new Reparto(izquierda, derecha);
    }

    public static void Guardar(IEnumerable<string> izquierda, IEnumerable<string> derecha)
    {
        try
        {
            Directory.CreateDirectory(AccesosDirectos.Carpeta);
            File.WriteAllText(Ruta, JsonSerializer.Serialize(
                new { izquierda = izquierda.ToList(), derecha = derecha.ToList() }, Formato));
        }
        catch (IOException)
        {
            // no persistir el orden no es grave — la próxima carga vuelve a
            // reconciliar contra el disco.
        }
    }

    private static Reparto Leer()
    {
        try
        {
            if (!File.Exists(Ruta))
                return new Reparto([], []);

            using var documento = JsonDocument.Parse(File.ReadAllText(Ruta));
            return new Reparto(Lado(documento, "izquierda"), Lado(documento, "derecha"));
        }
        catch (Exception ex) when (ex is IOException or JsonException or KeyNotFoundException)
        {
            return new Reparto([], []);
        }
    }

    private static List<string> Lado(JsonDocument documento, string clave) =>
        documento.RootElement.TryGetProperty(clave, out var valores)
            ? valores.EnumerateArray().Select(e => e.GetString()).OfType<string>().ToList()
            : [];
}
