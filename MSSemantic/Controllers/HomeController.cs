using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSSemantic.Data;
using MSSemantic.Models;
using MSSemantic.Services;
using MSSemantic.ViewModels;

namespace MSSemantic.Controllers;

public sealed class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IChatHistoryRepository _historyRepository;
    private readonly ChatService _chatService;
    private readonly OllamaModelCatalog _modelCatalog;

    public HomeController(ApplicationDbContext dbContext,
        IChatHistoryRepository historyRepository, ChatService chatService,
        OllamaModelCatalog modelCatalog)
    {
        _dbContext = dbContext;
        _historyRepository = historyRepository;
        _chatService = chatService;
        _modelCatalog = modelCatalog;
    }

    [HttpGet]
    public async Task<IActionResult> Index(Guid? sessionId)
    {
        var id = sessionId.GetValueOrDefault();
        if (id == Guid.Empty || !await _dbContext.Sessions.AnyAsync(x => x.Id == id))
            id = await _historyRepository.CreateSessionAsync();

        return View(await BuildViewModelAsync(id));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ask(Guid sessionId, string question,
        double temperature = 0.7, double topP = 0.9, int topK = 40,
        int maxTokens = 512, string? model = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _chatService.AskAsync(sessionId, question,
                new ChatGenerationOptions(temperature, topP, topK, maxTokens, model),
                cancellationToken);
            return RedirectToAction(nameof(Index), new { sessionId });
        }
        catch (ArgumentException exception)
        {
            return View(nameof(Index), await BuildViewModelAsync(sessionId, question, exception.Message));
        }
        catch (HttpRequestException)
        {
            return View(nameof(Index), await BuildViewModelAsync(sessionId, question,
                "AI servisine ulaşılamadı. Ollama çalışıyor mu?"));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task Stream(Guid sessionId, string question,
        double temperature = 0.7, double topP = 0.9, int topK = 40,
        int maxTokens = 512, string? model = null,
        CancellationToken cancellationToken = default)
    {
        if (sessionId == Guid.Empty || string.IsNullOrWhiteSpace(question))
        {
            Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        Response.ContentType = "text/plain; charset=utf-8";
        Response.Headers.CacheControl = "no-cache";

        await foreach (var chunk in _chatService.StreamAsync(
            sessionId, question,
            new ChatGenerationOptions(temperature, topP, topK, maxTokens, model),
            cancellationToken))
        {
            await Response.WriteAsync(chunk, cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }

    private async Task<ChatViewModel> BuildViewModelAsync(Guid sessionId,
        string question = "", string? error = null)
    {
        var messages = await _dbContext.Messages
            .Where(x => x.SessionId == sessionId && x.Role != "system")
            .OrderBy(x => x.Id).AsNoTracking().ToListAsync();

        IReadOnlyList<string> models = [];
        try
        {
            models = await _modelCatalog.GetModelsAsync();
        }
        catch (HttpRequestException)
        {
            // Ollama kapalıysa chat ekranı yine açılabilsin.
        }

        return new ChatViewModel { SessionId = sessionId, Question = question,
            Error = error, Messages = messages, Models = models,
            SelectedModel = models.FirstOrDefault() };
    }
}
