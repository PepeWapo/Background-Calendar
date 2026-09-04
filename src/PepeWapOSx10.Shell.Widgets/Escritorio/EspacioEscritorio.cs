using System.Windows;
using Microsoft.Win32;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Márgenes verticales compartidos por todas las ventanas ancladas al
/// escritorio (<see cref="MainWindow"/> y los <see cref="IconRailWindow"/>),
/// para que arranquen alineadas arriba y dejen un espacio libre abajo
/// reservado para la futura taskbar del widget (Fase 4 del roadmap) — sin
/// esto, con el alto completo del área de trabajo, la taskbar quedaría
/// tapada por (o solapada con) el calendario y los rieles.
/// </summary>
internal static class EspacioEscritorio
{
    public const double MargenSuperior = 32;
    public const double MargenInferiorReservado = 100;

    /// <summary>
    /// Vuelve a correr <paramref name="colocar"/> cada vez que cambia la
    /// configuración de pantallas.
    /// </summary>
    /// <remarks>
    /// Las ventanas del escritorio se colocan una sola vez, contra
    /// <c>SystemParameters.WorkArea</c> — el área de trabajo de la pantalla
    /// principal. Eso las deja en una posición fija, que es lo que se quiere,
    /// pero fija respecto de una configuración de monitores que acá no es
    /// estable: la tableta digitalizadora entra y sale como una segunda
    /// pantalla y mueve el área de trabajo. Sin esto el widget se quedaba en
    /// las coordenadas viejas, descentrado o directamente fuera de pantalla.
    ///
    /// El evento llega en un hilo del sistema, no en el de UI, de ahí el salto
    /// por el dispatcher.
    /// </remarks>
    public static void SeguirALaPantallaPrincipal(Window ventana, Action colocar)
    {
        void AlCambiar(object? _, EventArgs __) => ventana.Dispatcher.BeginInvoke(colocar);

        SystemEvents.DisplaySettingsChanged += AlCambiar;
        ventana.Closed += (_, _) => SystemEvents.DisplaySettingsChanged -= AlCambiar;
    }
}
