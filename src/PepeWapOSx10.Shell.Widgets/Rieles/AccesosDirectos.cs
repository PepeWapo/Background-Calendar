using System.IO;
using System.Runtime.InteropServices;
using Microsoft.VisualBasic.FileIO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// La carpeta propia de la app donde viven los accesos directos de los rieles:
/// migra los del escritorio real de Windows, los lista y les extrae el ícono.
/// </summary>
/// <remarks>
/// La migración es lo que resuelve, de raíz, el problema de que el wallpaper
/// animado tape los íconos reales del escritorio
/// (<see cref="VideoWallpaperWindow"/>): si el acceso directo ya no vive en el
/// escritorio, no hay nada ahí que el video pueda tapar. Se ejecuta en cada
/// arranque — cualquier acceso directo nuevo que aparezca en el escritorio se
/// termina mudando también. Solo accesos directos: archivos sueltos
/// (descargas, etc.) se dejan intactos, no son íconos de programa.
/// </remarks>
internal static class AccesosDirectos
{
    public static readonly string Carpeta = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PepeWapOSx10", "RielIconos");

    /// <summary>.lnk son programas; .url son accesos web (los de Steam, por ejemplo).</summary>
    private static readonly string[] Extensiones = [".lnk", ".url"];

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

    /// <summary>Mueve los accesos directos sueltos del escritorio real a la carpeta de la app.</summary>
    public static void MigrarDesdeElEscritorio()
    {
        Directory.CreateDirectory(Carpeta);

        foreach (var especial in CarpetasDeEscritorio)
        {
            var escritorio = Environment.GetFolderPath(especial);
            if (!Directory.Exists(escritorio))
                continue;

            // La lista se materializa antes de mover nada: enumerar una carpeta
            // de forma perezosa mientras se le sacan archivos deja la
            // enumeración caminando sobre un directorio que cambia debajo, y
            // Windows puede saltearse entradas — accesos directos que se
            // quedarían en el escritorio hasta el próximo arranque.
            foreach (var acceso in EnCarpeta(escritorio).ToList())
            {
                var destino = Path.Combine(Carpeta, Path.GetFileName(acceso));
                if (File.Exists(destino))
                    continue; // ya migrado antes con ese nombre; no pisar lo que ya está.

                try
                {
                    File.Move(acceso, destino);
                }
                catch (IOException)
                {
                    // el archivo puede estar en uso — se reintenta en el próximo arranque.
                }
                catch (UnauthorizedAccessException)
                {
                    // el de la carpeta pública puede pedir permisos que este usuario no tiene.
                }
            }
        }
    }

    /// <summary>Los nombres de archivo que hoy hay en la carpeta de la app.</summary>
    public static HashSet<string> Listar() =>
        Directory.Exists(Carpeta)
            ? EnCarpeta(Carpeta).Select(Path.GetFileName).OfType<string>().ToHashSet()
            : [];

    /// <summary>
    /// Arma el ítem de riel de un acceso directo, o <c>null</c> si no se le
    /// pudo sacar el ícono (apunta a algo desinstalado, típicamente).
    /// </summary>
    public static IconRailItem? Cargar(string nombreArchivo)
    {
        var ruta = Path.Combine(Carpeta, nombreArchivo);
        return ExtraerIcono(ruta) is { } icono
            ? new IconRailItem(Path.GetFileNameWithoutExtension(ruta), ruta, icono)
            : null;
    }

    /// <summary>
    /// Manda el acceso directo a la papelera de reciclaje.
    /// </summary>
    /// <remarks>
    /// Va a la papelera y no a <c>File.Delete</c> porque quitar un ícono del
    /// riel es la única forma que tiene el usuario de deshacer la migración de
    /// un acceso directo, y esa migración ya se lo sacó del escritorio: si acá
    /// se borrara de verdad, un click en el menú perdería el acceso directo
    /// para siempre. Tampoco alcanza con sacarlo de <c>orden.json</c> y dejar
    /// el archivo: la reconciliación del próximo arranque lo volvería a
    /// repartir en un riel.
    /// </remarks>
    public static bool EnviarAPapelera(string ruta)
    {
        try
        {
            FileSystem.DeleteFile(ruta, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or OperationCanceledException or FileNotFoundException)
        {
            return false;
        }
    }

    private static IEnumerable<string> EnCarpeta(string carpeta) =>
        Extensiones.SelectMany(ext => Directory.EnumerateFiles(carpeta, $"*{ext}"));

    private static BitmapSource? ExtraerIcono(string ruta)
    {
        var info = new ShFileInfo();
        SHGetFileInfo(ruta, 0, ref info, (uint)Marshal.SizeOf<ShFileInfo>(), SHGFI_ICON | SHGFI_LARGEICON);
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

    // ===================== interop =====================

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
}
