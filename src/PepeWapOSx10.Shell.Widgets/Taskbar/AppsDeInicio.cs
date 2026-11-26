using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace PepeWapOSx10.Shell.Widgets;

/// <param name="Id">
/// El identificador dentro de <c>shell:AppsFolder</c>: según la app es un
/// AppUserModelID, una ruta o una dirección. Sirve para el ícono y para abrirla.
/// </param>
internal sealed record AppDeInicio(string Nombre, string Id, ImageSource? Icono);

/// <summary>
/// Las aplicaciones instaladas, tal como las lista el Menú Inicio de Windows:
/// la carpeta virtual <c>shell:AppsFolder</c>.
/// </summary>
/// <remarks>
/// Leer los <c>.lnk</c> de <c>Start Menu\Programs</c> dejaba afuera los accesos
/// de la raíz del menú (Docker Desktop, Ubuntu) y las apps empaquetadas, y
/// mostraba en inglés las que Windows traduce. Esta carpeta resuelve las tres.
/// </remarks>
internal static partial class AppsDeInicio
{
    private const string Carpeta = "shell:AppsFolder";

    /// <summary>Más que los 22 que ocupa en la fila, para que no se vea borroso con escalado.</summary>
    private const int LadoIcono = 48;

    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(2);

    /// <summary>Enlaces a documentación y soporte que los instaladores agregan junto al programa.</summary>
    private static readonly string[] EsquemasDeEnlace = ["http://", "https://", "file://"];

    private static Task<IReadOnlyList<AppDeInicio>>? _tarea;
    private static DateTime _cargadaEn;

    /// <summary>
    /// La lista, leyéndola en segundo plano la primera vez (sacar más de cien
    /// íconos tarda) y reusándola un rato después.
    /// </summary>
    public static Task<IReadOnlyList<AppDeInicio>> Obtener()
    {
        var vencida = DateTime.UtcNow - _cargadaEn > Vigencia;
        if (_tarea is null || _tarea.IsFaulted || vencida && _tarea.IsCompleted)
        {
            _cargadaEn = DateTime.UtcNow;
            _tarea = EnHiloSta(Cargar);
        }

        return _tarea;
    }

    /// <exception cref="System.ComponentModel.Win32Exception">Si Windows no pudo abrirla.</exception>
    public static void Abrir(AppDeInicio app) =>
        Process.Start(new ProcessStartInfo($@"{Carpeta}\{app.Id}") { UseShellExecute = true });

    private static IReadOnlyList<AppDeInicio> Cargar()
    {
        var tipo = Type.GetTypeFromProgID("Shell.Application", throwOnError: true)!;
        dynamic shell = Activator.CreateInstance(tipo)!;
        var apps = new List<AppDeInicio>();

        foreach (var item in shell.NameSpace(Carpeta).Items())
        {
            // Windows devuelve algún nombre con espacios adelante.
            string nombre = ((string)item.Name).Trim();
            string id = item.Path;

            if (EsInstalador(nombre) || EsEnlace(id))
                continue;

            apps.Add(new AppDeInicio(nombre, id, ExtraerIcono(id)));
        }

        return [.. apps.OrderBy(a => a.Nombre, StringComparer.CurrentCultureIgnoreCase)];
    }

    /// <summary>Instaladores y desinstaladores: no son programas para usar.</summary>
    private static bool EsInstalador(string nombre) => PatronDeInstalador().IsMatch(nombre);

    // "unistal" cubre un acceso que viene mal escrito de fábrica (Pentablet).
    [GeneratedRegex(@"uninstal|unistal|desinstal|\binstal", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex PatronDeInstalador();

    private static bool EsEnlace(string id) =>
        EsquemasDeEnlace.Any(e => id.StartsWith(e, StringComparison.OrdinalIgnoreCase));

    /// <summary>El shell es COM de un solo hilo, y los hilos del pool no lo son.</summary>
    private static Task<T> EnHiloSta<T>(Func<T> trabajo)
    {
        var resultado = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
        var hilo = new Thread(() =>
        {
            try
            {
                resultado.SetResult(trabajo());
            }
            catch (Exception ex)
            {
                resultado.SetException(ex);
            }
        })
        { IsBackground = true };

        hilo.SetApartmentState(ApartmentState.STA);
        hilo.Start();
        return resultado.Task;
    }

    // ===================== íconos =====================

    /// <returns>El ícono ya congelado (se usa desde el hilo de UI), o <c>null</c> si no se pudo.</returns>
    private static BitmapSource? ExtraerIcono(string id)
    {
        var iid = typeof(IShellItemImageFactory).GUID;
        if (SHCreateItemFromParsingName($@"{Carpeta}\{id}", IntPtr.Zero, ref iid, out var fabrica) != 0)
            return null;

        var mapa = IntPtr.Zero;
        try
        {
            var tamano = new Tamano { Ancho = LadoIcono, Alto = LadoIcono };
            return fabrica.GetImage(tamano, SIIGBF_ICONONLY, out mapa) == 0 ? Convertir(mapa) : null;
        }
        finally
        {
            if (mapa != IntPtr.Zero)
                DeleteObject(mapa);
            Marshal.ReleaseComObject(fabrica);
        }
    }

    /// <summary>
    /// Copia los píxeles a mano: <c>CreateBitmapSourceFromHBitmap</c> no
    /// respeta de forma confiable la transparencia que traen estos íconos.
    /// </summary>
    private static BitmapSource? Convertir(IntPtr mapa)
    {
        if (GetObject(mapa, Marshal.SizeOf<MapaDeBits>(), out var datos) == 0)
            return null;

        var paso = datos.Ancho * 4;
        var pixeles = new byte[paso * datos.Alto];
        var formato = new CabeceraDeMapa
        {
            Tamano = (uint)Marshal.SizeOf<CabeceraDeMapa>(),
            Ancho = datos.Ancho,
            Alto = -datos.Alto, // negativo: filas de arriba hacia abajo.
            Planos = 1,
            BitsPorPixel = 32,
        };

        var contexto = GetDC(IntPtr.Zero);
        try
        {
            if (GetDIBits(contexto, mapa, 0, (uint)datos.Alto, pixeles, ref formato, 0) == 0)
                return null;
        }
        finally
        {
            ReleaseDC(IntPtr.Zero, contexto);
        }

        // Un ícono viejo sin canal alfa viene con todo en cero: se vería invisible.
        var conAlfa = false;
        for (var i = 3; i < pixeles.Length && !conAlfa; i += 4)
            conAlfa = pixeles[i] != 0;

        var imagen = BitmapSource.Create(
            datos.Ancho, datos.Alto, 96, 96,
            conAlfa ? PixelFormats.Pbgra32 : PixelFormats.Bgr32,
            null, pixeles, paso);
        imagen.Freeze();
        return imagen;
    }

    // ===================== interop =====================

    private const int SIIGBF_ICONONLY = 0x4;

    [ComImport]
    [Guid("bcc18b79-ba16-442f-80c4-8a59c30c463b")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    private interface IShellItemImageFactory
    {
        [PreserveSig]
        int GetImage(Tamano tamano, int opciones, out IntPtr mapa);
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Tamano
    {
        public int Ancho;
        public int Alto;
    }

    /// <summary>La estructura <c>BITMAP</c> de GDI.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MapaDeBits
    {
        public int Tipo;
        public int Ancho;
        public int Alto;
        public int BytesPorFila;
        public ushort Planos;
        public ushort BitsPorPixel;
        public IntPtr Bits;
    }

    /// <summary>La estructura <c>BITMAPINFOHEADER</c> de GDI.</summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct CabeceraDeMapa
    {
        public uint Tamano;
        public int Ancho;
        public int Alto;
        public ushort Planos;
        public ushort BitsPorPixel;
        public uint Compresion;
        public uint BytesDeImagen;
        public int ResolucionX;
        public int ResolucionY;
        public uint ColoresUsados;
        public uint ColoresImportantes;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Unicode)]
    private static extern int SHCreateItemFromParsingName(string ruta, IntPtr contexto, ref Guid iid, out IShellItemImageFactory item);

    [DllImport("gdi32.dll")]
    private static extern int GetObject(IntPtr objeto, int tamano, out MapaDeBits datos);

    [DllImport("gdi32.dll")]
    private static extern int GetDIBits(IntPtr contexto, IntPtr mapa, uint primeraFila, uint filas, byte[] pixeles, ref CabeceraDeMapa formato, uint uso);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr objeto);

    [DllImport("user32.dll")]
    private static extern IntPtr GetDC(IntPtr ventana);

    [DllImport("user32.dll")]
    private static extern int ReleaseDC(IntPtr ventana, IntPtr contexto);
}
