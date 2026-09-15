namespace AquaPass.ModelsDto
{
    public record StaffAuthResponseDto(
        string Token,
        string FullName,
        string Email,
        string Role,
        DateTime ExpiresAt
    );
}