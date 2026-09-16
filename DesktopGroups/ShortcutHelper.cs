using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DesktopGroups;

/// <summary>
/// Crea y elimina accesos directos .lnk en el escritorio para cada grupo.
/// Usa COM directamente para no necesitar librerías extra.
/// </summary>
public static class ShortcutHelper
{
    private static string DesktopPath =>
        Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);

    private static string ExePath =>
        System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;

    public static string GetShortcutPath(GroupData group) =>
        Path.Combine(DesktopPath, $"{group.Name}.lnk");

    /// <summary>Crea o actualiza el .lnk en el escritorio para este grupo.</summary>
    public static void CreateOrUpdate(GroupData group)
    {
        var lnkPath = GetShortcutPath(group);

        // Borrar el anterior si cambió el nombre
        foreach (var old in Directory.GetFiles(DesktopPath, "*.lnk"))
        {
            // Solo borramos si el target apunta a nuestra app y tiene el ID del grupo
            try
            {
                var existing = ResolveShortcutArguments(old);
                if (existing != null && existing.Contains(group.Id))
                {
                    File.Delete(old);
                    break;
                }
            }
            catch { }
        }

        CreateShortcut(
            lnkPath:     lnkPath,
            targetPath:  ExePath,
            arguments:   $"--open {group.Id}",
            description: $"Desktop Group: {group.Name}",
            iconPath:    string.IsNullOrEmpty(group.IconPath) ? ExePath : group.IconPath
        );
    }

    /// <summary>Elimina el .lnk del escritorio para este grupo.</summary>
    public static void Delete(GroupData group)
    {
        // Buscar por ID en los argumentos del shortcut
        foreach (var lnk in Directory.GetFiles(DesktopPath, "*.lnk"))
        {
            try
            {
                var args = ResolveShortcutArguments(lnk);
                if (args != null && args.Contains(group.Id))
                {
                    File.Delete(lnk);
                    return;
                }
            }
            catch { }
        }
    }

    // ── COM interop para crear .lnk ───────────────────────────────────────────
    [ComImport, Guid("00021401-0000-0000-C000-000000000046")]
    private class ShellLink { }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("000214F9-0000-0000-C000-000000000046")]
    private interface IShellLink
    {
        void GetPath([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszFile,
                     int cchMaxPath, IntPtr pfd, uint fFlags);
        void GetIDList(out IntPtr ppidl);
        void SetIDList(IntPtr pidl);
        void GetDescription([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszName, int cchMaxName);
        void SetDescription([MarshalAs(UnmanagedType.LPWStr)] string pszName);
        void GetWorkingDirectory([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszDir, int cchMaxPath);
        void SetWorkingDirectory([MarshalAs(UnmanagedType.LPWStr)] string pszDir);
        void GetArguments([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszArgs, int cchMaxPath);
        void SetArguments([MarshalAs(UnmanagedType.LPWStr)] string pszArgs);
        void GetHotkey(out short pwHotkey);
        void SetHotkey(short wHotkey);
        void GetShowCmd(out int piShowCmd);
        void SetShowCmd(int iShowCmd);
        void GetIconLocation([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszIconPath,
                             int cchIconPath, out int piIcon);
        void SetIconLocation([MarshalAs(UnmanagedType.LPWStr)] string pszIconPath, int iIcon);
        void SetRelativePath([MarshalAs(UnmanagedType.LPWStr)] string pszPathRel, uint dwReserved);
        void Resolve(IntPtr hwnd, uint fFlags);
        void SetPath([MarshalAs(UnmanagedType.LPWStr)] string pszFile);
    }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("0000010C-0000-0000-C000-000000000046")]
    private interface IPersist { void GetClassID(out Guid pClassID); }

    [ComImport, InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
     Guid("0000010B-0000-0000-C000-000000000046")]
    private interface IPersistFile : IPersist
    {
        new void GetClassID(out Guid pClassID);
        void IsDirty();
        void Load([MarshalAs(UnmanagedType.LPWStr)] string pszFileName, uint dwMode);
        void Save([MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
                  [MarshalAs(UnmanagedType.Bool)] bool fRemember);
        void SaveCompleted([MarshalAs(UnmanagedType.LPWStr)] string pszFileName);
        void GetCurFile([Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder ppszFileName);
    }

    private static void CreateShortcut(string lnkPath, string targetPath,
        string arguments, string description, string iconPath)
    {
        var link = (IShellLink)new ShellLink();
        link.SetPath(targetPath);
        link.SetArguments(arguments);
        link.SetDescription(description);
        link.SetIconLocation(iconPath, 0);
        link.SetWorkingDirectory(Path.GetDirectoryName(targetPath)!);
        var file = (IPersistFile)link;
        file.Save(lnkPath, false);
    }

    private static string? ResolveShortcutArguments(string lnkPath)
    {
        try
        {
            var link = (IShellLink)new ShellLink();
            var file = (IPersistFile)link;
            file.Load(lnkPath, 0);
            var sb = new StringBuilder(1024);
            link.GetArguments(sb, sb.Capacity);
            return sb.ToString();
        }
        catch { return null; }
    }
}
