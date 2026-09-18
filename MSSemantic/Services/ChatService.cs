using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using MSSemantic.Data;

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
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty) throw new ArgumentException("Geçerli bir oturum bulunamadı.");
        if (string.IsNullOrWhiteSpace(question)) throw new ArgumentException("Soru boş bırakılamaz.");

        var history = await _historyRepository.LoadHistoryAsync(sessionId, SystemPrompt);
        history.AddUserMessage(question);
        await _historyRepository.SaveMessageAsync(sessionId, AuthorRole.User, question);

        var response = await _completionService.GetChatMessageContentAsync(history,
            kernel: _kernel,
            executionSettings: new PromptExecutionSettings
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            }, cancellationToken: cancellationToken);

        var answer = response.Content ?? "Cevap üretilemedi.";
        await _historyRepository.SaveMessageAsync(sessionId, AuthorRole.Assistant, answer);
        return answer;
    }

    private const string SystemPrompt = """
        Sen UMAI Bilişim'in yapay zeka ürün asistanısın. Adın Cemile.
        Sadece UMAI Bilişim ürünleri hakkında bilgi ver.
        Emin olmadığın bilgileri uydurma; gerektiğinde ürün plugin'lerini kullan.
        Türkçe, kibar ve anlaşılır cevaplar ver.
        """;
}
