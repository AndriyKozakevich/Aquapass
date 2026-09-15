using System.ComponentModel.DataAnnotations;

namespace AquaPass.ModelsDto
{
    public record CreateStaffMemberDto(
    [Required] string FullName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(6)] string Password,
    [Required] string Role // "Cashier" або "Admin"
);
}
