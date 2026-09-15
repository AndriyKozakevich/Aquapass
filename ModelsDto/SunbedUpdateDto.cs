namespace AquaPass.ModelsDto
{
    public class SunbedUpdateDto
    {
        public int Number { get; set; }
        public string Row { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public bool IsAvailable { get; set; } = true;
    }
}