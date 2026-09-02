using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Dibuja los elementos de una línea de tiempo vertical (eje de horas,
/// grilla, bloques de agenda, indicador de "ahora") sobre un <see cref="Canvas"/>.
/// </summary>
/// <remarks>
/// Las vistas Día y Semana pintan exactamente los mismos elementos con las
/// mismas medidas y solo cambian dónde: Día apila bandas de 24 horas a lo
/// largo del scroll, Semana pone siete columnas de 24 horas lado a lado.
/// Antes las primitivas vivían en el archivo parcial de la vista Día y la
/// vista Semana las usaba desde ahí — funcionaba por ser la misma clase
/// parcial, pero ataba el dibujo genérico al estado de una vista concreta.
/// Acá no hay estado: son funciones de (canvas, geometría) a formas.
/// </remarks>
internal static class LineaDeTiempo
{
    public const double PixelsPorMinuto = 0.72;

    /// <summary>Ancho reservado a la izquierda para el eje de horas.</summary>
    public const double EjeX = 46;

    /// <summary>Dónde arrancan los bloques en la vista Día, ya pasado el eje de horas.</summary>
    public const double ContenidoX = 58;

    /// <summary>Alto de la banda de un día: las 24 horas completas.</summary>
    public const double AlturaDia = 24 * 60 * PixelsPorMinuto;

    /// <summary>Alto mínimo de un bloque para que siga siendo visible.</summary>
    public const double AltoMinimoBloque = 4;

    /// <summary>Debajo de este alto el bloque se dibuja en una sola línea.</summary>
    private const double AltoCompacto = 40;

    public static double MinutosAPixeles(double minutos) => minutos * PixelsPorMinuto;

    /// <summary>Eje de horas de una banda de 24 h con su línea divisoria por hora.</summary>
    /// <param name="lineaEnLaPrimera">
    /// La vista Día ya dibuja su propio separador de día en la hora 0 y no
    /// quiere una línea de grilla encima; Semana sí la necesita.
    /// </param>
    public static void DibujarHoras(Canvas canvas, double offsetY, double contenidoX, double anchoContenido, bool lineaEnLaPrimera)
    {
        for (var hora = 0; hora < 24; hora++)
        {
            var top = offsetY + MinutosAPixeles(hora * 60);
            EtiquetaDeHora(canvas, hora, top);

            if (lineaEnLaPrimera || hora > 0)
                LineaHorizontal(canvas, contenidoX, anchoContenido, top, 1, Paleta.LineaDeGrilla);
        }
    }

    public static void EtiquetaDeHora(Canvas canvas, int hora, double top) =>
        Posicionar(canvas, new TextBlock
        {
            Text = hora.ToString("00"),
            FontSize = 10.5,
            Foreground = Paleta.TextoTenue,
            Width = 40,
            TextAlignment = TextAlignment.Right,
        }, 0, top - 6);

    public static void LineaHorizontal(Canvas canvas, double left, double ancho, double top, double grosor, Brush color) =>
        Posicionar(canvas, new Rectangle { Width = ancho, Height = grosor, Fill = color }, left, top);

    public static void LineaVertical(Canvas canvas, double left, double top, double alto, Brush color) =>
        Posicionar(canvas, new Rectangle { Width = 1, Height = alto, Fill = color }, left, top);

    public static void Rectangulo(Canvas canvas, double left, double top, double ancho, double alto, Brush relleno) =>
        Posicionar(canvas, new Rectangle { Width = ancho, Height = alto, Fill = relleno }, left, top);

    /// <summary>Pastilla flotante con fondo propio, para que se lea sobre cualquier bloque.</summary>
    public static Border Pastilla(string texto, Brush color, double tamanio, Thickness padding) => new()
    {
        Background = Paleta.EtiquetaFlotante,
        CornerRadius = new CornerRadius(6),
        Padding = padding,
        Child = new TextBlock
        {
            Text = texto,
            FontSize = tamanio,
            FontWeight = FontWeights.Bold,
            Foreground = color,
        },
    };

    /// <summary>Línea + punto + hora marcando el instante actual dentro de la banda de hoy.</summary>
    public static void DibujarAhora(Canvas canvas, double left, double top, double ancho, DateTime ahora)
    {
        LineaHorizontal(canvas, left, ancho, top, 1.5, Paleta.AcentoFlexible);
        Posicionar(canvas, new Ellipse { Width = 11, Height = 11, Fill = Paleta.AcentoFlexible }, left - 5.5, top - 5.5);
        Posicionar(canvas, Pastilla($"AHORA · {ahora:HH:mm}", Paleta.AcentoFlexible, 10.5, new Thickness(7, 2, 7, 2)), left + 20, top - 10);
    }

    /// <summary>Un bloque de agenda: fondo, punto de color según tipo, título y horario.</summary>
    public static void DibujarBloque(
        Canvas canvas, ScheduledBlock bloque, double left, double top, double alto, double ancho, bool forzarCompacto = false)
    {
        var esLibre = bloque.Kind == BlockKind.Libre;
        var compacto = forzarCompacto || alto < AltoCompacto;

        var contenedor = new Grid { Width = ancho, Height = alto };
        contenedor.Children.Add(FondoDeBloque(esLibre, compacto, ancho, alto));
        contenedor.Children.Add(compacto
            ? ContenidoCompacto(bloque, esLibre)
            : ContenidoExtendido(bloque, esLibre, alto));

        Posicionar(canvas, contenedor, left, top);
    }

    public static Brush ColorDe(BlockKind tipo) => tipo switch
    {
        BlockKind.Fixed => Paleta.AcentoFijo,
        BlockKind.Flexible => Paleta.AcentoFlexible,
        _ => Paleta.AcentoLibre,
    };

    public static Ellipse Punto(BlockKind tipo, double diametro, double margenDerecho) => new()
    {
        Width = diametro,
        Height = diametro,
        Fill = ColorDe(tipo),
        VerticalAlignment = VerticalAlignment.Center,
        Margin = new Thickness(0, 0, margenDerecho, 0),
    };

    public static void Posicionar(Canvas canvas, UIElement elemento, double left, double top)
    {
        Canvas.SetLeft(elemento, left);
        Canvas.SetTop(elemento, top);
        canvas.Children.Add(elemento);
    }

    private static Rectangle FondoDeBloque(bool esLibre, bool compacto, double ancho, double alto)
    {
        var radio = compacto ? 10 : 14;
        var fondo = new Rectangle
        {
            Width = ancho,
            Height = alto,
            RadiusX = radio,
            RadiusY = radio,
            Fill = esLibre ? Brushes.Transparent : Paleta.FondoBloque,
            Stroke = esLibre ? Paleta.BordeBloqueLibre : Paleta.BordePanel,
            StrokeThickness = 1,
        };

        if (esLibre)
            fondo.StrokeDashArray = [3, 3];

        return fondo;
    }

    private static Grid ContenidoCompacto(ScheduledBlock bloque, bool esLibre)
    {
        var contenido = new Grid { Margin = new Thickness(10, 0, 10, 0) };
        contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        contenido.Children.Add(EnColumna(Punto(bloque.Kind, 6, 8), 0));
        contenido.Children.Add(EnColumna(new TextBlock
        {
            Text = bloque.Title,
            FontSize = 11.5,
            FontWeight = esLibre ? FontWeights.Normal : FontWeights.SemiBold,
            FontStyle = esLibre ? FontStyles.Italic : FontStyles.Normal,
            Foreground = esLibre ? Paleta.TextoTenue : Paleta.TextoPrimario,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
        }, 1));
        contenido.Children.Add(EnColumna(new TextBlock
        {
            Text = $"{bloque.Start:HH:mm}–{bloque.End:HH:mm}",
            FontSize = 10.5,
            Foreground = Paleta.TextoApagado,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Right,
        }, 3));

        return contenido;
    }

    private static Grid ContenidoExtendido(ScheduledBlock bloque, bool esLibre, double alto)
    {
        var filaTitulo = new StackPanel { Orientation = Orientation.Horizontal };
        filaTitulo.Children.Add(Punto(bloque.Kind, 7, 10));
        filaTitulo.Children.Add(new TextBlock
        {
            Text = bloque.Title,
            FontSize = alto > 150 ? 15 : 13.5,
            FontWeight = FontWeights.SemiBold,
            Foreground = Paleta.TextoPrimario,
        });

        var pila = new StackPanel { VerticalAlignment = VerticalAlignment.Center };
        pila.Children.Add(filaTitulo);
        pila.Children.Add(new TextBlock
        {
            Text = esLibre
                ? $"Libre · {Duracion(bloque)} sin asignar"
                : $"{bloque.Start:HH:mm} – {bloque.End:HH:mm}",
            FontSize = 11.5,
            FontStyle = esLibre ? FontStyles.Italic : FontStyles.Normal,
            Foreground = esLibre ? Paleta.TextoTenue : Paleta.TextoApagado,
            Margin = new Thickness(esLibre ? 0 : 17, 4, 0, 0),
        });

        var contenido = new Grid { Margin = new Thickness(16, 0, 16, 0) };
        contenido.Children.Add(pila);
        return contenido;
    }

    private static string Duracion(ScheduledBlock bloque)
    {
        var duracion = bloque.End - bloque.Start;
        return duracion.Hours > 0 ? $"{duracion.Hours}h {duracion.Minutes}m" : $"{duracion.Minutes}m";
    }

    private static T EnColumna<T>(T elemento, int columna) where T : UIElement
    {
        Grid.SetColumn(elemento, columna);
        return elemento;
    }
}
