using System.ComponentModel.DataAnnotations;

namespace PDBT.Models;

public class UserDTO
{
    [EmailAddress]
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}