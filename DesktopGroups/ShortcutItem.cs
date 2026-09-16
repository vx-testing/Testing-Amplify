using Newtonsoft.Json;

namespace DesktopGroups;

public class ShortcutItem
{
    [JsonProperty("name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("targetPath")]
    public string TargetPath { get; set; } = string.Empty;

    [JsonIgnore]
    public System.Windows.Media.ImageSource? Icon { get; set; }
}
