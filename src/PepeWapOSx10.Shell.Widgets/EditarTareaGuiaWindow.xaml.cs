using System.Windows;
using System.Windows.Input;
using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>Alta y edición de una tarea de la guía.</summary>
public partial class EditarTareaGuiaWindow : Window
{
    private static readonly (Repeticion Valor, string Texto)[] Repeticiones =
    [
        (Repeticion.Unica, "Única"),
        (Repeticion.Semanal, "Semanal"),
        (Repeticion.Mensual, "Mensual"),
    ];

    private readonly TareaGuia? _original;

    /// <summary>La tarea resultante, o <c>null</c> si se canceló.</summary>
    public TareaGuia? Resultado { get; private set; }

    /// <param name="tarea">La tarea a editar, o <c>null</c> para crear una nueva.</param>
    /// <param name="categoriasConocidas">Categorías ya en uso, para sugerir.</param>
    public EditarTareaGuiaWindow(TareaGuia? tarea, IEnumerable<string> categoriasConocidas)
    {
        InitializeComponent();

        _original = tarea;
        TituloDialogo.Text = tarea is null ? "Nueva tarea" : "Editar tarea";

        foreach (var categoria in categoriasConocidas.Distinct().OrderBy(c => c))
            CategoriaBox.Items.Add(categoria);

        RepeticionBox.ItemsSource = Repeticiones.Select(r => r.Texto).ToList();

        TituloBox.Text = tarea?.Title ?? string.Empty;
        CategoriaBox.Text = tarea?.Categoria ?? string.Empty;
        RepeticionBox.SelectedIndex = Array.FindIndex(
            Repeticiones,
            r => r.Valor == (tarea?.Repeticion ?? Repeticion.Semanal));

        Loaded += (_, _) => TituloBox.Focus();
    }

    private void Raiz_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            DragMove();
    }

    private void Guardar_Click(object sender, RoutedEventArgs e)
    {
        var titulo = TituloBox.Text.Trim();
        var categoria = CategoriaBox.Text.Trim();

        if (titulo.Length == 0)
        {
            MostrarError("Poné un título para la tarea.");
            return;
        }

        if (categoria.Length == 0)
        {
            MostrarError("Poné una categoría (por ejemplo: Limpieza).");
            return;
        }

        var repeticion = Repeticiones[Math.Max(RepeticionBox.SelectedIndex, 0)].Valor;

        Resultado = _original is null
            ? new TareaGuia(Guid.NewGuid().ToString("n"), titulo, categoria, Repeticion: repeticion)
            : _original with { Title = titulo, Categoria = categoria, Repeticion = repeticion };

        DialogResult = true;
    }

    private void MostrarError(string mensaje)
    {
        ErrorText.Text = mensaje;
        ErrorText.Visibility = Visibility.Visible;
    }

    private void Cancelar_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
