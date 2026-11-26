using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// El menú de inicio propio: buscador, lista de aplicaciones y energía.
/// Se abre sobre el botón de la taskbar.
/// </summary>
internal sealed class MenuInicioPopover : PopoverBase
{
    private readonly TextBox _buscador = new();
    private readonly StackPanel _lista = new();
    private readonly List<(Button Boton, AppDeInicio App)> _filas = [];
    private readonly TextBlock _estado = new();
    private readonly ScrollViewer _scroll;
    private int _seleccion = -1;

    public MenuInicioPopover() : this(new StackPanel { Width = 340 })
    {
    }

    private MenuInicioPopover(StackPanel raiz) : base("inicio", raiz)
    {
        ConfigurarBuscador();

        _scroll = new ScrollViewer
        {
            Height = 360,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            Content = _lista,
            Margin = new Thickness(0, 8, 0, 0),
        };
        _scroll.Resources.Add(typeof(ScrollBar), Paleta.Estilo("BarraFinaStyle"));

        _estado.Text = "Cargando aplicaciones…";
        _estado.Foreground = Paleta.TextoApagado;
        _estado.FontSize = 12;
        _estado.Margin = new Thickness(8);
        _lista.Children.Add(_estado);

        raiz.Children.Add(Rodear(_buscador));
        raiz.Children.Add(_scroll);
        raiz.Children.Add(new Border { Height = 1, Margin = new Thickness(4, 8, 4, 6), Background = (Brush)Application.Current.Resources["MenuSeparator"] });
        raiz.Children.Add(FilaDeEnergia());

        // La rueda recorre las opciones desde cualquier punto del menú.
        raiz.PreviewMouseWheel += (_, e) =>
        {
            Mover(e.Delta > 0 ? -1 : 1);
            e.Handled = true;
        };

        CargarAplicaciones();
    }

    protected override void AlMostrar() => _buscador.Focus();

    private async void CargarAplicaciones()
    {
        IReadOnlyList<AppDeInicio> apps;
        try
        {
            apps = await AppsDeInicio.Obtener();
        }
        catch (Exception ex)
        {
            // Es async void: sin esto, un fallo del shell tira abajo la app.
            _estado.Text = $"No se pudieron leer las aplicaciones: {ex.Message}";
            return;
        }

        _lista.Children.Clear();
        foreach (var app in apps)
        {
            var boton = FilaDeApp(app);
            _filas.Add((boton, app));
            _lista.Children.Add(boton);
        }

        Filtrar();
    }

    private Button FilaDeApp(AppDeInicio app)
    {
        var contenido = new StackPanel { Orientation = Orientation.Horizontal };
        contenido.Children.Add(new Image { Source = app.Icono, Width = 22, Height = 22, Margin = new Thickness(0, 0, 10, 0) });
        contenido.Children.Add(new TextBlock
        {
            Text = app.Nombre,
            FontSize = 12.5,
            VerticalAlignment = VerticalAlignment.Center,
            TextTrimming = TextTrimming.CharacterEllipsis,
            MaxWidth = 270,
        });

        var boton = new Button { Content = contenido, Style = Paleta.Estilo("PopoverItemStyle"), HorizontalContentAlignment = HorizontalAlignment.Left };
        boton.Click += (_, _) => Abrir(app);
        return boton;
    }

    private void Abrir(AppDeInicio app)
    {
        Cerrar();
        try
        {
            AppsDeInicio.Abrir(app);
        }
        catch (System.ComponentModel.Win32Exception)
        {
            // el acceso directo puede apuntar a algo desinstalado.
        }
    }

    // ===================== buscador =====================

    private void ConfigurarBuscador()
    {
        _buscador.Background = Brushes.Transparent;
        _buscador.BorderThickness = new Thickness(0);
        _buscador.Foreground = Paleta.TextoPrimario;
        _buscador.CaretBrush = Paleta.TextoPrimario;
        _buscador.FontSize = 13;
        _buscador.VerticalContentAlignment = VerticalAlignment.Center;
        _buscador.Padding = new Thickness(2, 7, 2, 7);

        _buscador.TextChanged += (_, _) => Filtrar();
        _buscador.PreviewKeyDown += (_, e) =>
        {
            switch (e.Key)
            {
                case Key.Down:
                    Mover(1);
                    e.Handled = true;
                    break;
                case Key.Up:
                    Mover(-1);
                    e.Handled = true;
                    break;
                case Key.PageDown:
                    Mover(8);
                    e.Handled = true;
                    break;
                case Key.PageUp:
                    Mover(-8);
                    e.Handled = true;
                    break;
                case Key.Enter:
                    var visibles = Visibles();
                    if (visibles.Count > 0)
                        Abrir(visibles[Math.Clamp(_seleccion, 0, visibles.Count - 1)].App);
                    e.Handled = true;
                    break;
            }
        };
    }

    // ===================== navegación =====================

    private List<(Button Boton, AppDeInicio App)> Visibles() =>
        _filas.Where(f => f.Boton.Visibility == Visibility.Visible).ToList();

    /// <summary>Mueve la selección <paramref name="paso"/> filas entre las visibles y la trae a la vista.</summary>
    private void Mover(int paso)
    {
        var visibles = Visibles();
        if (visibles.Count == 0)
            return;

        Seleccionar(Math.Clamp(_seleccion + paso, 0, visibles.Count - 1), visibles);
    }

    private void Seleccionar(int indice, List<(Button Boton, AppDeInicio App)> visibles)
    {
        foreach (var (boton, _) in _filas)
            boton.Background = Brushes.Transparent;

        _seleccion = indice;
        if (indice < 0 || indice >= visibles.Count)
            return;

        var elegido = visibles[indice].Boton;
        elegido.Background = (Brush)Application.Current.Resources["MenuHoverBackground"];
        elegido.BringIntoView();
    }

    /// <summary>El campo de texto con el fondo y el marco de los inputs del proyecto.</summary>
    private UIElement Rodear(TextBox campo)
    {
        var pista = new TextBlock
        {
            Text = "Buscar aplicaciones",
            Foreground = Paleta.TextoTenue,
            FontSize = 13,
            IsHitTestVisible = false,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(5, 0, 0, 0),
        };
        campo.TextChanged += (_, _) => pista.Visibility = campo.Text.Length == 0 ? Visibility.Visible : Visibility.Collapsed;

        var celda = new Grid();
        celda.Children.Add(campo);
        celda.Children.Add(pista);

        return new Border
        {
            Background = (Brush)Application.Current.Resources["InputBackground"],
            BorderBrush = (Brush)Application.Current.Resources["MenuBorder"],
            BorderThickness = new Thickness(1),
            CornerRadius = new CornerRadius(8),
            Padding = new Thickness(8, 0, 8, 0),
            Child = celda,
        };
    }

    private void Filtrar()
    {
        var texto = _buscador.Text.Trim();
        foreach (var (boton, app) in _filas)
        {
            boton.Visibility = texto.Length == 0 || app.Nombre.Contains(texto, StringComparison.CurrentCultureIgnoreCase)
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        // Tras filtrar, la selección vuelve a la primera coincidencia.
        var visibles = Visibles();
        Seleccionar(visibles.Count > 0 ? 0 : -1, visibles);
        _scroll.ScrollToTop();
    }

    // ===================== energía =====================

    private UIElement FilaDeEnergia()
    {
        var fila = new UniformGrid { Rows = 1 };
        fila.Children.Add(BotonDeEnergia("Suspender", AccionesDeSistema.Suspender));
        fila.Children.Add(BotonDeEnergia("Reiniciar", AccionesDeSistema.Reiniciar));
        fila.Children.Add(BotonDeEnergia("Apagar", AccionesDeSistema.Apagar));
        return fila;
    }

    private Button BotonDeEnergia(string texto, Action accion)
    {
        var boton = new Button
        {
            Content = new TextBlock { Text = texto, FontSize = 12, Foreground = Paleta.TextoApagado },
            Style = Paleta.Estilo("PopoverItemStyle"),
            HorizontalContentAlignment = HorizontalAlignment.Center,
        };
        boton.Click += (_, _) =>
        {
            Cerrar();
            accion();
        };
        return boton;
    }
}
