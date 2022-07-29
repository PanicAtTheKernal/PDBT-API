using System.Text.Json.Serialization;

namespace PDBT.Models;

public class Issue
{
    public int Id { get; set; }
    public string IssueName { get; set; } = null!;
    public string Description { get; set; } = null!;
    public IssueType Type { get; set; } = IssueType.Bug;
    public IssuePriority Priority { get; set; } = IssuePriority.Medium;
    public DateTime? TimeForCompletion { get; set; }
    public DateTime? DueDate { get; set; }
    public ICollection<LinkedIssue>? LinkedIssues { get; set; }
    public ICollection<Label> Labels { get; set; }
}

public enum IssuePriority
{
    Lowest,
    Low,
    Medium,
    High,
    Highest
}

public enum IssueType
{
    Bug,
    Improvement,
    QualityOfLife,
    NewFeature
}