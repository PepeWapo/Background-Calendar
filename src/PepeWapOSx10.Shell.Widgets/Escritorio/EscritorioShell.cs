using System.Windows;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Las ventanas que el widget pone sobre el escritorio además de sí mismo —
/// el wallpaper animado y los dos rieles de iconos — con su ciclo de vida y
/// su anclaje al fondo del z-order.
/// </summary>
/// <remarks>
/// <see cref="MainWindow"/> antes creaba, ordenaba y cerraba estas ventanas
/// a mano desde sus propios handlers de <c>Loaded</c>/<c>Closed</c>. Acá esa
/// responsabilidad queda separada: la ventana principal abre el escritorio y
/// lo suelta, sin saber qué piezas lo componen ni en qué orden van.
/// </remarks>
internal sealed class EscritorioShell : IDisposable
{
    private readonly VideoWallpaperWindow? _wallpaper;
    private readonly IconRailWindow _rielIzquierdo;
    private readonly IconRailWindow _rielDerecho;
    private readonly AnclajeEscritorio _anclaje;

    /// <summary>
    /// Abre el wallpaper y los rieles, y los ancla junto con el widget en un
    /// único orden fijo. De atrás hacia adelante: wallpaper animado, rieles,
    /// widget — que queda justo debajo de la taskbar y de las ventanas
    /// normales, que no participan de este anclaje y siempre van por encima.
    /// </summary>
    public EscritorioShell(Window widget)
    {
        // El wallpaper animado puede no existir (sin carpeta de videos
        // configurada); los rieles siempre están, aunque queden vacíos.
        _wallpaper = VideoWallpaperWindow.CrearSiHayVideos();
        _wallpaper?.Show();

        var rieles = RielesDeIconos.MigrarYCargar();
        _rielIzquierdo = new IconRailWindow(LadoEscritorio.Izquierda, rieles);
        _rielDerecho = new IconRailWindow(LadoEscritorio.Derecha, rieles);
        _rielIzquierdo.Show();
        _rielDerecho.Show();

        List<Window> deAtrasHaciaAdelante = [_rielIzquierdo, _rielDerecho, widget];
        if (_wallpaper is not null)
            deAtrasHaciaAdelante.Insert(0, _wallpaper);

        _anclaje = new AnclajeEscritorio(
            deAtrasHaciaAdelante,
            clickThrough: _wallpaper is null ? [] : [_wallpaper]);
    }

    public void Dispose()
    {
        _anclaje.Dispose();
        _rielIzquierdo.Close();
        _rielDerecho.Close();
        _wallpaper?.Close();
    }
}
