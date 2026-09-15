using System.ComponentModel.DataAnnotations;

namespace AquaPass.ModelsDto
{
    public class OrderItemRequestDto
    {
        [Required]
        public Guid TariffId { get; set; }

        [Range(1, 50, ErrorMessage = "Кількість має бути від 1 до 50")]
        public int Quantity { get; set; }
        public Guid? SunbedId { get; set; }
    }
}