using System.Windows;

namespace DesktopGroups;

public partial class CreateGroupDialog : Window
{
    public CreateGroupDialog()
    {
        InitializeComponent();
        Loaded += (_, _) => { TxtName.SelectAll(); TxtName.Focus(); };
    }

    private void Create_Click(object sender, RoutedEventArgs e) => Confirm();

    private void TxtName_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == System.Windows.Input.Key.Enter)  Confirm();
        if (e.Key == System.Windows.Input.Key.Escape) { DialogResult = false; Close(); }
    }

    private void Confirm()
    {
        var name = TxtName.Text.Trim();
        if (string.IsNullOrEmpty(name)) name = "Nuevo Grupo";
        GroupManager.Instance.CreateGroup(name);
        DialogResult = true;
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
        { DialogResult = false; Close(); }
}
