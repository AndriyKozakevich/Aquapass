namespace AquaPass.ModelsDto
{
    public class HoldSunbedRequest
    {
        public DateTime VisitDate { get; set; }
        public string HoldToken { get; set; } = string.Empty;
    }
}