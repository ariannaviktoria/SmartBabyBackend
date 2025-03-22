using System;

namespace SmartBaby.Core.DTOs;

public class NoteDto
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int BabyId { get; set; }
} 