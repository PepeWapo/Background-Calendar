using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;
using LibVLCSharp.Shared;
using LibVLCSharp.WPF;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Wallpaper animado: cubre las pantallas con loops de video tomados de una
/// carpeta (<see cref="WallpaperConfig"/>) — pensada para loops "seamless"
/// (el último frame empalma con el primero), así que cada video se repite en
/// el lugar hasta que toca cambiar. El anclaje al fondo del z-order lo
/// orquesta <see cref="MainWindow"/> junto con el resto de las ventanas de
/// escritorio, para que este quede siempre detrás de ellas
/// (<see cref="AnclajeEscritorio"/>). Click-through total
/// (<see cref="AnclajeEscritorio.HacerClickThrough"/>) para no robarle el
/// menú contextual ni ningún click al escritorio real que queda detrás.
///
/// Usa LibVLCSharp (motor de VLC) en vez del <c>MediaElement</c> nativo de
/// WPF: trae sus propios decoders embebidos, así que no depende de qué
/// códecs tenga instalados Windows — reproduce HEVC/H.265 y prácticamente
/// cualquier otro formato sin pedirle nada al usuario.
///
/// Cada <see cref="IntervaloDeRotacion"/> cruza (crossfade) al próximo video
/// de la carpeta en vez de cortar en seco: el que entra ya arranca a
/// reproducirse por debajo mientras el actual se desenfoca y desvanece
/// encima suyo (<see cref="DuracionTransicion"/>) — por eso hacen falta dos
/// <c>VideoView</c>/<c>MediaPlayer</c> en paralelo
/// (<see cref="ReproductorA"/>/<see cref="ReproductorB"/>) en vez de uno solo.
/// </summary>
public partial class VideoWallpaperWindow : Window
{
    private static readonly string[] ExtensionesSoportadas = [".mp4", ".mkv", ".webm", ".avi", ".mov"];
    private static readonly TimeSpan IntervaloDeRotacion = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan DuracionTransicion = TimeSpan.FromSeconds(6);
    private const double RadioDeDesenfoqueMaximo = 60;

    private readonly List<string> _videos;
    private readonly Random _orden = new();
    private readonly LibVLC _libVlc;
    private readonly MediaPlayer _mediaPlayerA;
    private readonly MediaPlayer _mediaPlayerB;
    private readonly string _cropGeometry;
    private int _indiceActual = -1;
    private bool _aEstaArriba = true;
    private bool _transicionEnCurso;
    private DispatcherTimer? _rotacionTimer;

    private VideoView ReproductorArriba => _aEstaArriba ? ReproductorA : ReproductorB;
    private VideoView ReproductorAbajo => _aEstaArriba ? ReproductorB : ReproductorA;
    private BlurEffect EfectoArriba => _aEstaArriba ? DesenfoqueA : DesenfoqueB;
    private MediaPlayer JugadorArriba => _aEstaArriba ? _mediaPlayerA : _mediaPlayerB;
    private MediaPlayer JugadorAbajo => _aEstaArriba ? _mediaPlayerB : _mediaPlayerA;

    /// <summary>
    /// Arma la ventana si la carpeta configurada tiene al menos un video;
    /// <c>null</c> en caso contrario (nada que mostrar, feature deshabilitada).
    /// </summary>
    public static VideoWallpaperWindow? CrearSiHayVideos()
    {
        var carpeta = WallpaperConfig.CarpetaConfigurada();
        if (carpeta is null)
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

        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        // Le dice a VLC que recorte cada video a la proporción de la pantalla
        // antes de dibujarlo: como el VideoView siempre estira el resultado
        // para llenar todo su espacio, recortar primero a la misma proporción
        // es lo que logra "centrado, sin franjas, sin deformar" en vez de
        // pillarbox/letterbox o una imagen estirada.
        _cropGeometry = $"{(int)Width}:{(int)Height}";

        _libVlc = new LibVLC();
        _mediaPlayerA = new MediaPlayer(_libVlc) { Mute = true };
        _mediaPlayerB = new MediaPlayer(_libVlc) { Mute = true };
        ReproductorA.MediaPlayer = _mediaPlayerA;
        ReproductorB.MediaPlayer = _mediaPlayerB;

        foreach (var jugador in new[] { _mediaPlayerA, _mediaPlayerB })
        {
            jugador.EndReached += (_, _) => Dispatcher.BeginInvoke(() => AlTerminar(jugador));
            jugador.EncounteredError += (_, _) => Dispatcher.BeginInvoke(() => AlFallarReproduccion(jugador));
        }

        SourceInitialized += (_, _) =>
        {
            AnclajeEscritorio.OcultarDeAltTab(this);
            AnclajeEscritorio.HacerClickThrough(this);
        };
        Loaded += (_, _) =>
        {
            ElegirSiguienteIndice();
            ReproducirActual();

            _rotacionTimer = new DispatcherTimer { Interval = IntervaloDeRotacion };
            _rotacionTimer.Tick += (_, _) => IniciarCrossfade();
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

    private void ReproducirActual()
    {
        if (_videos.Count == 0)
            return;

        using var media = new Media(_libVlc, new Uri(_videos[_indiceActual]));
        JugadorArriba.Play(media);
        JugadorArriba.CropGeometry = _cropGeometry;
    }

    /// <summary>
    /// Dispara cada <see cref="IntervaloDeRotacion"/>: arranca el próximo
    /// video en el reproductor de abajo (ya visible a través del de arriba a
    /// medida que este se desvanece) y desenfoca + desvanece el de arriba a
    /// lo largo de <see cref="DuracionTransicion"/>.
    /// </summary>
    private void IniciarCrossfade()
    {
        if (_videos.Count <= 1 || _transicionEnCurso)
            return; // nada para cruzar si hay un solo video, o ya hay un cruce en curso.

        _transicionEnCurso = true;
        ElegirSiguienteIndice();

        using var media = new Media(_libVlc, new Uri(_videos[_indiceActual]));
        JugadorAbajo.Play(media);
        JugadorAbajo.CropGeometry = _cropGeometry;

        var desenfoque = new DoubleAnimation(0, RadioDeDesenfoqueMaximo, DuracionTransicion);
        var desvanecido = new DoubleAnimation(1.0, 0.0, DuracionTransicion);
        desvanecido.Completed += (_, _) => TerminarCrossfade();

        EfectoArriba.BeginAnimation(BlurEffect.RadiusProperty, desenfoque);
        ReproductorArriba.BeginAnimation(OpacityProperty, desvanecido);

        // El que entra arranca apenas visible y sube a opacidad completa en
        // los primeros 3 segundos — así no aparece de golpe al 100% debajo
        // del que se está yendo.
        var aparece = new DoubleAnimation(0.15, 1.0, TimeSpan.FromSeconds(3));
        ReproductorAbajo.BeginAnimation(OpacityProperty, aparece);
    }

    /// <summary>Cierra el ciclo: apaga el que se acaba de desvanecer y da vuelta los roles para el próximo cruce.</summary>
    private void TerminarCrossfade()
    {
        var arribaSaliente = ReproductorArriba;
        var efectoSaliente = EfectoArriba;
        var jugadorSaliente = JugadorArriba;

        jugadorSaliente.Stop();
        arribaSaliente.Opacity = 1.0;
        efectoSaliente.Radius = 0;

        _aEstaArriba = !_aEstaArriba;
        Panel.SetZIndex(ReproductorArriba, 1);
        Panel.SetZIndex(ReproductorAbajo, 0);

        _transicionEnCurso = false;
    }

    /// <summary>
    /// El video llegó a su propio final (loop "seamless": el último frame
    /// empalma con el primero) sin que le tocara todavía rotar — se repite
    /// en el lugar hasta que el timer de <see cref="IntervaloDeRotacion"/>
    /// dispare el próximo cruce.
    /// </summary>
    private void AlTerminar(MediaPlayer jugador)
    {
        if (jugador != JugadorArriba || _transicionEnCurso)
            return;

        ReproducirActual();
    }

    /// <summary>
    /// Un video que VLC no puede decodificar dispara <c>EncounteredError</c>
    /// en vez de reproducirse. Se lo descarta de la playlist — no tiene
    /// sentido reintentarlo.
    /// </summary>
    private void AlFallarReproduccion(MediaPlayer jugador)
    {
        if (_indiceActual >= 0 && _indiceActual < _videos.Count)
            _videos.RemoveAt(_indiceActual);
        _indiceActual--;

        if (jugador != JugadorArriba)
        {
            _transicionEnCurso = false; // el que se estaba preparando para el cruce falló; se reintenta en el próximo ciclo natural.
            return;
        }

        _transicionEnCurso = false;
        ElegirSiguienteIndice();
        ReproducirActual();
    }

    public new void Close()
    {
        _rotacionTimer?.Stop();
        _mediaPlayerA.Stop();
        _mediaPlayerB.Stop();
        _mediaPlayerA.Dispose();
        _mediaPlayerB.Dispose();
        _libVlc.Dispose();
        base.Close();
    }
}
