namespace AquaPass.ModelsDto
{
    public class SunbedResponseDto
    {
        public Guid Id { get; set; }
        public int Number { get; set; }
        public string Row { get; set; } = string.Empty;
        public Guid ZoneId { get; set; }
        public string Description { get; set; } = string.Empty;
        public bool IsAvailable { get; set; }
    }
}