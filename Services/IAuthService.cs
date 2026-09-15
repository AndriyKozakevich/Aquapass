using AquaPass.ModelsDto;

namespace AquaPass.Services
{
    public interface IAuthService
    {
        Task<StaffAuthResponseDto?> LoginAsync(StaffLoginDto dto);
    }
}