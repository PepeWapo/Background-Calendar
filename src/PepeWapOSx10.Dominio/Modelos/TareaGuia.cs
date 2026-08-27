namespace PepeWapOSx10.Dominio.Modelos;

public sealed record TareaGuia(
    string Id,
    string Title,
    string Categoria,
    bool HechaHoy = false,
    DateOnly? UltimaVez = null);
