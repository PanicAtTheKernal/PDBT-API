namespace PDBT.Models;

public class LinkedIssue
{
    public int Id { get; set; }
    public int IssueId { get; set; }
    public Issue Issue { get; set; } = null!;
    public IssueReason Reason { get; set; } = IssueReason.Duplicate;
}

public enum IssueReason
{
    Blocked,
    Linked,
    Duplicate
}