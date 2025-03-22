using System.ComponentModel.DataAnnotations;

namespace SmartBaby.Core.DTOs;

public class UserDto
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }
}

public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}

public class TokenDto
{
    public string Token { get; set; }
    public DateTime Expiration { get; set; }
} 