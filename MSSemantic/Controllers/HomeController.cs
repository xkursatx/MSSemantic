using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MSSemantic.Data;
using MSSemantic.Services;
using MSSemantic.ViewModels;

namespace MSSemantic.Controllers;

public sealed class HomeController : Controller
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IChatHistoryRepository _historyRepository;
    private readonly ChatService _chatService;

    public HomeController(ApplicationDbContext dbContext,
        IChatHistoryRepository historyRepository, ChatService chatService)
    {
        _dbContext = dbContext;
        _historyRepository = historyRepository;
        _chatService = chatService;
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
        CancellationToken cancellationToken)
    {
        try
        {
            await _chatService.AskAsync(sessionId, question, cancellationToken);
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

    private async Task<ChatViewModel> BuildViewModelAsync(Guid sessionId,
        string question = "", string? error = null)
    {
        var messages = await _dbContext.Messages
            .Where(x => x.SessionId == sessionId && x.Role != "system")
            .OrderBy(x => x.Id).AsNoTracking().ToListAsync();

        return new ChatViewModel { SessionId = sessionId, Question = question,
            Error = error, Messages = messages };
    }
}
