using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.Entities;

public class Baby
{
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }

    public string? LastMood { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [Required]
    public string UserId { get; set; }

    public virtual User User { get; set; }

    public virtual ICollection<SleepPeriod> SleepPeriods { get; set; } = new List<SleepPeriod>();
    public virtual ICollection<Feeding> Feedings { get; set; } = new List<Feeding>();
    public virtual ICollection<Crying> Cryings { get; set; } = new List<Crying>();
    public virtual ICollection<Note> Notes { get; set; } = new List<Note>();
    public virtual ICollection<DailyRoutine> DailyRoutines { get; set; } = new List<DailyRoutine>();
}

public enum Gender
{
    Male,
    Female,
    Other
} 