using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Placeholders con brillo pulsante que ocupan el lugar del mini-calendario,
/// la vista Día y la lista de tareas mientras se cargan los datos reales.
/// </summary>
/// <remarks>
/// La ventana se muestra antes de que haya nada que mostrar: abrir la base y
/// bajar el ICS del calendario tardan, y hasta que terminaban los tres paneles
/// se veían completamente vacíos, como si el widget se hubiera colgado. Con el
/// esqueleto la forma de cada panel está desde el primer frame y lo único que
/// cambia después es el contenido.
///
/// Dibuja con las mismas formas y medidas que los renderers de verdad
/// (<see cref="MiniCalendario"/>, <see cref="VistaDia"/>,
/// <see cref="PanelGuia"/>) para que la transición no salte. No hace falta
/// limpiarlo a mano en el camino feliz: los tres renderers vacían su
/// contenedor antes de dibujar.
/// </remarks>
internal static class Esqueleto
{
    /// <summary>Ancho del canvas de la vista Día, igual que en el XAML.</summary>
    private const double AnchoAgenda = 733;

    private static readonly TimeSpan Pulso = TimeSpan.FromSeconds(1.1);

    public static void Mostrar(Grid calendario, Canvas agenda, Panel guia)
    {
        DibujarCalendario(calendario);
        DibujarAgenda(agenda);
        DibujarGuia(guia);

        foreach (Panel contenedor in (Panel[])[calendario, agenda, guia])
            Pulsar(contenedor);
    }

    /// <summary>
    /// Saca el esqueleto de los paneles que no van a recibir datos reales.
    /// </summary>
    /// <remarks>
    /// El camino feliz no lo necesita, pero si la base no abre no se dibuja
    /// nada encima y el esqueleto se quedaría pulsando para siempre, tapando
    /// el mensaje de error.
    /// </remarks>
    public static void Quitar(params Panel[] contenedores)
    {
        foreach (var contenedor in contenedores)
        {
            contenedor.BeginAnimation(UIElement.OpacityProperty, null);
            contenedor.Opacity = 1;
            contenedor.Children.Clear();
        }
    }

    /// <summary>Late entre dos opacidades para que se lea como "cargando" y no como contenido real.</summary>
    private static void Pulsar(UIElement contenedor) =>
        contenedor.BeginAnimation(UIElement.OpacityProperty, new DoubleAnimation(0.45, 0.85, Pulso)
        {
            AutoReverse = true,
            RepeatBehavior = RepeatBehavior.Forever,
        });

    private static void DibujarCalendario(Grid grilla)
    {
        grilla.Children.Clear();
        grilla.RowDefinitions.Clear();
        grilla.ColumnDefinitions.Clear();

        // Una fila de encabezado (las iniciales de los días) más seis de números.
        for (var i = 0; i < 7; i++)
            grilla.RowDefinitions.Add(new RowDefinition());
        for (var i = 0; i < 7; i++)
            grilla.ColumnDefinitions.Add(new ColumnDefinition());

        for (var fila = 0; fila < 7; fila++)
        {
            for (var columna = 0; columna < 7; columna++)
            {
                var celda = Bloque(fila == 0 ? 12 : 22, fila == 0 ? 8 : 22, radio: fila == 0 ? 4 : 11);
                celda.HorizontalAlignment = HorizontalAlignment.Center;
                celda.VerticalAlignment = VerticalAlignment.Center;
                Grid.SetRow(celda, fila);
                Grid.SetColumn(celda, columna);
                grilla.Children.Add(celda);
            }
        }
    }

    /// <summary>
    /// Bloques de agenda de altos distintos separados por huecos, que es como
    /// se ve un día real con eventos y ratos libres.
    /// </summary>
    private static void DibujarAgenda(Canvas canvas)
    {
        canvas.Children.Clear();

        double[] altos = [64, 96, 48, 140, 72, 56, 108, 64];
        double y = 4;

        foreach (var alto in altos)
        {
            var bloque = Bloque(AnchoAgenda - 60, alto, radio: 10);
            Canvas.SetLeft(bloque, 56);
            Canvas.SetTop(bloque, y);
            canvas.Children.Add(bloque);

            // La regla de horas de la izquierda.
            var hora = Bloque(26, 9, radio: 4);
            Canvas.SetLeft(hora, 10);
            Canvas.SetTop(hora, y + 4);
            canvas.Children.Add(hora);

            y += alto + 10;
        }

        canvas.Height = y;
    }

    /// <summary>Filas de tarea: título largo arriba y una línea corta de detalle abajo.</summary>
    private static void DibujarGuia(Panel lista)
    {
        lista.Children.Clear();

        for (var i = 0; i < 7; i++)
        {
            var fila = new StackPanel { Margin = new Thickness(0, 0, 0, 18) };
            fila.Children.Add(Bloque(i % 3 == 0 ? 118 : 158, 12, radio: 5));
            fila.Children.Add(Bloque(92, 8, radio: 4, margenSuperior: 7));
            lista.Children.Add(fila);
        }
    }

    private static Border Bloque(double ancho, double alto, double radio, double margenSuperior = 0) => new()
    {
        Width = ancho,
        Height = alto,
        CornerRadius = new CornerRadius(radio),
        Background = Paleta.FondoBloque,
        BorderBrush = Paleta.BordeBloqueLibre,
        BorderThickness = new Thickness(1),
        HorizontalAlignment = HorizontalAlignment.Left,
        Margin = new Thickness(0, margenSuperior, 0, 0),
    };
}
