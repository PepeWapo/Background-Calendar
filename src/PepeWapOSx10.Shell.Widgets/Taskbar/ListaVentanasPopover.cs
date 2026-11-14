using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// La lista con los nombres de las ventanas de un programa abierto en varias
/// instancias, para elegir a cuál ir.
/// </summary>
internal sealed class ListaVentanasPopover : PopoverBase
{
    public ListaVentanasPopover(ProgramaAbierto programa) : base(programa.Clave, Armar(programa))
    {
    }

    private static StackPanel Armar(ProgramaAbierto programa)
    {
        var raiz = new StackPanel { MinWidth = 220, MaxWidth = 360 };
        raiz.Children.Add(new TextBlock
        {
            Text = $"{programa.Nombre} · {programa.Ventanas.Count}",
            Foreground = Paleta.TextoApagado,
            FontSize = 11,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(8, 4, 8, 6),
        });

        // El popover se cierra solo al perder el foco (lo que ocurre al traer
        // la ventana elegida al frente): el click no tiene que cerrarlo a mano.
        foreach (var ventana in programa.Ventanas)
        {
            var contenido = new StackPanel { Orientation = Orientation.Horizontal };
            contenido.Children.Add(new Image { Source = programa.Icono, Width = 18, Height = 18, Margin = new Thickness(0, 0, 10, 0) });
            contenido.Children.Add(new TextBlock
            {
                Text = ventana.Titulo,
                FontSize = 12.5,
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                MaxWidth = 290,
                Opacity = ventana.Minimizada ? 0.6 : 1,
                Foreground = ventana.EnPrimerPlano ? Paleta.AcentoFlexible : Paleta.TextoPrimario,
            });

            var boton = new Button { Content = contenido, Style = Paleta.Estilo("PopoverItemStyle") };
            var hwnd = ventana.Hwnd;
            boton.Click += (_, _) =>
            {
                CerrarAbierto();
                ProgramasAbiertos.Activar(hwnd);
            };
            raiz.Children.Add(boton);
        }

        return raiz;
    }
}
