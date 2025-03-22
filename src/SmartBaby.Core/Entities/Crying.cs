using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.Entities;

public class Crying
{
    public int Id { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public string? Intensity { get; set; }

    public string? AIResponse { get; set; }

    public string? Notes { get; set; }

    [Required]
    public int BabyId { get; set; }

    public virtual Baby Baby { get; set; }
} 