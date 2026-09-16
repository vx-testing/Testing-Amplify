using System.Collections.ObjectModel;
using Newtonsoft.Json;

namespace DesktopGroups;

public class GroupData
{
    [JsonProperty("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonProperty("name")]
    public string Name { get; set; } = "Nuevo Grupo";

    [JsonProperty("iconPath")]
    public string IconPath { get; set; } = string.Empty;

    [JsonProperty("shortcuts")]
    public ObservableCollection<ShortcutItem> Shortcuts { get; set; } = new();

    [JsonIgnore]
    public System.Windows.Media.ImageSource? IconSource { get; set; }
}
