namespace AquaPass.Models
{
    public class Zone
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; // "VIP Zone", "Standard Zone"
        public ICollection<Sunbed> Sunbeds { get; set; } = new List<Sunbed>();
        public ICollection<Tariff> Tariffs { get; set; } = new List<Tariff>();
    }
}