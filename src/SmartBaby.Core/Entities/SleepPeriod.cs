using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.Entities;

public class SleepPeriod
{
    public int Id { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Quality { get; set; }

    public string? Notes { get; set; }

    [Required]
    public int BabyId { get; set; }

    public virtual Baby Baby { get; set; }
} 