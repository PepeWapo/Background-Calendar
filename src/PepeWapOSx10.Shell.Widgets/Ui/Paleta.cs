using System.Windows;
using System.Windows.Media;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Acceso tipado a los pinceles de <c>Tema.xaml</c>.
/// </summary>
/// <remarks>
/// El código que arma elementos a mano necesitaba un
/// <c>(Brush)FindResource("TextMuted")</c> por cada color — medio centenar de
/// castos repetidos, cada uno con la clave escrita como string suelto y sin
/// red de seguridad ante un typo. Acá la clave se escribe una sola vez y el
/// resto del código pide <c>Paleta.TextoApagado</c>.
///
/// Se resuelve contra <see cref="Application.Resources"/> y no contra el
/// <c>FindResource</c> de cada elemento: el tema es global, así que no hace
/// falta un elemento de referencia para leerlo — eso es justamente lo que
/// permite que clases sin ningún <c>FrameworkElement</c> a mano (los
/// renderers) usen la paleta.
/// </remarks>
internal static class Paleta
{
    public static Brush FondoPanel => Obtener("PanelBackground");
    public static Brush BordePanel => Obtener("PanelBorder");
    public static Brush TextoPrimario => Obtener("TextPrimary");
    public static Brush TextoApagado => Obtener("TextMuted");
    public static Brush TextoTenue => Obtener("TextFaint");
    public static Brush AcentoFijo => Obtener("AccentFijo");
    public static Brush AcentoFlexible => Obtener("AccentFlexible");
    public static Brush AcentoLibre => Obtener("AccentLibre");
    public static Brush FondoBloque => Obtener("BlockBackground");
    public static Brush LineaDeGrilla => Obtener("GridLine");
    public static Brush TinteSeleccion => Obtener("SelectedTint");
    public static Brush EtiquetaFlotante => Obtener("EtiquetaFlotante");

    /// <summary>Borde punteado de los bloques libres: el mismo blanco de los textos, casi transparente.</summary>
    public static Brush BordeBloqueLibre { get; } = Congelar(new SolidColorBrush(Color.FromArgb(0x1A, 0xED, 0xEF, 0xF2)));

    public static Style Estilo(string clave) => (Style)Application.Current.Resources[clave];

    private static Brush Obtener(string clave) => (Brush)Application.Current.Resources[clave];

    private static Brush Congelar(Brush pincel)
    {
        pincel.Freeze();
        return pincel;
    }
}
