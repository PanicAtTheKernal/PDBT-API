namespace PDBT.Models;

public class Label
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Issue> Issues = null!;
}