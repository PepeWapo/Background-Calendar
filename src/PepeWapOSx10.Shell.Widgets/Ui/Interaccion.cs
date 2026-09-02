using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Gestos de mouse compartidos por las ventanas ancladas al escritorio.
/// </summary>
internal static class Interaccion
{
    /// <summary>
    /// Marca un elemento como interactivo frente al arrastre de la ventana.
    /// </summary>
    /// <remarks>
    /// La raíz de las ventanas maneja <c>MouseLeftButtonDown</c> para permitir
    /// arrastrarlas desde cualquier zona vacía. <see cref="Window.DragMove"/>
    /// entra en un loop modal de movimiento que captura el mouse y consume el
    /// <c>MouseLeftButtonUp</c> siguiente, por lo que los handlers de click de
    /// las celdas (que viven en el evento de <em>up</em>) nunca se ejecutaban.
    /// Marcar el <em>down</em> como manejado evita que llegue a la raíz.
    ///
    /// Se engancha al evento burbujeante y no al <c>Preview</c>: el de vista
    /// previa tunelea desde la raíz hacia abajo, así que un contenedor le
    /// robaría el mouse-down a los botones que tenga adentro (el ✎ de la guía)
    /// y nunca dispararían su <c>Click</c>. Burbujeando, los controles que ya
    /// manejan el down —botones— lo consumen primero, y este handler solo actúa
    /// cuando el click cayó en el fondo del contenedor.
    /// </remarks>
    public static void HabilitarClick(this FrameworkElement elemento) =>
        elemento.MouseLeftButtonDown += (_, e) => e.Handled = true;

    /// <summary>
    /// Arrastra la ventana desde una zona vacía, ignorando el gesto si el
    /// botón ya se soltó.
    /// </summary>
    /// <remarks>
    /// <see cref="Window.DragMove"/> lanza <see cref="InvalidOperationException"/>
    /// si el botón primario ya no está apretado cuando llega acá — puede pasar
    /// si otro handler demoró el despacho del evento.
    /// </remarks>
    public static void ArrastrarVentana(this Window ventana, MouseButtonEventArgs e)
    {
        if (e.ButtonState == MouseButtonState.Pressed)
            ventana.DragMove();
    }

    /// <summary>
    /// Cablea una celda con el gesto estándar del widget: un click la resalta,
    /// dos clicks navegan.
    /// </summary>
    /// <param name="celda">La celda; se le habilita el click frente al arrastre de la ventana.</param>
    /// <param name="detector">Detector compartido por todas las celdas de la misma ventana.</param>
    /// <param name="clave">Identidad lógica de la celda (la fecha, no el control — ver <see cref="DetectorDobleClick"/>).</param>
    public static void AlSeleccionar(
        this FrameworkElement celda, DetectorDobleClick detector, string clave, Action simple, Action doble)
    {
        celda.HabilitarClick();
        celda.MouseLeftButtonUp += (_, _) =>
        {
            if (detector.EsDoble(clave))
                doble();
            else
                simple();
        };
    }

    /// <summary>
    /// Cablea el <c>Click</c> de un botón a una acción asíncrona y lo devuelve,
    /// para poder construirlo y engancharlo en una sola expresión.
    /// </summary>
    public static Button ConClick(this Button boton, Func<Task> accion)
    {
        boton.Click += async (_, _) => await accion();
        return boton;
    }
}

/// <summary>
/// Detecta el doble click por cuenta propia, sin usar
/// <see cref="MouseButtonEventArgs.ClickCount"/>.
/// </summary>
/// <remarks>
/// El primer click resalta, y resaltar vuelve a construir las celdas de la
/// vista. WPF no le asigna <c>ClickCount = 2</c> a un click que cae sobre un
/// elemento distinto del anterior, así que el segundo click de un doble
/// click siempre llegaba como <c>ClickCount = 1</c> y nunca navegaba. Por eso
/// la identidad de lo clickeado se pasa como una <em>clave lógica</em> (la
/// fecha de la celda, la ruta del ícono) en vez de mirar el control.
/// </remarks>
internal sealed class DetectorDobleClick
{
    [DllImport("user32.dll")]
    private static extern uint GetDoubleClickTime();

    private string? _ultimaClave;
    private DateTime _ultimoInstante = DateTime.MinValue;

    public bool EsDoble(string clave)
    {
        var ahora = DateTime.UtcNow;
        var esDoble = _ultimaClave == clave
                      && (ahora - _ultimoInstante).TotalMilliseconds <= GetDoubleClickTime();

        // Un doble click cierra la secuencia: el tercer click vuelve a contar como simple.
        _ultimaClave = esDoble ? null : clave;
        _ultimoInstante = esDoble ? DateTime.MinValue : ahora;
        return esDoble;
    }
}
