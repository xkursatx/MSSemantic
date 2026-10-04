using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MSSemantic.Data;
using MSSemantic.Models;
using Microsoft.SemanticKernel.Connectors.Ollama;

namespace MSSemantic.Services;

public sealed class ChatService
{
    private readonly Kernel _kernel;
    private readonly IChatCompletionService _completionService;
    private readonly IChatHistoryRepository _historyRepository;
    private readonly OllamaModelCatalog _modelCatalog;
    private readonly IConfiguration _configuration;
    private readonly ILoggerFactory _loggerFactory;

    public ChatService(Kernel kernel, IChatCompletionService completionService,
        IChatHistoryRepository historyRepository, OllamaModelCatalog modelCatalog,
        IConfiguration configuration, ILoggerFactory loggerFactory)
    {
        _kernel = kernel;
        _completionService = completionService;
        _historyRepository = historyRepository;
        _modelCatalog = modelCatalog;
        _configuration = configuration;
        _loggerFactory = loggerFactory;
    }

    public async Task<string> AskAsync(Guid sessionId, string question,
        ChatGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("Geçerli bir oturum bulunamadı.");
        if (string.IsNullOrWhiteSpace(question)) throw new ArgumentException("Soru boş bırakılamaz.");

        var history = await _historyRepository.LoadHistoryAsync(sessionId, SystemPrompt);
        history.AddUserMessage(question);
        await _historyRepository.SaveMessageAsync(sessionId, AuthorRole.User, question);

        var completionService = await GetCompletionServiceAsync(options, cancellationToken);
        var response = await completionService.GetChatMessageContentAsync(history,
            kernel: _kernel,
            executionSettings: CreateExecutionSettings(options),
            cancellationToken: cancellationToken);

        var answer = response.Content ?? "Cevap üretilemedi.";
        await _historyRepository.SaveMessageAsync(sessionId, AuthorRole.Assistant, answer);
        return answer;
    }

    public async IAsyncEnumerable<string> StreamAsync(Guid sessionId, string question,
        ChatGenerationOptions? options = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation]
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("Geçerli bir oturum bulunamadı.");
        if (string.IsNullOrWhiteSpace(question)) throw new ArgumentException("Soru boş bırakılamaz.");

        var completionService = await GetCompletionServiceAsync(options, cancellationToken);
        var history = await _historyRepository.LoadHistoryAsync(sessionId, SystemPrompt);
        history.AddUserMessage(question);
        await _historyRepository.SaveMessageAsync(sessionId, AuthorRole.User, question);

        var answer = new System.Text.StringBuilder();
        await foreach (var content in completionService.GetStreamingChatMessageContentsAsync(
            history,
            kernel: _kernel,
            executionSettings: CreateExecutionSettings(options),
            cancellationToken: cancellationToken))
        {
            if (string.IsNullOrEmpty(content.Content)) continue;

            answer.Append(content.Content);
            yield return content.Content;
        }

        var completedAnswer = answer.ToString();
        if (string.IsNullOrWhiteSpace(completedAnswer))
            completedAnswer = "Cevap üretilemedi.";

        await _historyRepository.SaveMessageAsync(
            sessionId, AuthorRole.Assistant, completedAnswer);
    }

    private static OllamaPromptExecutionSettings CreateExecutionSettings(
        ChatGenerationOptions? options)
    {
        var normalized = (options ?? new ChatGenerationOptions()).Normalize();

        return new OllamaPromptExecutionSettings
        {
            Temperature = (float)normalized.Temperature,
            TopP = (float)normalized.TopP,
            TopK = normalized.TopK,
            NumPredict = normalized.MaxTokens,
            FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
        };
    }

    private async Task<IChatCompletionService> GetCompletionServiceAsync(
        ChatGenerationOptions? options,
        CancellationToken cancellationToken)
    {
        if (!string.Equals(_configuration["AiProvider"], "LocalOllama",
                StringComparison.OrdinalIgnoreCase))
            return _completionService;

        var model = await _modelCatalog.ResolveModelAsync(
            options?.Model, cancellationToken);
        var endpoint = _configuration["OllamaSettings:Local:Endpoint"]
            ?? "http://localhost:11434";

#pragma warning disable CS0618
        return new OllamaChatCompletionService(
            model, new Uri(endpoint), _loggerFactory);
#pragma warning restore CS0618
    }

    private const string SystemPrompt = """
        Sen UMAI Bilişim'in yapay zeka ürün asistanısın. Adın Cemile.
        Sadece UMAI Bilişim ürünleri hakkında bilgi ver.
        Emin olmadığın bilgileri uydurma; gerektiğinde ürün plugin'lerini kullan.
        Türkçe, kibar ve anlaşılır cevaplar ver.
        """;
}
