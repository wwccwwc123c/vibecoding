using System.ComponentModel.DataAnnotations;

namespace Vibecoding.Api.Models;

public class AppUser
{
    public int Id { get; set; }

    [MaxLength(64)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
