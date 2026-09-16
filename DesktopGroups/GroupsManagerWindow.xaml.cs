using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DesktopGroups;

public partial class GroupsManagerWindow : Window
{
    private GroupData? _selected;

    public GroupsManagerWindow() { InitializeComponent(); RefreshList(); }

    private void RefreshList()
    {
        foreach (var g in GroupManager.Instance.Groups)
            g.IconSource = LoadGroupIconSource(g);

        GroupListBox.ItemsSource = null;
        GroupListBox.ItemsSource = GroupManager.Instance.Groups;

        var factory = new System.Windows.FrameworkElementFactory(
            typeof(System.Windows.Controls.StackPanel));
        factory.SetValue(System.Windows.Controls.StackPanel.OrientationProperty,
            System.Windows.Controls.Orientation.Horizontal);
        var img = new System.Windows.FrameworkElementFactory(
            typeof(System.Windows.Controls.Image));
        img.SetBinding(System.Windows.Controls.Image.SourceProperty,
            new System.Windows.Data.Binding("IconSource"));
        img.SetValue(System.Windows.Controls.Image.WidthProperty,  16.0);
        img.SetValue(System.Windows.Controls.Image.HeightProperty, 16.0);
        img.SetValue(System.Windows.Controls.Image.MarginProperty, new Thickness(0, 0, 8, 0));
        var txt = new System.Windows.FrameworkElementFactory(
            typeof(System.Windows.Controls.TextBlock));
        txt.SetBinding(System.Windows.Controls.TextBlock.TextProperty,
            new System.Windows.Data.Binding("Name"));
        txt.SetValue(System.Windows.Controls.TextBlock.VerticalAlignmentProperty,
            VerticalAlignment.Center);
        factory.AppendChild(img);
        factory.AppendChild(txt);
        GroupListBox.ItemTemplate = new DataTemplate { VisualTree = factory };
    }

    private static System.Windows.Media.ImageSource? LoadGroupIconSource(GroupData g)
    {
        if (!string.IsNullOrEmpty(g.IconPath) && File.Exists(g.IconPath))
            return new BitmapImage(new Uri(g.IconPath));
        return NativeMethods.GetFileIcon(
            System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName, false);
    }

    private void GroupListBox_SelectionChanged(object sender,
        System.Windows.Controls.SelectionChangedEventArgs e)
    {
        _selected = GroupListBox.SelectedItem as GroupData;
        if (_selected == null) { DetailPanel.Visibility = Visibility.Collapsed; return; }
        DetailPanel.Visibility = Visibility.Visible;
        TxtGroupName.Text      = _selected.Name;
        TxtIconPath.Text       = _selected.IconPath;
        foreach (var s in _selected.Shortcuts)
            if (s.Icon == null && File.Exists(s.TargetPath))
                s.Icon = NativeMethods.GetFileIcon(s.TargetPath, false);
        ShortcutListBox.ItemsSource = _selected.Shortcuts;
    }

    private void RenameGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        var name = TxtGroupName.Text.Trim();
        if (!string.IsNullOrEmpty(name))
            { GroupManager.Instance.RenameGroup(_selected.Id, name); RefreshList(); }
    }

    private void BrowseIcon_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        var dlg = new Microsoft.Win32.OpenFileDialog
            { Title = "Seleccionar icono (.ico)", Filter = "Icono (*.ico)|*.ico" };
        if (dlg.ShowDialog() != true) return;
        GroupManager.Instance.SetGroupIcon(_selected.Id, dlg.FileName);
        TxtIconPath.Text = dlg.FileName;
        RefreshList();
    }

    // ── NUEVO: Agregar acceso directo desde explorador ────────────────────────
    private void AddShortcut_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        var dlg = new Microsoft.Win32.OpenFileDialog
        {
            Title  = "Seleccionar acceso directo o ejecutable",
            Filter = "Accesos directos y ejecutables (*.lnk;*.exe)|*.lnk;*.exe|Todos los archivos (*.*)|*.*",
            Multiselect = true
        };
        if (dlg.ShowDialog() != true) return;

        foreach (var path in dlg.FileNames)
        {
            var item = new ShortcutItem
            {
                Name       = Path.GetFileNameWithoutExtension(path),
                TargetPath = path,
                Icon       = NativeMethods.GetFileIcon(path, false)
            };
            GroupManager.Instance.AddShortcut(_selected.Id, item);
        }

        ShortcutListBox.ItemsSource = null;
        ShortcutListBox.ItemsSource = _selected.Shortcuts;
    }

    private void RemoveShortcutManager_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        var path = (string)((System.Windows.Controls.Button)sender).Tag;
        GroupManager.Instance.RemoveShortcut(_selected.Id, path);
        ShortcutListBox.ItemsSource = null;
        ShortcutListBox.ItemsSource = _selected.Shortcuts;
    }

    // ── NUEVO: Recrear .lnk en el escritorio ─────────────────────────────────
    private void RecreateShortcut_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        ShortcutHelper.CreateOrUpdate(_selected);
        System.Windows.MessageBox.Show(
            $"El grupo \"{_selected.Name}\" fue recreado en el escritorio.",
            "Listo", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void DeleteGroup_Click(object sender, RoutedEventArgs e)
    {
        if (_selected == null) return;
        if (System.Windows.MessageBox.Show(
            $"Eliminar \"{_selected.Name}\"?", "Confirmar",
            MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes) return;
        GroupManager.Instance.DeleteGroup(_selected.Id);
        DetailPanel.Visibility = Visibility.Collapsed;
        _selected = null;
        RefreshList();
    }

    private void NewGroup_Click(object sender, RoutedEventArgs e)
        { new CreateGroupDialog().ShowDialog(); RefreshList(); }

    private void CloseBtn_Click(object sender, RoutedEventArgs e) => Close();
}
