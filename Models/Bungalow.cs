using System.ComponentModel.DataAnnotations.Schema;

namespace AquaPass.Models
{
    public class Bungalow
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int Number { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}