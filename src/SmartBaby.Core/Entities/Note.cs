using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.Entities;

public class Note
{
    public int Id { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    [MaxLength(500)]
    public string Content { get; set; }

    public string? Category { get; set; }

    [Required]
    public int BabyId { get; set; }

    public virtual Baby Baby { get; set; }
} 