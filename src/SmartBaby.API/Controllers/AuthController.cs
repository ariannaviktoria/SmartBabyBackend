using Microsoft.AspNetCore.Mvc;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;

namespace SmartBaby.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] UserDto userDto)
    {
        var user = new User
        {
            UserName = userDto.Email,
            Email = userDto.Email,
            FullName = userDto.FullName
        };

        var result = await _userService.CreateUserAsync(user, userDto.Password);
        if (!result)
            return BadRequest("Regisztráció sikertelen");

        return Ok("Regisztráció sikeres");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        var isValid = await _userService.ValidateUserAsync(loginDto.Email, loginDto.Password);
        if (!isValid)
            return Unauthorized("Érvénytelen bejelentkezési adatok");

        var user = await _userService.GetUserByEmailAsync(loginDto.Email);
        if (user == null)
            return NotFound("Felhasználó nem található");

        var token = await _userService.GenerateJwtTokenAsync(user);
        var tokenDto = new TokenDto
        {
            Token = token,
            Expiration = DateTime.Now.AddDays(7)
        };

        return Ok(tokenDto);
    }
} 