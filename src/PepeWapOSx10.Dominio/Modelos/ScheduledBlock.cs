namespace PepeWapOSx10.Dominio.Modelos;

public enum BlockKind
{
    Fixed,
    Flexible,
    Libre,
}

public sealed record ScheduledBlock(
    string Title,
    DateTime Start,
    DateTime End,
    BlockKind Kind,
    string? TaskId = null);
