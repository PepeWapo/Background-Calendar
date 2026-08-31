namespace PepeWapOSx10.Dominio.Modelos;

/// <summary>Cada cuánto vuelve a estar pendiente una tarea de la guía.</summary>
public enum Repeticion
{
    /// <summary>Se hace una sola vez; una vez marcada no vuelve.</summary>
    Unica,

    /// <summary>Vuelve a estar pendiente al empezar la semana siguiente (lunes).</summary>
    Semanal,

    /// <summary>Vuelve a estar pendiente al empezar el mes siguiente.</summary>
    Mensual,
}

/// <param name="Hecha">
/// Si ya se hizo dentro del período vigente según <paramref name="Repeticion"/>.
/// Es un valor derivado de <paramref name="UltimaVez"/>, no un estado propio.
/// </param>
public sealed record TareaGuia(
    string Id,
    string Title,
    string Categoria,
    bool Hecha = false,
    DateOnly? UltimaVez = null,
    Repeticion Repeticion = Repeticion.Unica);
