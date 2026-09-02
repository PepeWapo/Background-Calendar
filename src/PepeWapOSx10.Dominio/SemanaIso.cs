namespace PepeWapOSx10.Dominio;

/// <summary>
/// Semanas que arrancan el lunes, como las muestra el widget y como las
/// interpreta la repetición semanal de la guía.
/// </summary>
/// <remarks>
/// Vive en el dominio porque la usan tanto la UI (para navegar y resaltar)
/// como la persistencia (para decidir si una tarea semanal sigue contando
/// como hecha): antes cada capa tenía su propia copia de la misma fórmula, y
/// que se desincronizaran habría hecho que una tarea se viera hecha en una
/// semana distinta de la que la UI resalta.
/// </remarks>
public static class SemanaIso
{
    /// <summary>El lunes de la semana a la que pertenece <paramref name="fecha"/>.</summary>
    public static DateOnly Inicio(DateOnly fecha) => fecha.AddDays(-((int)fecha.DayOfWeek + 6) % 7);

    /// <summary>Posición del día dentro de su semana: 0 = lunes … 6 = domingo.</summary>
    public static int IndiceDeDia(DateOnly fecha) => ((int)fecha.DayOfWeek + 6) % 7;
}
