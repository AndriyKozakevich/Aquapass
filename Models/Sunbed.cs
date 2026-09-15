using System.ComponentModel.DataAnnotations.Schema;

namespace AquaPass.Models
{
    public class Sunbed
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int Number { get; set; }
        public string Row { get; set; } = string.Empty;
        public Guid ZoneId { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}