using System.ComponentModel.DataAnnotations;

namespace Shared.Models;

public class KnownUser
{
    [Key]
    public Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
}
