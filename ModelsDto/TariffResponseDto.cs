using AquaPass.Enums;

namespace AquaPass.ModelsDto
{
    public record TariffResponseDto(
        Guid Id,
        string Name,
        ServiceType ServiceType,
        string ServiceTypeName,
        decimal Price,
        Guid ZoneId,
        string DayType
    );
}