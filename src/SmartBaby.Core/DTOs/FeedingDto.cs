using System;

namespace SmartBaby.Core.DTOs;

public class FeedingDto
{
    public int Id { get; set; }
    public DateTime Time { get; set; }
    public FeedingType Type { get; set; }
    public double? Amount { get; set; }
    public string? Notes { get; set; }
    public int BabyId { get; set; }
}

public enum FeedingType
{
    Breast,
    Bottle,
    Solid
} 