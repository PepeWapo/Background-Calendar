using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using PepeWapOSx10.Dominio.Interfaces;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// El panel de Tareas: la guía de tareas recurrentes agrupadas por categoría,
/// con alta, edición, borrado y marcado.
/// </summary>
/// <remarks>
/// Antes vivía dentro de <see cref="MainWindow"/>, que además maneja el
/// arrastre de la ventana, tres vistas de agenda y el mini-calendario. Sacarlo
/// a su propia clase deja a la ventana como mero armador de piezas y hace que
/// todo lo de la guía —consultas, render y diálogos— esté en un solo lugar.
///
/// La guía siempre refleja "hoy" real, independientemente de qué fecha esté
/// mirando la vista Día/Semana/Mes: por eso no recibe ninguna fecha.
/// </remarks>
internal sealed class PanelGuia
{
    private readonly Panel _lista;
    private readonly TextBlock _resumen;
    private readonly Window _duenio;
    private readonly ITareaGuiaRepository _repositorio;

    private IReadOnlyList<TareaGuia> _tareas = [];

    public PanelGuia(Panel lista, TextBlock resumen, Window duenio, ITareaGuiaRepository repositorio)
    {
        _lista = lista;
        _resumen = resumen;
        _duenio = duenio;
        _repositorio = repositorio;
    }

    public async Task RefrescarAsync()
    {
        try
        {
            _tareas = await _repositorio.ObtenerTodasAsync();
            Render();
        }
        catch (Exception ex)
        {
            // El panel de guía no es crítico para la agenda, pero fallar en
            // silencio escondía problemas de esquema/migración.
            _resumen.Text = $"No se pudo cargar la guía: {ex.Message}";
        }
    }

    public async Task AgregarAsync()
    {
        if (PedirTarea(tarea: null) is not { } nueva)
            return;

        await _repositorio.CrearAsync(nueva);
        await RefrescarAsync();
    }

    private void Render()
    {
        _lista.Children.Clear();

        var pendientes = _tareas.Count(t => !t.Hecha);
        _resumen.Text = _tareas.Count == 0
            ? "sin tareas · usá + para agregar"
            : $"{pendientes} de {_tareas.Count} pendientes";

        foreach (var grupo in _tareas.GroupBy(t => t.Categoria))
        {
            _lista.Children.Add(new TextBlock
            {
                Text = grupo.Key.ToUpperInvariant(),
                FontSize = 10,
                FontWeight = FontWeights.Bold,
                Foreground = Paleta.TextoApagado,
                Margin = new Thickness(0, 14, 0, 10),
            });

            foreach (var tarea in grupo)
                _lista.Children.Add(Fila(tarea));
        }
    }

    /// <remarks>
    /// La fila entera es un solo blanco de click: el <c>Border</c> exterior
    /// tiene fondo (transparente, pero hit-testeable) y cubre también el
    /// casillero y el espacio entre columnas, así que marcar la tarea funciona
    /// igual clickeando el casillero, el título o el hueco de al lado. Los dos
    /// botones de la derecha consumen su propio click y no llegan acá.
    /// </remarks>
    private Border Fila(TareaGuia tarea)
    {
        var contenido = new Grid();
        foreach (var ancho in new[] { GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto })
            contenido.ColumnDefinitions.Add(new ColumnDefinition { Width = ancho });

        Agregar(contenido, Casilla(tarea), columna: 0);
        Agregar(contenido, Textos(tarea), columna: 1);
        Agregar(contenido, BotonDeFila("✎", 12, "Editar tarea", () => EditarAsync(tarea)), columna: 2);
        Agregar(contenido, BotonDeFila("✕", 11, "Eliminar tarea", () => EliminarAsync(tarea), izquierda: 2), columna: 3);

        var fila = new Border
        {
            Background = Brushes.Transparent,
            Cursor = Cursors.Hand,
            Padding = new Thickness(0, 2, 0, 2),
            Margin = new Thickness(0, 0, 0, 8),
            Child = contenido,
        };

        fila.HabilitarClick();
        fila.MouseLeftButtonUp += async (_, _) => await AlternarAsync(tarea);
        return fila;
    }

    private static Border Casilla(TareaGuia tarea)
    {
        var casilla = new Border
        {
            Width = 18,
            Height = 18,
            CornerRadius = new CornerRadius(5),
            BorderBrush = tarea.Hecha ? Paleta.AcentoFlexible : Paleta.TextoTenue,
            BorderThickness = new Thickness(1.6),
            Background = tarea.Hecha ? Paleta.AcentoFlexible : Brushes.Transparent,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(0, 1, 11, 0),
        };

        if (tarea.Hecha)
        {
            casilla.Child = new Path
            {
                Data = Geometry.Parse("M4 9.5 L7.5 13 L14 5.5"),
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round,
            };
        }

        return casilla;
    }

    private static StackPanel Textos(TareaGuia tarea)
    {
        var textos = new StackPanel();
        textos.Children.Add(new TextBlock
        {
            Text = tarea.Title,
            FontSize = 13,
            Foreground = tarea.Hecha ? Paleta.TextoApagado : Paleta.TextoPrimario,
            TextDecorations = tarea.Hecha ? TextDecorations.Strikethrough : null,
            TextWrapping = TextWrapping.Wrap,
        });
        textos.Children.Add(new TextBlock
        {
            Text = DescribirEstado(tarea),
            FontSize = 10.5,
            Foreground = Paleta.TextoTenue,
            Margin = new Thickness(0, 2, 0, 0),
        });
        return textos;
    }

    private static Button BotonDeFila(string glifo, double tamanio, string tooltip, Func<Task> alClickear, double izquierda = 0) =>
        new Button
        {
            Content = glifo,
            Style = Paleta.Estilo("NavArrowButtonStyle"),
            Width = 22,
            Height = 22,
            FontSize = tamanio,
            VerticalAlignment = VerticalAlignment.Top,
            Margin = new Thickness(izquierda, 0, 0, 0),
            ToolTip = tooltip,
        }.ConClick(alClickear);

    private static void Agregar(Grid grilla, FrameworkElement elemento, int columna)
    {
        Grid.SetColumn(elemento, columna);
        grilla.Children.Add(elemento);
    }

    private async Task AlternarAsync(TareaGuia tarea)
    {
        if (tarea.Hecha)
            await _repositorio.DesmarcarAsync(tarea.Id);
        else
            await _repositorio.MarcarHechaAsync(tarea.Id, DateOnly.FromDateTime(DateTime.Today));

        await RefrescarAsync();
    }

    private async Task EditarAsync(TareaGuia tarea)
    {
        if (PedirTarea(tarea) is not { } editada)
            return;

        await _repositorio.ActualizarAsync(editada);
        await RefrescarAsync();
    }

    private async Task EliminarAsync(TareaGuia tarea)
    {
        var confirmar = MessageBox.Show(
            _duenio,
            $"¿Eliminar \"{tarea.Title}\" de la guía? Esto borra también su historial.",
            "Eliminar tarea",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning,
            MessageBoxResult.No);

        if (confirmar != MessageBoxResult.Yes)
            return;

        await _repositorio.EliminarAsync(tarea.Id);
        await RefrescarAsync();
    }

    /// <summary>Abre el diálogo de alta/edición y devuelve la tarea, o <c>null</c> si se canceló.</summary>
    private TareaGuia? PedirTarea(TareaGuia? tarea)
    {
        var dialogo = new EditarTareaGuiaWindow(tarea, _tareas.Select(t => t.Categoria)) { Owner = _duenio };
        return dialogo.ShowDialog() == true ? dialogo.Resultado : null;
    }

    private static string DescribirEstado(TareaGuia tarea)
    {
        var cadencia = tarea.Repeticion switch
        {
            Repeticion.Semanal => "semanal",
            Repeticion.Mensual => "mensual",
            _ => "una vez",
        };

        if (!tarea.Hecha)
            return $"{cadencia} · {DescribirUltimaVez(tarea.UltimaVez)}";

        var vigencia = tarea.Repeticion switch
        {
            Repeticion.Semanal => "hecha esta semana",
            Repeticion.Mensual => "hecha este mes",
            _ => "hecha",
        };

        return $"{cadencia} · {vigencia}";
    }

    private static string DescribirUltimaVez(DateOnly? ultimaVez)
    {
        if (ultimaVez is null)
            return "sin registro";

        var dias = DateOnly.FromDateTime(DateTime.Today).DayNumber - ultimaVez.Value.DayNumber;
        return dias switch
        {
            0 => "hoy",
            1 => "ayer",
            _ => $"última vez hace {dias} días",
        };
    }
}
