using System.Collections.ObjectModel;
using System.IO;

namespace PepeWapOSx10.Shell.Widgets;

internal enum LadoEscritorio { Izquierda, Derecha }

/// <summary>
/// El contenido de los dos rieles: qué ícono está en cada lado y en qué orden,
/// y cómo se mueve uno de un lugar a otro.
/// </summary>
/// <remarks>
/// Las dos listas viven acá y no una en cada <see cref="IconRailWindow"/>:
/// arrastrar un ícono de un riel al otro es una operación sobre <em>ambas</em>
/// listas más el guardado del orden, así que cuando cada ventana era dueña de
/// su lista, la que recibía el drop tenía que alcanzar la colección privada de
/// la otra para completarlo. Con el contenido en un solo objeto, cada ventana
/// solo muestra su lado y pide el movimiento.
/// </remarks>
internal sealed class RielesDeIconos
{
    public ObservableCollection<IconRailItem> Izquierda { get; }

    public ObservableCollection<IconRailItem> Derecha { get; }

    private RielesDeIconos(IEnumerable<IconRailItem> izquierda, IEnumerable<IconRailItem> derecha)
    {
        Izquierda = new ObservableCollection<IconRailItem>(izquierda);
        Derecha = new ObservableCollection<IconRailItem>(derecha);
    }

    /// <summary>
    /// Migra los accesos directos que hayan aparecido en el escritorio real y
    /// arma ambos rieles en el orden persistido.
    /// </summary>
    public static RielesDeIconos MigrarYCargar()
    {
        AccesosDirectos.MigrarDesdeElEscritorio();

        var reparto = OrdenDeRieles.ReconciliarCon(AccesosDirectos.Listar());
        OrdenDeRieles.Guardar(reparto.Izquierda, reparto.Derecha);

        return new RielesDeIconos(Cargar(reparto.Izquierda), Cargar(reparto.Derecha));
    }

    public ObservableCollection<IconRailItem> De(LadoEscritorio lado) =>
        lado == LadoEscritorio.Izquierda ? Izquierda : Derecha;

    /// <summary>
    /// Mueve un ícono a la posición indicada de un riel — desde el otro riel o
    /// desde otra posición del mismo — y persiste el resultado.
    /// </summary>
    public void Mover(IconRailItem item, LadoEscritorio destino, int indice)
    {
        var origen = Izquierda.Contains(item) ? Izquierda : Derecha;
        var llegada = De(destino);

        var indiceOrigen = origen.IndexOf(item);
        if (indiceOrigen < 0)
            return;

        origen.RemoveAt(indiceOrigen);

        // Si salió de más arriba en la misma lista, todo lo de abajo ya se
        // corrió un lugar y el índice de destino apunta un casillero de más.
        if (ReferenceEquals(origen, llegada) && indiceOrigen < indice)
            indice--;

        llegada.Insert(Math.Clamp(indice, 0, llegada.Count), item);
        Guardar();
    }

    /// <summary>
    /// Saca un ícono del riel en el que esté y persiste el resultado.
    /// </summary>
    /// <returns><c>false</c> si el ícono ya no estaba en ninguno de los dos.</returns>
    public bool Quitar(IconRailItem item)
    {
        if (!Izquierda.Remove(item) && !Derecha.Remove(item))
            return false;

        Guardar();
        return true;
    }

    private void Guardar() => OrdenDeRieles.Guardar(Nombres(Izquierda), Nombres(Derecha));

    private static IEnumerable<string> Nombres(IEnumerable<IconRailItem> items) =>
        items.Select(i => Path.GetFileName(i.Target));

    private static IEnumerable<IconRailItem> Cargar(IEnumerable<string> nombres) =>
        nombres.Select(AccesosDirectos.Cargar).OfType<IconRailItem>();
}
