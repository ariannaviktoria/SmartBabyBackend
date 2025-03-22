using System.ComponentModel.DataAnnotations;
using SmartBaby.Core.Entities;

namespace SmartBaby.Core.DTOs;

public class BabyDto
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

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string UserId { get; set; }
}

public class CreateBabyDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; }

    [Required]
    public DateTime DateOfBirth { get; set; }

    [Required]
    public Gender Gender { get; set; }
}

public class UpdateBabyDto
{
    [MaxLength(100)]
    public string? Name { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public Gender? Gender { get; set; }

    public string? LastMood { get; set; }
} 