namespace AquaPass.Services
{
    public interface ISunbedHoldService
    {
        Task<bool> HoldSunbedAsync(Guid sunbedId, DateTime visitDate, string holdToken, TimeSpan duration);
        Task ReleaseHoldAsync(Guid sunbedId, DateTime visitDate, string holdToken);
        Task<HashSet<Guid>> GetHeldSunbedIdsAsync(DateTime visitDate, string? currentHoldToken = null);
    }
}
