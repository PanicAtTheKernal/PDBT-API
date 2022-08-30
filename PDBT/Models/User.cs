using System.Text.Json.Serialization;

namespace PDBT.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public byte[] PasswordHash { get; set; } = new byte[32];
    public byte[] PasswordSalt { get; set; } = new byte[32];
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenCreated { get; set; }
    public DateTime? RefreshTokenExpires { get; set; }
    [JsonIgnore]
    public ICollection<Project> Projects { get; set; }
    public ICollection<Issue> AssignedIssues { get; set; }
}