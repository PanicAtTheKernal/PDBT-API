using System.Text.Json.Serialization;

namespace PDBT.Models;

public class Label
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    [JsonIgnore]
    public ICollection<Issue> Issues { get; set; }

    [JsonIgnore] public Project RootProject { get; set; } = null!;
}