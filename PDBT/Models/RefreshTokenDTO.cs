namespace PDBT.Models;

public class RefreshTokenDTO
{
    public string Token { get; set; } = string.Empty;
    public string JWT { get; set; } = string.Empty;
    public DateTime Expries { get; set; }
}