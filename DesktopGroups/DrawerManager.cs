using System.Windows;

namespace DesktopGroups;

public static class DrawerManager
{
    private static DrawerWindow? _current;

    public static void OpenDrawer(GroupData group)
    {
        if (_current != null && _current.IsLoaded)
        {
            _current.ForceClose();
            _current = null;
        }

        // Usar la posición del cursor: siempre estará cerca del ícono clicado
        var cursorPos = GetCursorPosition();

        _current = new DrawerWindow(group, cursorPos);
        _current.Show();
        _current.Activate();
    }

    private static System.Windows.Point GetCursorPosition()
    {
        // Obtener posición del cursor en coordenadas de pantalla
        var p = System.Windows.Forms.Cursor.Position;
        return new System.Windows.Point(p.X, p.Y);
    }
}
