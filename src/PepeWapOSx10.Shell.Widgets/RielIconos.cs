using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Migra los accesos directos (.lnk y .url — programas y accesos web tipo
/// Steam) del escritorio real de Windows a una carpeta propia de la app,
/// arma los <see cref="IconRailItem"/> de cada riel con su ícono real, y
/// persiste el orden/lado de cada uno para que sobreviva a un reinicio de la
/// app (<see cref="GuardarOrden"/>, llamado después de cada drag-and-drop
/// entre rieles).
/// </summary>
/// <remarks>
/// La migración es lo que resuelve, de raíz, el problema de que el wallpaper
/// animado tape los íconos reales del escritorio (<see cref="VideoWallpaperWindow"/>):
/// si el acceso directo ya no vive en el escritorio, no hay nada ahí que el
/// video pueda tapar. Se ejecuta en cada arranque — cualquier acceso directo
/// nuevo que aparezca en el escritorio se termina mudando también. Solo
/// accesos directos: archivos sueltos (descargas, etc.) se dejan intactos,
/// no son íconos de programa.
/// </remarks>
internal static class RielIconos
{
    private static readonly string CarpetaDestino = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PepeWapOSx10", "RielIconos");

    private static readonly string RutaOrden = Path.Combine(CarpetaDestino, "orden.json");

    private static readonly string[] ExtensionesDeAccesoDirecto = [".lnk", ".url"];

    private static IEnumerable<string> AccesosDirectosEn(string carpeta) =>
        ExtensionesDeAccesoDirecto.SelectMany(ext => Directory.EnumerateFiles(carpeta, $"*{ext}"));

    public sealed record Rieles(List<IconRailItem> Izquierda, List<IconRailItem> Derecha);

    /// <summary>Mueve los .lnk sueltos del escritorio real a la carpeta del riel y arma ambas listas, en el orden persistido.</summary>
    public static Rieles MigrarYCargar()
    {
        Directory.CreateDirectory(CarpetaDestino);
        MigrarDesdeElEscritorio();

        var (nombresIzquierda, nombresDerecha) = ReconciliarConDisco();
        GuardarOrden(nombresIzquierda, nombresDerecha);

        return new Rieles(
            nombresIzquierda.Select(CargarItem).OfType<IconRailItem>().ToList(),
            nombresDerecha.Select(CargarItem).OfType<IconRailItem>().ToList());
    }

    /// <summary>Persiste el orden/lado actual de los íconos — se llama después de cada drag-and-drop.</summary>
    public static void GuardarOrden(IEnumerable<IconRailItem> izquierda, IEnumerable<IconRailItem> derecha) =>
        GuardarOrden(izquierda.Select(i => Path.GetFileName(i.Target)).ToList(), derecha.Select(i => Path.GetFileName(i.Target)).ToList());

    private static void GuardarOrden(List<string> izquierda, List<string> derecha)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { izquierda, derecha }, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(RutaOrden, json);
        }
        catch
        {
            // no persistir el orden no es grave — la próxima carga vuelve a reconciliar contra el disco.
        }
    }

    /// <summary>
    /// Cruza lo que dice <c>orden.json</c> con lo que realmente hay en la carpeta:
    /// descarta entradas de archivos que ya no existen, y reparte los .lnk nuevos
    /// (recién migrados, o primera vez que corre la app) alternando entre lados.
    /// </summary>
    private static (List<string> Izquierda, List<string> Derecha) ReconciliarConDisco()
    {
        var enDisco = AccesosDirectosEn(CarpetaDestino).Select(Path.GetFileName).OfType<string>().ToHashSet();

        var (izquierda, derecha) = LeerOrdenGuardado();
        izquierda = izquierda.Where(enDisco.Contains).ToList();
        derecha = derecha.Where(enDisco.Contains).ToList();

        var yaUbicados = izquierda.Concat(derecha).ToHashSet();
        var nuevos = enDisco.Where(n => !yaUbicados.Contains(n)).OrderBy(n => n);

        foreach (var nombre in nuevos)
        {
            if (izquierda.Count <= derecha.Count)
                izquierda.Add(nombre);
            else
                derecha.Add(nombre);
        }

        return (izquierda, derecha);
    }

    private static (List<string> Izquierda, List<string> Derecha) LeerOrdenGuardado()
    {
        try
        {
            if (!File.Exists(RutaOrden))
                return ([], []);

            using var documento = JsonDocument.Parse(File.ReadAllText(RutaOrden));
            var izquierda = documento.RootElement.GetProperty("izquierda").EnumerateArray().Select(e => e.GetString()!).ToList();
            var derecha = documento.RootElement.GetProperty("derecha").EnumerateArray().Select(e => e.GetString()!).ToList();
            return (izquierda, derecha);
        }
        catch
        {
            return ([], []);
        }
    }

    /// <summary>
    /// El escritorio de Windows en pantalla combina dos carpetas reales: la
    /// del usuario y la "pública" (accesos directos que algún instalador puso
    /// para todos los usuarios). Hay que migrar de las dos, si no quedan
    /// accesos directos visibles en el escritorio real que igual el video tapa.
    /// </summary>
    private static readonly Environment.SpecialFolder[] CarpetasDeEscritorio =
    [
        Environment.SpecialFolder.DesktopDirectory,
        Environment.SpecialFolder.CommonDesktopDirectory,
    ];

    private static void MigrarDesdeElEscritorio()
    {
        foreach (var carpeta in CarpetasDeEscritorio)
        {
            var escritorio = Environment.GetFolderPath(carpeta);
            if (!Directory.Exists(escritorio))
                continue;

            foreach (var acceso in AccesosDirectosEn(escritorio))
            {
                var destino = Path.Combine(CarpetaDestino, Path.GetFileName(acceso));
                if (File.Exists(destino))
                    continue; // ya migrado antes con ese nombre; no pisar lo que ya está.

                try
                {
                    File.Move(acceso, destino);
                }
                catch
                {
                    // el archivo puede estar en uso, o el de la carpeta pública
                    // puede requerir permisos que este usuario no tiene — se
                    // reintenta solo en el próximo arranque.
                }
            }
        }
    }

    private static IconRailItem? CargarItem(string nombreArchivo)
    {
        var ruta = Path.Combine(CarpetaDestino, nombreArchivo);
        var icono = ExtraerIcono(ruta);
        return icono is null ? null : new IconRailItem(Path.GetFileNameWithoutExtension(ruta), ruta, icono);
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref ShFileInfo psfi, uint cbFileInfo, uint uFlags);

    [DllImport("user32.dll")]
    private static extern bool DestroyIcon(IntPtr hIcon);

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    private struct ShFileInfo
    {
        public IntPtr hIcon;
        public int iIcon;
        public uint dwAttributes;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string szDisplayName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
        public string szTypeName;
    }

    private const uint SHGFI_ICON = 0x100;
    private const uint SHGFI_LARGEICON = 0x0;

    private static BitmapSource? ExtraerIcono(string rutaLnk)
    {
        var info = new ShFileInfo();
        SHGetFileInfo(rutaLnk, 0, ref info, (uint)Marshal.SizeOf<ShFileInfo>(), SHGFI_ICON | SHGFI_LARGEICON);
        if (info.hIcon == IntPtr.Zero)
            return null;

        try
        {
            var bitmap = Imaging.CreateBitmapSourceFromHIcon(info.hIcon, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions());
            bitmap.Freeze();
            return bitmap;
        }
        finally
        {
            DestroyIcon(info.hIcon);
        }
    }
}
