using System.Globalization;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.Win32;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// El reloj de 24 horas que va arriba del mini-calendario, con la hora local
/// del sistema.
/// </summary>
/// <remarks>
/// No hace tic cada un intervalo fijo: cada vez calcula cuánto falta para el
/// próximo minuto, así cambia junto con el reloj del sistema y no acumula
/// atraso. Como solo despierta una vez por minuto, además escucha el cambio de
/// hora y la salida de suspensión para no quedar mostrando una hora vieja.
/// </remarks>
internal sealed class RelojDigital : IDisposable
{
    /// <summary>Para caer apenas pasado el cambio de minuto y no justo antes.</summary>
    private static readonly TimeSpan Holgura = TimeSpan.FromMilliseconds(50);

    private readonly TextBlock _texto;
    // Prioridad normal: con la de fábrica (Background) el cambio de minuto espera
    // detrás de la carga del arranque.
    private readonly DispatcherTimer _timer = new(DispatcherPriority.Normal);

    public RelojDigital(TextBlock texto)
    {
        _texto = texto;
        _timer.Tick += (_, _) => Actualizar();

        SystemEvents.TimeChanged += AlCambiarLaHora;
        SystemEvents.PowerModeChanged += AlCambiarLaEnergia;

        Actualizar();
    }

    private void Actualizar()
    {
        var ahora = DateTime.Now;
        _texto.Text = ahora.ToString("HH:mm", CultureInfo.InvariantCulture);

        _timer.Stop();
        _timer.Interval = TimeSpan.FromTicks(TimeSpan.TicksPerMinute - ahora.Ticks % TimeSpan.TicksPerMinute) + Holgura;
        _timer.Start();
    }

    private void AlCambiarLaHora(object? sender, EventArgs e)
    {
        // .NET guarda la zona horaria en caché: sin esto un cambio de zona no se ve.
        TimeZoneInfo.ClearCachedData();
        _texto.Dispatcher.BeginInvoke(Actualizar);
    }

    private void AlCambiarLaEnergia(object sender, PowerModeChangedEventArgs e)
    {
        if (e.Mode == PowerModes.Resume)
            _texto.Dispatcher.BeginInvoke(Actualizar);
    }

    public void Dispose()
    {
        // Son eventos estáticos: sin desuscribirse retienen el reloj.
        SystemEvents.TimeChanged -= AlCambiarLaHora;
        SystemEvents.PowerModeChanged -= AlCambiarLaEnergia;
        _timer.Stop();
    }
}
