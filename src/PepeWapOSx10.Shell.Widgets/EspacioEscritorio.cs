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
}
