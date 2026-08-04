using Microsoft.SemanticKernel.ChatCompletion;

public interface IChatHistoryRepository
{
    Task EnsureSchemaAsync();
    Task<Guid> CreateSessionAsync();
    Task<ChatHistory> LoadHistoryAsync(Guid sessionId, string systemPrompt);
    Task SaveMessageAsync(Guid sessionId, AuthorRole role, string content);
}
