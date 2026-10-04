using MSSemantic.Models;

namespace MSSemantic.ViewModels;

public sealed class ChatViewModel
{
    public Guid SessionId { get; init; }
    public IReadOnlyList<string> Models { get; init; } = [];
    public string? SelectedModel { get; init; }
    public string Question { get; set; } = string.Empty;
    public string? Error { get; init; }
    public IReadOnlyList<MessageModel> Messages { get; init; } = [];
}
