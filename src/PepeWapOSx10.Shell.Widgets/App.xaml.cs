using System.Configuration;
using System.Data;
using System.Windows;
using LibVLCSharp.Shared;

namespace PepeWapOSx10.Shell.Widgets;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Debe llamarse una sola vez, antes de crear cualquier LibVLC —
        // localiza los binarios nativos que trae el paquete VideoLAN.LibVLC.Windows.
        Core.Initialize();
    }
}

