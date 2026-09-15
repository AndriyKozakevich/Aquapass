namespace AquaPass.Models
{
    public class Order
    {
        public Guid Id { get; set; }
        public string OrderNumber { get; set; } = Guid.NewGuid().ToString("N")[..8].ToUpper();
        public DateTime VisitDate { get; set; }
        public string CustomerFirstName { get; set; } = string.Empty;
        public string CustomerLastName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Created"; // Created, Paid, Cancelled
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }
}