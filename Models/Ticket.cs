namespace AquaPass.Models
{
    public class Ticket
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public Guid EntranceTariffId { get; set; } // Вхідний тариф (EntranceTicketAdult або EntranceTicketChild)
        public decimal EntrancePrice { get; set; }
        public string TicketCode { get; set; } = string.Empty; // Унікальний код для персонального QR-коду гостя
        public string Status { get; set; } = "Active"; // "Active", "Used", "Cancelled"
        public Guid? SunbedId { get; set; }
        public decimal? SunbedPrice { get; set; }
        public decimal TotalPrice => EntrancePrice + (SunbedPrice ?? 0m);
        public Order Order { get; set; } = null!;
        public Tariff EntranceTariff { get; set; } = null!;
        public Sunbed? Sunbed { get; set; }
    }
}