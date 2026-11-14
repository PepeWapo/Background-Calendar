using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Los íconos del área de notificación de Windows dentro de un contenedor
/// propio. Click y click derecho se reenvían al ícono real
/// (<see cref="BandejaDelSistema"/>).
/// </summary>
internal sealed class BandejaPopover : PopoverBase
{
    private const int Columnas = 5;

    public BandejaPopover() : base("bandeja", Armar())
    {
    }

    private static UIElement Armar()
    {
        var iconos = BandejaDelSistema.Leer();
        if (iconos.Count == 0)
        {
            return new TextBlock
            {
                Text = "No se pudo leer la bandeja",
                Foreground = Paleta.TextoApagado,
                FontSize = 12,
                Margin = new Thickness(8),
            };
        }

        var grilla = new UniformGrid { Columns = Math.Min(Columnas, iconos.Count) };
        foreach (var icono in iconos)
        {
            var boton = new Button
            {
                Content = new Image { Source = icono.Icono, Width = 20, Height = 20 },
                Style = Paleta.Estilo("PopoverItemStyle"),
                HorizontalContentAlignment = HorizontalAlignment.Center,
                Padding = new Thickness(9),
                ToolTip = icono.Texto,
            };

            // El popover se cierra después del click: el menú o ventana que la
            // app abra en respuesta le roba el foco igual, pero si la app no
            // abre nada tiene que cerrarse a mano.
            boton.Click += (_, _) => Reenviar(icono, derecho: false);
            boton.MouseRightButtonUp += (_, e) =>
            {
                e.Handled = true;
                Reenviar(icono, derecho: true);
            };
            grilla.Children.Add(boton);
        }

        return grilla;
    }

    private static void Reenviar(IconoDeBandeja icono, bool derecho)
    {
        // El click físico cae sobre la barra real y le saca el foco al popover,
        // que se cierra solo; se cierra antes igual para no depender de eso.
        CerrarAbierto();
        _ = BandejaDelSistema.Click(icono, derecho);
    }
}
