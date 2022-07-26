namespace PDBT.Models;

public class LinkedIssue
{
    public int Id { get; set; }
    public Issue Issue1 { get; set; } = null!;
    public Issue Issue2 { get; set; } = null!;
    public IssueReason Reason { get; set; } = IssueReason.Duplicate;
}

public enum IssueReason
{
    Blocked,
    Linked,
    Duplicate
}