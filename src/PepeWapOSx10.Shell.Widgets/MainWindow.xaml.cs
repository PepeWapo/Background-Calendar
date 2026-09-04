using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Threading;
using PepeWapOSx10.Datos;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// El widget de agenda: el panel central con las tres vistas, el
/// mini-calendario a la izquierda y la guía de tareas a la derecha.
/// </summary>
/// <remarks>
/// Esta clase arma las piezas y reparte la navegación entre ellas; el trabajo
/// lo hacen <see cref="VistaDia"/>, <see cref="VistaSemana"/>,
/// <see cref="VistaMes"/>, <see cref="MiniCalendario"/> y
/// <see cref="PanelGuia"/>, sobre los datos que les da
/// <see cref="IAgendaService"/>. Las otras ventanas de escritorio (wallpaper y
/// rieles) las administra <see cref="EscritorioShell"/>.
/// </remarks>
public partial class MainWindow : Window
{
    private enum Vista { Dia, Semana, Mes }

    private Vista _vista = Vista.Dia;

    private EscritorioShell? _escritorio;
    private MiniCalendario? _miniCalendario;
    private PanelGuia? _guia;
    private VistaDia? _dia;
    private VistaSemana? _semana;
    private VistaMes? _mes;

    public MainWindow()
    {
        InitializeComponent();

        Colocar();
        EspacioEscritorio.SeguirALaPantallaPrincipal(this, Colocar);

        SourceInitialized += (_, _) =>
        {
            AnclajeEscritorio.OcultarDeAltTab(this);
            AnclajeEscritorio.ImpedirMinimizado(this);
        };
    }

    /// <summary>
    /// Alto y posición calculados en vez de fijos en el XAML: deja el mismo
    /// margen superior que los rieles de iconos (misma línea de arranque
    /// arriba) y reserva espacio libre abajo para la taskbar del widget.
    /// </summary>
    private void Colocar()
    {
        var area = SystemParameters.WorkArea;
        Height = area.Height - EspacioEscritorio.MargenSuperior - EspacioEscritorio.MargenInferiorReservado;
        Top = area.Top + EspacioEscritorio.MargenSuperior;
        Left = area.Left + (area.Width - Width) / 2;
    }

    /// <summary>
    /// Muestra el esqueleto de entrada y va reemplazándolo por los datos reales
    /// a medida que llegan.
    /// </summary>
    /// <remarks>
    /// El orden importa y no es el natural. Antes esto era todo secuencial —
    /// abrir el escritorio, abrir la base, pedir la agenda — y recién al final
    /// aparecía algo: el widget se veía como tres paneles vacíos durante lo que
    /// tardara la primera bajada del ICS. Ahora primero se dibuja el esqueleto
    /// y se le cede el turno al dispatcher para que ese frame llegue a
    /// pantalla; después arranca todo lo caro, y lo que no necesita el hilo de
    /// UI (abrir y migrar la base) se va a un hilo de fondo.
    /// </remarks>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        Esqueleto.Mostrar(CalendarDaysGrid, AgendaCanvas, GuiaPanel);
        ActualizarUiDeVista(Vista.Dia);

        // Cede el turno hasta que no quede trabajo de layout ni de render
        // pendiente: sin esto lo de abajo se encola antes del primer frame y
        // el esqueleto no se llega a ver nunca.
        await Dispatcher.Yield(DispatcherPriority.ContextIdle);

        // Las ventanas de escritorio son ventanas WPF: se crean sí o sí en este
        // hilo. Van después del esqueleto para no demorar ese primer frame.
        _escritorio = new EscritorioShell(this);

        if (!await ArmarPanelesAsync())
        {
            Esqueleto.Quitar(CalendarDaysGrid, AgendaCanvas, GuiaPanel);
            return;
        }

        await _guia!.RefrescarAsync();
        await _dia!.MostrarAsync(DateOnly.FromDateTime(DateTime.Today));
    }

    private void Window_Closed(object sender, EventArgs e) => _escritorio?.Dispose();

    /// <summary>
    /// Al soltar el click, el widget vuelve al fondo del z-order.
    /// </summary>
    /// <remarks>
    /// Clickear el widget lo activa, y activar una ventana la sube al tope de
    /// su banda del z-order — no se puede evitar sin volverla no activable, y
    /// esta necesita el foco de verdad (botones, edición de tareas). Se acepta
    /// entonces que suba mientras dure el gesto, y se la baja al terminarlo.
    ///
    /// Va en el evento de vista previa porque tunelea desde la raíz: llega
    /// siempre, aunque el control clickeado marque el <c>up</c> como manejado.
    /// </remarks>
    protected override void OnPreviewMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnPreviewMouseLeftButtonUp(e);

        // Después de que termine de despacharse el gesto: el reanclaje mira el
        // estado del botón para no pisar un click en curso, y acá todavía es
        // parte de ese click.
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () => _escritorio?.Reanclar());
    }

    /// <summary>
    /// Cablea los paneles contra la base de datos y el calendario.
    /// </summary>
    /// <returns>
    /// <c>false</c> si no se pudo abrir la base — sin ella no hay ni agenda ni
    /// guía que mostrar, así que el widget se queda con el mensaje de error a
    /// la vista en vez de caerse.
    /// </returns>
    private async Task<bool> ArmarPanelesAsync()
    {
        IAgendaService agenda;
        AgendaDbContext contexto;

        try
        {
            // Abrir el archivo SQLite y correr el esquema/semilla es E/S pura y
            // no toca ningún objeto de WPF, así que se hace fuera del hilo de
            // UI: es lo más lento del arranque después del ICS.
            contexto = await Task.Run(() =>
            {
                var abierto = new AgendaDbContext();
                abierto.Inicializar();
                return abierto;
            });
            agenda = new AgendaService(contexto);
        }
        catch (Exception ex)
        {
            GuiaSummaryText.Text = $"No se pudo abrir la base: {ex.Message}";
            DiaFooterText.Text = GuiaSummaryText.Text;
            return false;
        }

        _guia = new PanelGuia(GuiaPanel, GuiaSummaryText, this, new SqliteTareaGuiaRepository(contexto));

        _miniCalendario = new MiniCalendario(
            CalendarMonthText, CalendarYearText, CalendarDaysGrid,
            abrirDia: dia => _ = CambiarAsync(Vista.Dia, dia));

        _dia = new VistaDia(
            DiaScroll, AgendaCanvas, DiaHeaderDiaText, DiaHeaderMesText, DiaHeaderAnioText, DiaFooterText, agenda,
            alMostrarDia: fecha =>
            {
                if (_vista == Vista.Dia)
                    _miniCalendario!.Mostrar(fecha);
            });

        _semana = new VistaSemana(
            SemanaHeadGrid, SemanaScroll, SemanaCanvas, NavLabelText, AgendaSubtitleText, agenda,
            abrirDia: dia => _ = CambiarAsync(Vista.Dia, dia));

        _mes = new VistaMes(
            MesHeadGrid, MesRowsGrid, NavLabelText, AgendaSubtitleText, agenda,
            abrirDia: dia => _ = CambiarAsync(Vista.Dia, dia),
            abrirSemana: semana => _ = CambiarAsync(Vista.Semana, semana));

        return true;
    }

    // ===================== navegación entre vistas =====================

    private void TabVista_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton { Tag: string tag })
            _ = CambiarAsync(Enum.Parse<Vista>(tag));
    }

    private void NavPrev_Click(object sender, RoutedEventArgs e) => _ = DesplazarAsync(-1);

    private void NavNext_Click(object sender, RoutedEventArgs e) => _ = DesplazarAsync(+1);

    private void NavHoy_Click(object sender, RoutedEventArgs e) => _ = IrAHoyAsync();

    private void GuiaAgregar_Click(object sender, RoutedEventArgs e) => _ = _guia?.AgregarAsync();

    /// <param name="fecha">
    /// A dónde ir dentro de la vista nueva: el día a mostrar, o el lunes de la
    /// semana. Si no se pasa, cada vista se queda donde estaba.
    /// </param>
    private async Task CambiarAsync(Vista vista, DateOnly? fecha = null)
    {
        if (_dia is null)
            return;

        _vista = vista;
        ActualizarUiDeVista(vista);

        switch (vista)
        {
            case Vista.Dia:
                var dia = fecha ?? DateOnly.FromDateTime(DateTime.Today);
                _miniCalendario!.Mostrar(dia);
                await _dia.MostrarAsync(dia);
                break;
            case Vista.Semana:
                await _semana!.MostrarAsync(fecha ?? _semana.Inicio);
                break;
            case Vista.Mes:
                await _mes!.MostrarAsync(fecha ?? _mes.Mes);
                break;
        }
    }

    /// <summary>Un paso hacia atrás o hacia adelante en la vista activa (semana o mes).</summary>
    private Task DesplazarAsync(int pasos) => _vista switch
    {
        Vista.Semana => _semana!.DesplazarAsync(pasos),
        Vista.Mes => _mes!.DesplazarAsync(pasos),
        _ => Task.CompletedTask, // la vista Día navega con el scroll continuo, no con flechas.
    };

    private Task IrAHoyAsync() => _vista switch
    {
        Vista.Semana => _semana!.IrAHoyAsync(),
        Vista.Mes => _mes!.IrAHoyAsync(),
        _ => Task.CompletedTask,
    };

    private void ActualizarUiDeVista(Vista vista)
    {
        TabDia.IsChecked = vista == Vista.Dia;
        TabSemana.IsChecked = vista == Vista.Semana;
        TabMes.IsChecked = vista == Vista.Mes;

        DiaPanel.Visibility = Visible(vista == Vista.Dia);
        SemanaPanel.Visibility = Visible(vista == Vista.Semana);
        MesPanel.Visibility = Visible(vista == Vista.Mes);

        // La vista Día trae su propio encabezado con la fecha en grande; las
        // otras dos usan el genérico y la fila de navegación.
        DiaHeaderStack.Visibility = Visible(vista == Vista.Dia);
        AgendaHeaderGenerico.Visibility = Visible(vista != Vista.Dia);
        AgendaNavRow.Visibility = Visible(vista != Vista.Dia);

        NavHintText.Text = vista switch
        {
            Vista.Semana => "click resalta el día · doble click abre su vista Día",
            Vista.Mes => "click resalta día/semana · doble click navega",
            _ => string.Empty,
        };
    }

    private static Visibility Visible(bool visible) => visible ? Visibility.Visible : Visibility.Collapsed;
}
