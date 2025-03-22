using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.Entities;

public class Feeding
{
    public int Id { get; set; }

    [Required]
    public DateTime Time { get; set; }

    [Required]
    public FeedingType Type { get; set; }

    public double? Amount { get; set; }

    public string? Notes { get; set; }

    [Required]
    public int BabyId { get; set; }

    public virtual Baby Baby { get; set; }
}

public enum FeedingType
{
    Breast,
    Bottle,
    Solid
} 