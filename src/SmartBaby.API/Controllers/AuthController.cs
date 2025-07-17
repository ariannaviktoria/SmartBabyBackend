using Microsoft.AspNetCore.Mvc;
using SmartBaby.Core.DTOs;
using SmartBaby.Core.Entities;
using SmartBaby.Core.Interfaces;

namespace SmartBaby.API.Controllers;

/// <summary>
/// Authentication controller for user registration and login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="userDto">User registration information</param>
    /// <returns>Registration result</returns>
    /// <response code="200">Registration successful</response>
    /// <response code="400">Registration failed</response>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
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

    /// <summary>
    /// Authenticate user and get JWT token
    /// </summary>
    /// <param name="loginDto">User login credentials</param>
    /// <returns>JWT token for authenticated user</returns>
    /// <response code="200">Login successful, returns JWT token</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="404">User not found</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
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