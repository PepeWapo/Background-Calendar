using System.IO;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using LibVLCSharp.Shared;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Wallpaper animado: cubre las pantallas con loops de video tomados de una
/// carpeta (<see cref="WallpaperConfig"/>) — pensada para loops "seamless"
/// (el último frame empalma con el primero), así que cada video se repite en
/// el lugar hasta que toca cambiar.
/// </summary>
/// <remarks>
/// Usa LibVLCSharp (motor de VLC) en vez del <c>MediaElement</c> nativo de
/// WPF: trae sus propios decoders embebidos, así que no depende de qué
/// códecs tenga instalados Windows — reproduce HEVC/H.265 y prácticamente
/// cualquier otro formato sin pedirle nada al usuario.
///
/// El anclaje al fondo del z-order lo orquesta <see cref="EscritorioShell"/>
/// junto con el resto de las ventanas de escritorio, para que esta quede
/// siempre detrás de ellas, y es también quien la mantiene click-through
/// —a ella y a las ventanas internas de LibVLC— para no robarle el menú
/// contextual al escritorio real ni los clicks al resto del widget
/// (<see cref="AnclajeEscritorio.HacerClickThroughConSusVentanas"/>).
/// </remarks>
public partial class VideoWallpaperWindow : Window
{
    private static readonly string[] ExtensionesSoportadas = [".mp4", ".mkv", ".webm", ".avi", ".mov"];
    private static readonly TimeSpan IntervaloDeRotacion = TimeSpan.FromMinutes(10);

    /// <summary>Cuánto tarda el video en apagarse (y después en encenderse) al rotar.</summary>
    private static readonly TimeSpan MedioFundido = TimeSpan.FromSeconds(1.2);

    private readonly List<string> _videos;
    private readonly Random _orden = new();
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _reproductor;
    private string _recorte;

    private int _indiceActual = -1;
    private bool _rotando;
    private DispatcherTimer? _rotacionTimer;

    /// <summary>
    /// Arma la ventana si la carpeta configurada tiene al menos un video;
    /// <c>null</c> en caso contrario (nada que mostrar, feature deshabilitada).
    /// </summary>
    public static VideoWallpaperWindow? CrearSiHayVideos()
    {
        if (WallpaperConfig.CarpetaConfigurada() is not { } carpeta)
            return null;

        var videos = Directory.EnumerateFiles(carpeta)
            .Where(ruta => ExtensionesSoportadas.Contains(Path.GetExtension(ruta), StringComparer.OrdinalIgnoreCase))
            .ToList();

        return videos.Count == 0 ? null : new VideoWallpaperWindow(videos);
    }

    private VideoWallpaperWindow(List<string> videos)
    {
        InitializeComponent();
        _videos = videos;

        _recorte = string.Empty;
        Colocar();

        // Conectar la tableta digitalizadora agranda el escritorio virtual, y
        // con él la franja que el wallpaper tiene que tapar.
        EspacioEscritorio.SeguirALaPantallaPrincipal(this, Colocar);

        _libVlc = new LibVLC();
        _reproductor = new MediaPlayer(_libVlc) { Mute = true };
        Reproductor.MediaPlayer = _reproductor;

        _reproductor.EndReached += (_, _) => Dispatcher.BeginInvoke(AlTerminar);
        _reproductor.EncounteredError += (_, _) => Dispatcher.BeginInvoke(AlFallarReproduccion);

        SourceInitialized += (_, _) =>
        {
            AnclajeEscritorio.OcultarDeAltTab(this);
            AnclajeEscritorio.ImpedirMinimizado(this);
            AnclajeEscritorio.RechazarActivacionPorClick(this);
        };
        Loaded += (_, _) =>
        {
            ElegirSiguienteIndice();
            ReproducirActual();

            _rotacionTimer = new DispatcherTimer { Interval = IntervaloDeRotacion };
            _rotacionTimer.Tick += (_, _) => _ = RotarAsync();
            _rotacionTimer.Start();
        };
    }

    private void ElegirSiguienteIndice()
    {
        // Barajado simple: próximo índice al azar entre los que no sean el
        // actual, para no repetir el mismo loop dos veces seguidas cuando hay
        // más de un video.
        _indiceActual = _videos.Count <= 1
            ? 0
            : Enumerable.Range(0, _videos.Count).Where(i => i != _indiceActual).OrderBy(_ => _orden.Next()).First();
    }

    /// <summary>
    /// El wallpaper tapa el escritorio virtual entero, no una pantalla.
    /// </summary>
    /// <remarks>
    /// El recorte le dice a VLC que corte cada video a la proporción de la
    /// pantalla antes de dibujarlo: como el <c>VideoView</c> siempre estira el
    /// resultado para llenar todo su espacio, recortar primero a la misma
    /// proporción es lo que logra "centrado, sin franjas, sin deformar" en vez
    /// de pillarbox/letterbox o una imagen estirada. Se aplica al empezar cada
    /// video, así que un cambio de pantallas se acomoda en la próxima rotación.
    /// </remarks>
    private void Colocar()
    {
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        _recorte = $"{(int)Width}:{(int)Height}";
    }

    private void ReproducirActual()
    {
        if (_videos.Count == 0)
            return;

        using var media = new Media(_libVlc, new Uri(_videos[_indiceActual]));
        _reproductor.Play(media);
        _reproductor.CropGeometry = _recorte;
    }

    /// <summary>
    /// Cada <see cref="IntervaloDeRotacion"/>: apaga el video, cambia al
    /// siguiente y lo vuelve a encender.
    /// </summary>
    /// <remarks>
    /// El fundido lo hace un velo negro dibujado <em>dentro</em> del
    /// <c>VideoView</c>, que es lo único que se compone por encima del video.
    ///
    /// Los dos intentos anteriores no producían ningún efecto visible, los dos
    /// por la misma razón (VLC dibuja sobre una ventana nativa propia, ajena
    /// al árbol visual de WPF): (1) dos reproductores superpuestos, uno
    /// desvaneciéndose con <c>Opacity</c> y un <c>BlurEffect</c> encima del
    /// otro — se verificó poniendo <c>Opacity = 0</c> en los dos y el video
    /// seguía viéndose a pleno brillo, así que lo único que lograba era
    /// decodificar dos videos a la vez durante seis segundos y cortar de golpe
    /// en un momento arbitrario; y (2) bajarle el brillo con el filtro
    /// <c>adjust</c> del propio VLC (<c>SetAdjustFloat</c>), que tampoco movió
    /// el brillo medido en pantalla al activarlo con la reproducción ya en
    /// curso.
    /// </remarks>
    private async Task RotarAsync()
    {
        if (_videos.Count <= 1 || _rotando)
            return; // nada para rotar si hay un solo video, o ya hay una rotación en curso.

        _rotando = true;
        try
        {
            await FundirAsync(hasta: 1.0);
            ElegirSiguienteIndice();
            ReproducirActual();
            await FundirAsync(hasta: 0.0);
        }
        finally
        {
            _rotando = false;
        }
    }

    /// <summary>Lleva el velo a la opacidad pedida y espera a que la animación termine.</summary>
    private Task FundirAsync(double hasta)
    {
        var terminado = new TaskCompletionSource();
        var animacion = new DoubleAnimation(hasta, MedioFundido);
        animacion.Completed += (_, _) => terminado.TrySetResult();
        Velo.BeginAnimation(OpacityProperty, animacion);
        return terminado.Task;
    }

    /// <summary>
    /// El video llegó a su propio final (loop "seamless": el último frame
    /// empalma con el primero) sin que le tocara todavía rotar — se repite
    /// en el lugar hasta que el timer de <see cref="IntervaloDeRotacion"/>
    /// dispare la próxima rotación.
    /// </summary>
    private void AlTerminar()
    {
        if (!_rotando)
            ReproducirActual();
    }

    /// <summary>
    /// Un video que VLC no puede decodificar dispara <c>EncounteredError</c>
    /// en vez de reproducirse. Se lo descarta de la playlist — no tiene
    /// sentido reintentarlo.
    /// </summary>
    private void AlFallarReproduccion()
    {
        if (_indiceActual >= 0 && _indiceActual < _videos.Count)
            _videos.RemoveAt(_indiceActual);
        _indiceActual--;

        if (_videos.Count == 0)
            return;

        ElegirSiguienteIndice();
        ReproducirActual();
    }

    /// <summary>
    /// Suelta el motor de VLC al cerrarse.
    /// </summary>
    /// <remarks>
    /// Va en <see cref="Window.OnClosed"/> y no en un <c>Close()</c> propio:
    /// un método <c>new Close()</c> solo corría si quien cerraba la ventana la
    /// tenía tipada como <see cref="VideoWallpaperWindow"/>. Cuando la cierra
    /// WPF al apagar la app —que es el camino normal— la ve como
    /// <see cref="Window"/> y llamaba al <c>Close()</c> de la clase base, así
    /// que el <c>MediaPlayer</c> y el <c>LibVLC</c> quedaban sin liberar y el
    /// proceso podía no terminar de bajar.
    /// </remarks>
    protected override void OnClosed(EventArgs e)
    {
        _rotacionTimer?.Stop();
        _reproductor.Stop();
        _reproductor.Dispose();
        _libVlc.Dispose();
        base.OnClosed(e);
    }
}
