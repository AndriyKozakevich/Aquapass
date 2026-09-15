namespace AquaPass.ModelsDto
{
    public record TicketResponseDto(
        Guid TicketId,
        string TicketCode,            // Код для генерації QR-коду
        string EntranceType,          // "Дорослий" або "Дитячий"
        decimal EntrancePrice,        // Вартість входу
        string Status,                // "Active" / "Used"

        // Додаткова послуга: Шезлонг
        bool HasSunbed,               // true, якщо гість орендував шезлонг
        string? SunbedDetails,        // Наприклад: "Ряд A, №12" (null якщо без шезлонга)
        decimal? SunbedPrice,         // Вартість шезлонга

        decimal TotalTicketPrice      // Загальна сума за цього гостя (Вхід + Шезлонг)
    );
}