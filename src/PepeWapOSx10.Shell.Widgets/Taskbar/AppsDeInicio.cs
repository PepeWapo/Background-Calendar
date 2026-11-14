using System.IO;
using System.Windows.Media;

namespace PepeWapOSx10.Shell.Widgets;

internal sealed record AppDeInicio(string Nombre, string Ruta, ImageSource? Icono);

/// <summary>
/// Las aplicaciones del Menú Inicio de Windows: los accesos directos de las
/// dos carpetas que Windows combina (la del usuario y la de todos los usuarios).
/// </summary>
/// <remarks>
/// No incluye las apps de la Tienda (UWP): no tienen acceso directo en esas
/// carpetas, viven en <c>shell:AppsFolder</c>.
/// </remarks>
internal static class AppsDeInicio
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

    private static readonly string[] Carpetas =
    [
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu), "Programs"),
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu), "Programs"),
    ];

    private static Task<IReadOnlyList<AppDeInicio>>? _tarea;
    private static DateTime _cargadaEn;

    /// <summary>
    /// La lista, leyéndola en segundo plano la primera vez (sacar el ícono de
    /// cientos de accesos directos tarda) y reusándola un rato después.
    /// </summary>
    public static Task<IReadOnlyList<AppDeInicio>> Obtener()
    {
        if (_tarea is null || DateTime.UtcNow - _cargadaEn > Vigencia && _tarea.IsCompleted)
        {
            _cargadaEn = DateTime.UtcNow;
            _tarea = Task.Run(Cargar);
        }

        return _tarea;
    }

    private static IReadOnlyList<AppDeInicio> Cargar()
    {
        var porNombre = new Dictionary<string, AppDeInicio>(StringComparer.OrdinalIgnoreCase);

        foreach (var carpeta in Carpetas.Where(Directory.Exists))
        {
            foreach (var ruta in Directory.EnumerateFiles(carpeta, "*.lnk", SearchOption.AllDirectories))
            {
                var nombre = Path.GetFileNameWithoutExtension(ruta);
                if (porNombre.ContainsKey(nombre) || EsDesinstalador(nombre))
                    continue;

                porNombre[nombre] = new AppDeInicio(nombre, ruta, AccesosDirectos.ExtraerIcono(ruta));
            }
        }

        return [.. porNombre.Values.OrderBy(a => a.Nombre, StringComparer.CurrentCultureIgnoreCase)];
    }

    private static bool EsDesinstalador(string nombre) =>
        nombre.Contains("uninstall", StringComparison.OrdinalIgnoreCase)
        || nombre.Contains("desinstal", StringComparison.OrdinalIgnoreCase);
}
