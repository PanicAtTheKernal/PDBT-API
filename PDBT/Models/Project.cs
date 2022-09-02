namespace PDBT.Models;

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public ICollection<Issue> Issues { get; set; }
    public ICollection<User> Users { get; set; }
    public ICollection<Label> Labels { get; set; }
}