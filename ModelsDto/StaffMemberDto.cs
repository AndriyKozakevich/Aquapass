namespace AquaPass.ModelsDto
{
    public record StaffMemberDto(
    Guid Id,
    string FullName,
    string Email,
    string Role,
    DateTime CreatedAt
);
}
