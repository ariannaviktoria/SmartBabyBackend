using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.Entities;

public class DailyRoutine
{
    public int Id { get; set; }

    [Required]
    public int BabyId { get; set; }

    [Required]
    public DateTime Date { get; set; }

    public TimeSpan? WakeUpTime { get; set; }
    public TimeSpan? BedTime { get; set; }

    public int? NapCount { get; set; }
    public int? FeedingCount { get; set; }

    public string? Notes { get; set; }

    public bool IsDefault { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public virtual Baby Baby { get; set; }
} 