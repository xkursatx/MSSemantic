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

    public ChatService(Kernel kernel, IChatCompletionService completionService,
        IChatHistoryRepository historyRepository)
    {
        _kernel = kernel;
        _completionService = completionService;
        _historyRepository = historyRepository;
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

        var response = await _completionService.GetChatMessageContentAsync(history,
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

        var history = await _historyRepository.LoadHistoryAsync(sessionId, SystemPrompt);
        history.AddUserMessage(question);
        await _historyRepository.SaveMessageAsync(sessionId, AuthorRole.User, question);

        var answer = new System.Text.StringBuilder();
        await foreach (var content in _completionService.GetStreamingChatMessageContentsAsync(
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

    private const string SystemPrompt = """
        Sen UMAI Bilişim'in yapay zeka ürün asistanısın. Adın Cemile.
        Sadece UMAI Bilişim ürünleri hakkında bilgi ver.
        Emin olmadığın bilgileri uydurma; gerektiğinde ürün plugin'lerini kullan.
        Türkçe, kibar ve anlaşılır cevaplar ver.
        """;
}
