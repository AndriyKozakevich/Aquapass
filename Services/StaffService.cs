using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AquaPass.Models;
using AquaPass.ModelsDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AquaPass.Services;

public class StaffService : IStaffService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public StaffService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<StaffAuthResponseDto?> LoginAsync(StaffLoginDto dto)
    {
        var cleanEmail = dto.Email.Trim().ToLower();
        var staff = await _context.Staffs.FirstOrDefaultAsync(u => u.Email == cleanEmail);

        if (staff == null || !BCrypt.Net.BCrypt.Verify(dto.Password, staff.PasswordHash))
        {
            return null;
        }

        var expiresAt = DateTime.UtcNow.AddHours(12);
        var token = GenerateJwtToken(staff, expiresAt);

        return new StaffAuthResponseDto(
            Token: token,
            FullName: staff.FullName,
            Email: staff.Email,
            Role: staff.Role,
            ExpiresAt: expiresAt
        );
    }

    public async Task<List<StaffMemberDto>> GetAllStaffAsync()
    {
        return await _context.Staffs
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .Select(u => new StaffMemberDto(
                u.Id,
                u.FullName,
                u.Email,
                u.Role,
                u.CreatedAt
            ))
            .ToListAsync();
    }

    public async Task<(bool Success, string Message, StaffMemberDto? Data)> CreateStaffMemberAsync(CreateStaffMemberDto dto)
    {
        var cleanEmail = dto.Email.Trim().ToLower();

        if (await _context.Staffs.AnyAsync(u => u.Email == cleanEmail))
        {
            return (false, "Співробітник із такою електронною поштою вже існує в системі.", null);
        }

        var role = dto.Role is "Admin" or "Cashier" ? dto.Role : "Cashier";

        var newMember = new Staff
        {
            Id = Guid.NewGuid(),
            FullName = dto.FullName.Trim(),
            Email = cleanEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = role,
            CreatedAt = DateTime.UtcNow
        };

        _context.Staffs.Add(newMember);
        await _context.SaveChangesAsync();

        var resultDto = new StaffMemberDto(
            newMember.Id,
            newMember.FullName,
            newMember.Email,
            newMember.Role,
            newMember.CreatedAt
        );

        return (true, "Співробітника успішно додано.", resultDto);
    }

    public async Task<bool> DeleteStaffMemberAsync(Guid id, Guid currentAdminId)
    {
        // Адміністратор не може видалити сам себе
        if (id == currentAdminId)
        {
            return false;
        }

        var staff = await _context.Staffs.FindAsync(id);
        if (staff == null)
        {
            return false;
        }

        _context.Staffs.Remove(staff);
        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateJwtToken(Staff staff, DateTime expiresAt)
    {
        var jwtKey = _config["Jwt:Key"] ?? "AQUAPASS_SUPER_SECRET_JWT_KEY_MIN_32_CHARS_LONG_2026";
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, staff.Id.ToString()),
            new(ClaimTypes.Name, staff.FullName),
            new(ClaimTypes.Email, staff.Email),
            new(ClaimTypes.Role, staff.Role)
        };

        var token = new JwtSecurityToken(
            issuer: _config["Jwt:Issuer"] ?? "AquaPassServer",
            audience: _config["Jwt:Audience"] ?? "AquaPassClient",
            claims: claims,
            expires: expiresAt,
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}