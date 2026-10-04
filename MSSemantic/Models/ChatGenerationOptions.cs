namespace MSSemantic.Models;

public sealed record ChatGenerationOptions(
    double Temperature = 0.7,
    double TopP = 0.9,
    int TopK = 40,
    int MaxTokens = 512,
    string? Model = null)
{
    public ChatGenerationOptions Normalize() => new(
        Math.Clamp(Temperature, 0, 1.5),
        Math.Clamp(TopP, 0.1, 1),
        Math.Clamp(TopK, 1, 100),
        Math.Clamp(MaxTokens, 64, 2048),
        string.IsNullOrWhiteSpace(Model) ? null : Model.Trim());
}
