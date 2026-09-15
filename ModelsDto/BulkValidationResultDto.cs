namespace AquaPass.ModelsDto
{
    public record BulkValidationResultDto(
        bool Success,
        string Message,
        int ValidatedCount
    );
}