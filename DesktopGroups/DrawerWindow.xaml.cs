using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DesktopGroups;

public partial class DrawerWindow : Window
{
    private readonly GroupData _group;
    private bool _forceClose = false;

    public DrawerWindow(GroupData group, System.Windows.Point anchorBottomCenter)
    {
        InitializeComponent();
        _group = group;
        Loaded += (_, _) => { PositionWindow(anchorBottomCenter); PopulateGroup(); };
    }

    public void ForceClose()
    {
        _forceClose = true;
        Close();
    }

    private void PositionWindow(System.Windows.Point anchor)
    {
        Left = anchor.X - (ActualWidth / 2);
        Top  = anchor.Y + 8;
        var wa = SystemParameters.WorkArea;
        if (Left + ActualWidth  > wa.Right)  Left = wa.Right  - ActualWidth  - 6;
        if (Left                < wa.Left)   Left = wa.Left   + 6;
        if (Top  + ActualHeight > wa.Bottom) Top  = anchor.Y  - ActualHeight - 8;
    }

    private void PopulateGroup()
    {
        TxtGroupName.Text = _group.Name;
        if (!string.IsNullOrEmpty(_group.IconPath) && File.Exists(_group.IconPath))
            ImgGroupIcon.Source = new BitmapImage(new Uri(_group.IconPath));
        else
            ImgGroupIcon.Source = NativeMethods.GetFileIcon(
                System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName, false);

        foreach (var s in _group.Shortcuts)
            if (s.Icon == null && File.Exists(s.TargetPath))
                s.Icon = NativeMethods.GetFileIcon(s.TargetPath, false);

        ShortcutList.ItemsSource = _group.Shortcuts;
    }

    private void MainPanel_DragOver(object sender, System.Windows.DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)
                    ? System.Windows.DragDropEffects.Link
                    : System.Windows.DragDropEffects.None;
        e.Handled = true;
    }

    private void MainPanel_Drop(object sender, System.Windows.DragEventArgs e)
    {
        if (!e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop)) return;
        foreach (var path in (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop))
        {
            GroupManager.Instance.AddShortcut(_group.Id, new ShortcutItem
            {
                Name       = Path.GetFileNameWithoutExtension(path),
                TargetPath = path,
                Icon       = NativeMethods.GetFileIcon(path, false)
            });
        }
        ShortcutList.ItemsSource = null;
        ShortcutList.ItemsSource = _group.Shortcuts;
    }

    private void ShortcutBtn_Click(object sender, RoutedEventArgs e)
    {
        var path = (string)((System.Windows.Controls.Button)sender).Tag;
        if (File.Exists(path) || Directory.Exists(path))
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                { FileName = path, UseShellExecute = true });
            ForceClose();
        }
    }

    private void RemoveShortcut_Click(object sender, RoutedEventArgs e)
    {
        var path = (string)((System.Windows.Controls.MenuItem)sender).Tag;
        GroupManager.Instance.RemoveShortcut(_group.Id, path);
        ShortcutList.ItemsSource = null;
        ShortcutList.ItemsSource = _group.Shortcuts;
    }

    protected override void OnDeactivated(EventArgs e)
    {
        base.OnDeactivated(e);
        if (!_forceClose) Close();
    }
}
