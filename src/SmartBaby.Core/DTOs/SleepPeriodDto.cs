using System;

namespace SmartBaby.Core.DTOs;

public class SleepPeriodDto
{
    public int Id { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Quality { get; set; }
    public string? Notes { get; set; }
    public int BabyId { get; set; }
} 