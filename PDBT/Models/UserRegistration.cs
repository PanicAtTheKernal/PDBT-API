using System.ComponentModel.DataAnnotations;

namespace PDBT.Models;

public class UserRegistration
{
    [EmailAddress] 
    public string Email { get; set; } = null!;

    [MinLength(6), MaxLength(64)]
    public string Password { get; set; } = null!;

    [Compare("Password")] 
    public string ConfirmPassword { get; set; } = null!;
    
    public string FirstName { get; set; } = null!;
    public string  LastName { get; set; } = null!;
    public string Username { get; set; } = null!;
}