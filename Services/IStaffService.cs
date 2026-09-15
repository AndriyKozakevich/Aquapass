using AquaPass.ModelsDto;

public interface IStaffService
{
    Task<StaffAuthResponseDto?> LoginAsync(StaffLoginDto dto);
    Task<List<StaffMemberDto>> GetAllStaffAsync();
    Task<(bool Success, string Message, StaffMemberDto? Data)> CreateStaffMemberAsync(CreateStaffMemberDto dto);
    Task<bool> DeleteStaffMemberAsync(Guid id, Guid currentAdminId);
}
