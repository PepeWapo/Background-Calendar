using System.Windows.Media;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Un icono dentro de un <see cref="IconRailWindow"/>, respaldado por un
/// acceso directo real (<see cref="Target"/> es la ruta al .lnk, lanzable
/// directo con <c>Process.Start</c>). <see cref="Icono"/> es el ícono real
/// extraído de ese acceso directo (<see cref="RielIconos"/>).
/// </summary>
internal sealed record IconRailItem(string Label, string Target, ImageSource Icono);
