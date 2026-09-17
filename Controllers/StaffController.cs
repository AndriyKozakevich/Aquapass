using System.Security.Claims;
using AquaPass.ModelsDto;
using AquaPass.Services;
using AquaPass.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AquaPass.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class StaffController : ControllerBase
{
    private readonly IStaffService _staffService;

    public StaffController(IStaffService staffService)
    {
        _staffService = staffService;
    }

    /// <summary>
    /// Авторизація співробітника (Касира або Адміністратора)
    /// </summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] StaffLoginDto dto)
    {
        var response = await _staffService.LoginAsync(dto);
        if (response == null)
        {
            return Unauthorized(new { message = "Невірний email або пароль." });
        }

        return Ok(response);
    }

    /// <summary>
    /// Отримання списку всього персоналу (Тільки Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var staff = await _staffService.GetAllStaffAsync();
        return Ok(staff);
    }

    /// <summary>
    /// Створення нового співробітника (Тільки Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateStaffMemberDto dto)
    {
        var (success, message, data) = await _staffService.CreateStaffMemberAsync(dto);
        if (!success)
        {
            return BadRequest(new { message });
        }

        return CreatedAtAction(nameof(GetAll), new { id = data!.Id }, data);
    }

    /// <summary>
    /// Видалення облікового запису співробітника (Тільки Admin)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var invalid = this.ValidateId(id, nameof(id));
        if (invalid != null) return invalid;
        var currentUserIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _ = Guid.TryParse(currentUserIdStr, out var currentAdminId);

        var deleted = await _staffService.DeleteStaffMemberAsync(id, currentAdminId);
        if (!deleted)
        {
            return BadRequest(new { message = "Неможливо видалити співробітника (його не знайдено або ви намагаєтесь видалити себе)." });
        }

        return Ok(new { message = "Співробітника успішно видалено." });
    }
}