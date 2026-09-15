namespace AquaPass.ModelsDto;

public class SunbedCreateDto
{
    public int Number { get; set; }
    public string Row { get; set; } = string.Empty;
    public Guid ZoneId { get; set; } 
    public decimal Price { get; set; }
    public bool IsAvailable { get; set; } = true;
}