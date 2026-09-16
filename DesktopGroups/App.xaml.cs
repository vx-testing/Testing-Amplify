using System.Windows;

namespace DesktopGroups;

public partial class App : System.Windows.Application
{
    private TrayManager? _trayManager;
    private CancellationTokenSource? _pipeCts;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Intentar ser la instancia primaria
        if (!SingleInstance.TryBecomePrimary(e.Args, out _pipeCts))
        {
            // Ya hay una instancia corriendo, ella recibió los args → salir
            Shutdown();
            return;
        }

        GroupManager.Instance.Load();
        _trayManager = new TrayManager();
        _trayManager.Initialize();
        StartupHelper.RegisterStartup();

        // Si se lanzó con --open, abrir el cajón
        if (e.Args.Length >= 2 && e.Args[0] == "--open")
        {
            var group = GroupManager.Instance.Groups
                .FirstOrDefault(g => g.Id == e.Args[1]);
            if (group != null)
                DrawerManager.OpenDrawer(group);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _pipeCts?.Cancel();
        _trayManager?.Dispose();
        base.OnExit(e);
    }
}
