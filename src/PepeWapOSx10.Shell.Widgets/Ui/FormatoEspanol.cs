using System.Globalization;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Nombres de días y meses en español rioplatense, con la mayúscula inicial
/// que espera la UI.
/// </summary>
/// <remarks>
/// Antes cada vista construía su propio <c>new CultureInfo("es-AR")</c> (seis
/// veces entre las tres vistas y el mini-calendario) y repetía a mano el
/// <c>char.ToUpper(nombre[0]) + nombre[1..]</c> para capitalizar, porque
/// <c>es-AR</c> devuelve los nombres en minúscula. Una cultura compartida y un
/// solo lugar donde se capitaliza.
/// </remarks>
internal static class FormatoEspanol
{
    public static readonly CultureInfo Cultura = new("es-AR");

    /// <summary>Iniciales de los días arrancando en lunes, como se muestran en las grillas.</summary>
    public static readonly string[] InicialesDeDia = ["L", "M", "M", "J", "V", "S", "D"];

    public static string Mes(int mes) => Capitalizar(Cultura.DateTimeFormat.GetMonthName(mes));

    public static string DiaDeSemana(DayOfWeek dia) => Capitalizar(Cultura.DateTimeFormat.GetDayName(dia));

    /// <summary>Ej.: <c>Miércoles 2</c> — el encabezado de la vista Día.</summary>
    public static string DiaYNumero(DateOnly fecha) => $"{DiaDeSemana(fecha.DayOfWeek)} {fecha.Day}";

    /// <summary>Ej.: <c>Miércoles 2 de septiembre</c> — el separador de banda de la vista Día.</summary>
    public static string FechaLarga(DateOnly fecha) =>
        $"{DiaYNumero(fecha)} de {Cultura.DateTimeFormat.GetMonthName(fecha.Month)}";

    /// <summary>Ej.: <c>31 de Agosto – 6 de Septiembre 2026</c>, colapsando el mes si es el mismo.</summary>
    public static string RangoDeSemana(DateOnly inicio, DateOnly fin) =>
        inicio.Month == fin.Month
            ? $"{inicio.Day} – {fin.Day} de {Mes(inicio.Month)} {fin.Year}"
            : $"{inicio.Day} de {Mes(inicio.Month)} – {fin.Day} de {Mes(fin.Month)} {fin.Year}";

    private static string Capitalizar(string texto) =>
        texto.Length == 0 ? texto : char.ToUpper(texto[0], Cultura) + texto[1..];
}
