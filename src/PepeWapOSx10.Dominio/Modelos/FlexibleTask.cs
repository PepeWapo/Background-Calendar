namespace PepeWapOSx10.Dominio.Modelos;

public sealed record FlexibleTask(
    string Id,
    string Title,
    int DurationMin,
    int Priority,
    IReadOnlyList<DayOfWeek>? DiasPermitidos,
    TimeOnly? HoraMin,
    TimeOnly? HoraMax,
    bool Recurrente = false);
