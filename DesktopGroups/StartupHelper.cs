using Microsoft.Win32;

namespace DesktopGroups;

public static class StartupHelper
{
    private const string AppName = "DesktopGroups";
    private const string RunKey  = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

    public static void RegisterStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
        var exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule?.FileName;
        if (exePath != null) key?.SetValue(AppName, $"\"{exePath}\"");
    }

    public static void UnregisterStartup()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
    }

    public static bool IsRegistered()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, false);
        return key?.GetValue(AppName) != null;
    }
}
