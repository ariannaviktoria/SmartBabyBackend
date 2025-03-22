using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.Entities;

public class User : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? LastLoginAt { get; set; }
    
    public bool IsActive { get; set; } = true;
    
    public virtual ICollection<Baby> Babies { get; set; } = new List<Baby>();
} 