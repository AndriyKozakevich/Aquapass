using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AquaPass.Models;
using AquaPass.ModelsDto;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AquaPass.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _config;

    public AuthService(AppDbContext context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }

    public async Task<StaffAuthResponseDto?> LoginAsync(StaffLoginDto dto)
    {
        var email = dto.Email.Trim().ToLower();
        var user = await _context.Staffs.FirstOrDefaultAsync(u => u.Email == email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
        {
            return null;
        }

        return GenerateJwtToken(user);
    }

    
    private StaffAuthResponseDto GenerateJwtToken(Staff staff)
    {
        var jwtKey = _config["Jwt:Key"] ?? "SUPER_SECRET_AQUAPASS_KEY_123456789_LONG_ENOUGH";
        var issuer = _config["Jwt:Issuer"] ?? "AquaPassServer";
        var audience = _config["Jwt:Audience"] ?? "AquaPassClient";

        var expires = DateTime.UtcNow.AddDays(7);
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
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return new StaffAuthResponseDto(
            Token: tokenString,
            FullName: staff.FullName,
            Email: staff.Email,
            Role: staff.Role,
            ExpiresAt: expires
        );
    }
}