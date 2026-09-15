using AquaPass.ModelsDto;
using AquaPass.Services;
using Microsoft.AspNetCore.Mvc;

namespace AquaPass.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] StaffLoginDto dto)
    {
        var response = await _authService.LoginAsync(dto);

        if (response == null)
        {
            return Unauthorized(new { message = "Невірний email або пароль." });
        }

        return Ok(response);
    }

    
}