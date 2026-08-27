using PepeWapOSx10.Dominio.Modelos;

namespace PepeWapOSx10.Dominio;

public static class Scheduler
{
    public static (DateTime Inicio, DateTime Fin) InferirVentanaDia(IReadOnlyList<FixedEvent> fijos, DateOnly fecha)
    {
        if (fijos.Count == 0)
            return (fecha.ToDateTime(new TimeOnly(8, 0)), fecha.ToDateTime(new TimeOnly(23, 0)));

        return (fijos.Min(f => f.Start), fijos.Max(f => f.End));
    }

    public static List<(DateTime Start, DateTime End)> CalcularHuecos(
        IReadOnlyList<FixedEvent> fijos, DateTime inicioDia, DateTime finDia)
    {
        var huecos = new List<(DateTime Start, DateTime End)>();
        var cursor = inicioDia;

        foreach (var evento in fijos.OrderBy(e => e.Start))
        {
            if (evento.Start > cursor)
                huecos.Add((cursor, evento.Start));

            if (evento.End > cursor)
                cursor = evento.End;
        }

        if (cursor < finDia)
            huecos.Add((cursor, finDia));

        return huecos;
    }

    public static List<ScheduledBlock> Empaquetar(
        List<(DateTime Start, DateTime End)> huecos,
        IReadOnlyList<FlexibleTask> tareas,
        DateOnly fecha,
        IReadOnlySet<string> yaAgendadas)
    {
        var diaSemana = DiaAIndice(fecha.DayOfWeek);

        var candidatas = tareas
            .Where(t => t.DiasPermitidos is null || t.DiasPermitidos.Any(d => DiaAIndice(d) == diaSemana))
            .Where(t => t.Recurrente || !yaAgendadas.Contains(t.Id))
            .OrderBy(t => t.Priority);

        var resultado = new List<ScheduledBlock>();

        foreach (var tarea in candidatas)
        {
            var duracion = TimeSpan.FromMinutes(tarea.DurationMin);

            for (var i = 0; i < huecos.Count; i++)
            {
                var (inicio, fin) = huecos[i];
                var inicioValido = tarea.HoraMin is { } horaMin ? MaxHora(inicio, fecha, horaMin) : inicio;
                var finValido = tarea.HoraMax is { } horaMax ? MinHora(fin, fecha, horaMax) : fin;

                if (finValido - inicioValido < duracion)
                    continue;

                var bloqueFin = inicioValido + duracion;
                resultado.Add(new ScheduledBlock(tarea.Title, inicioValido, bloqueFin, BlockKind.Flexible, tarea.Id));

                huecos.RemoveAt(i);
                if (inicioValido > inicio)
                    huecos.Insert(i++, (inicio, inicioValido));
                if (bloqueFin < fin)
                    huecos.Insert(i, (bloqueFin, fin));

                break;
            }
        }

        return resultado;
    }

    public static List<ScheduledBlock> ArmarAgenda(
        IReadOnlyList<FixedEvent> fijos,
        IReadOnlyList<FlexibleTask> tareasLibres,
        DateOnly fecha,
        IReadOnlySet<string> yaAgendadas)
    {
        var (inicioDia, finDia) = InferirVentanaDia(fijos, fecha);
        var huecos = CalcularHuecos(fijos, inicioDia, finDia);
        var empaquetadas = Empaquetar(huecos, tareasLibres, fecha, yaAgendadas);

        var bloques = new List<ScheduledBlock>();
        bloques.AddRange(fijos.Select(f => new ScheduledBlock(f.Title, f.Start, f.End, BlockKind.Fixed)));
        bloques.AddRange(empaquetadas);
        bloques.AddRange(huecos.Select(h => new ScheduledBlock("Libre", h.Start, h.End, BlockKind.Libre)));

        return bloques.OrderBy(b => b.Start).ToList();
    }

    private static int DiaAIndice(DayOfWeek dia) => ((int)dia + 6) % 7;

    private static DateTime MaxHora(DateTime valor, DateOnly fecha, TimeOnly hora)
    {
        var limite = fecha.ToDateTime(hora);
        return valor > limite ? valor : limite;
    }

    private static DateTime MinHora(DateTime valor, DateOnly fecha, TimeOnly hora)
    {
        var limite = fecha.ToDateTime(hora);
        return valor < limite ? valor : limite;
    }
}
