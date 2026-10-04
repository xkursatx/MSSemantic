using System.Net.Http.Json;

namespace MSSemantic.Services;

public sealed class OllamaModelCatalog
{
    private readonly HttpClient _httpClient;
    private readonly string _defaultModel;

    public OllamaModelCatalog(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(
            configuration["OllamaSettings:Local:Endpoint"]
                ?? "http://localhost:11434");
        _defaultModel = configuration["OllamaSettings:Local:Model"]
            ?? OllamaModels.Qwen25_14B;
    }

    public async Task<IReadOnlyList<string>> GetModelsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<OllamaTagsResponse>(
            "/api/tags", cancellationToken);

        return result?.Models
            .Select(model => model.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];
    }

    public async Task<string> ResolveModelAsync(
        string? requestedModel,
        CancellationToken cancellationToken = default)
    {
        var models = await GetModelsAsync(cancellationToken);
        var selected = string.IsNullOrWhiteSpace(requestedModel)
            ? _defaultModel
            : requestedModel.Trim();

        if (!models.Contains(selected, StringComparer.OrdinalIgnoreCase))
            throw new InvalidOperationException(
                $"Ollama modeli bulunamadı veya yüklü değil: {selected}");

        return models.First(model =>
            string.Equals(model, selected, StringComparison.OrdinalIgnoreCase));
    }

    private sealed record OllamaTagsResponse(IReadOnlyList<OllamaTag> Models);
    private sealed record OllamaTag(string Name);
}
