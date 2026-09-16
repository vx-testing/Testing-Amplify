using System.Collections.ObjectModel;
using System.IO;
using Newtonsoft.Json;

namespace DesktopGroups;

public class GroupManager
{
    public static GroupManager Instance { get; } = new();

    private readonly string _dataDir =
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                     "DesktopGroups");

    private string DataFile => Path.Combine(_dataDir, "groups.json");
    public ObservableCollection<GroupData> Groups { get; private set; } = new();

    private GroupManager() { }

    public void Load()
    {
        Directory.CreateDirectory(_dataDir);
        if (!File.Exists(DataFile)) return;
        try
        {
            var json = File.ReadAllText(DataFile);
            var list = JsonConvert.DeserializeObject<List<GroupData>>(json);
            if (list != null) Groups = new ObservableCollection<GroupData>(list);
        }
        catch { }
    }

    public void Save()
    {
        Directory.CreateDirectory(_dataDir);
        File.WriteAllText(DataFile, JsonConvert.SerializeObject(Groups, Formatting.Indented));
    }

    public GroupData CreateGroup(string name = "Nuevo Grupo")
    {
        var g = new GroupData { Name = name };
        Groups.Add(g);
        Save();
        ShortcutHelper.CreateOrUpdate(g); // ← crea el .lnk en el escritorio
        return g;
    }

    public void DeleteGroup(string id)
    {
        var g = Groups.FirstOrDefault(x => x.Id == id);
        if (g == null) return;
        ShortcutHelper.Delete(g); // ← elimina el .lnk del escritorio
        Groups.Remove(g);
        Save();
    }

    public void AddShortcut(string groupId, ShortcutItem item)
    {
        var g = Groups.FirstOrDefault(x => x.Id == groupId);
        if (g == null) return;
        if (g.Shortcuts.Any(s => s.TargetPath == item.TargetPath)) return;
        g.Shortcuts.Add(item);
        Save();
    }

    public void RemoveShortcut(string groupId, string targetPath)
    {
        var g = Groups.FirstOrDefault(x => x.Id == groupId);
        var s = g?.Shortcuts.FirstOrDefault(x => x.TargetPath == targetPath);
        if (s != null) { g!.Shortcuts.Remove(s); Save(); }
    }

    public void RenameGroup(string id, string newName)
    {
        var g = Groups.FirstOrDefault(x => x.Id == id);
        if (g == null) return;
        ShortcutHelper.Delete(g); // borrar .lnk con nombre viejo
        g.Name = newName;
        Save();
        ShortcutHelper.CreateOrUpdate(g); // crear .lnk con nombre nuevo
    }

    public void SetGroupIcon(string id, string icoPath)
    {
        var g = Groups.FirstOrDefault(x => x.Id == id);
        if (g == null) return;
        g.IconPath = icoPath;
        Save();
        ShortcutHelper.CreateOrUpdate(g); // actualizar ícono del .lnk
    }
}
