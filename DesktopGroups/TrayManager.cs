using System.IO;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;

namespace DesktopGroups;

public class TrayManager : IDisposable
{
    private TaskbarIcon? _trayIcon;
    private GroupsManagerWindow? _managerWindow;

    public void Initialize()
    {
        _trayIcon = new TaskbarIcon
        {
            ToolTipText = "Desktop Groups",
            Icon        = LoadDefaultIcon(),
        };

        var menu = new ContextMenu();

        var itemNew = new System.Windows.Controls.MenuItem { Header = "Nuevo Grupo" };
        itemNew.Click += (_, _) => new CreateGroupDialog().ShowDialog();

        var itemManage = new System.Windows.Controls.MenuItem { Header = "Gestionar Grupos" };
        itemManage.Click += (_, _) => OpenManager();

        var itemSep = new Separator();

        var itemExit = new System.Windows.Controls.MenuItem { Header = "Salir de Desktop Groups" };
        itemExit.Click += (_, _) => Application.Current.Shutdown();

        menu.Items.Add(itemNew);
        menu.Items.Add(itemManage);
        menu.Items.Add(itemSep);
        menu.Items.Add(itemExit);

        _trayIcon.ContextMenu     = menu;
        _trayIcon.DoubleClickCommand = new RelayCommand(OpenManager);
    }

    private static System.Drawing.Icon LoadDefaultIcon()
    {
        var icoPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                                   "Assets", "default_group.ico");
        return File.Exists(icoPath)
            ? new System.Drawing.Icon(icoPath)
            : System.Drawing.SystemIcons.Application;
    }

    private void OpenManager()
    {
        if (_managerWindow == null || !_managerWindow.IsLoaded)
            _managerWindow = new GroupsManagerWindow();
        _managerWindow.Show();
        _managerWindow.Activate();
    }

    public void Dispose() => _trayIcon?.Dispose();
}
