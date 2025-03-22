using System;

namespace SmartBaby.Core.DTOs;

public class CryingDto
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Intensity { get; set; }
    public string? AIResponse { get; set; }
    public string? Notes { get; set; }
    public int BabyId { get; set; }
} 