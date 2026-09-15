using System.ComponentModel.DataAnnotations;
using AquaPass.Enums;

namespace AquaPass.ModelsDto
{
    public class TariffUpdateDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [Range(0, 1000000)]
        public decimal Price { get; set; }

        [Required]
        public ServiceType ServiceType { get; set; }

        [Required]
        public DayType DayType { get; set; }

        [Required]
        public Guid ZoneId { get; set; }
    }
}
