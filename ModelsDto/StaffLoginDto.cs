using System.ComponentModel.DataAnnotations;

namespace AquaPass.ModelsDto
{
    public record StaffLoginDto(
        [Required, EmailAddress] string Email,
        [Required] string Password
    );
}