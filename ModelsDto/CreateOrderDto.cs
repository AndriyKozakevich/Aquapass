using System.ComponentModel.DataAnnotations;

namespace AquaPass.ModelsDto
{
    public class CreateOrderDto
    {
        [Required]
        public DateTime VisitDate { get; set; }

        [Required]
        public string CustomerFirstName { get; set; } = string.Empty;

        [Required]
        public string CustomerLastName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string CustomerPhone { get; set; } = string.Empty;

        // Список обраних товарів/послуг
        [Required]
        [MinLength(1, ErrorMessage = "Замовлення має містити хоча б один товар")]
        public List<OrderItemRequestDto> Items { get; set; } = new();
    }
}
