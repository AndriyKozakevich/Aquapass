using AquaPass.Enums;

namespace AquaPass.Models
{
    public class Tariff
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // "Цілий день (Вихідний)"
        public decimal Price { get; set; }
        public ServiceType ServiceType { get; set; }
        public DayType DayType { get; set; }
        public Guid ZoneId { get; set; }
        public Zone? Zone { get; set; }
    }
}