using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel.ChatCompletion;
using MSSemantic.Models;

namespace MSSemantic.Data
{
    public class PostgresChatHistoryRepository : IChatHistoryRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PostgresChatHistoryRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task EnsureSchemaAsync()
        {
            // EF Core migration ile yönetildiği için burada bir şey yapmaya gerek yok
            await Task.CompletedTask;
        }

        public async Task<Guid> CreateSessionAsync()
        {
            var session = new SessionModel
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Sessions.Add(session);
            await _dbContext.SaveChangesAsync();

            return session.Id;
        }

        public async Task<ChatHistory> LoadHistoryAsync(Guid sessionId, string systemPrompt)
        {
            var history = new ChatHistory();
            history.AddSystemMessage(systemPrompt);

            var messages = await _dbContext.Messages
                .Where(m => m.SessionId == sessionId)
                .OrderBy(m => m.Id)
                .ToListAsync();

            foreach (var message in messages)
            {
                // system mesajı zaten yukarıda ekledik, DB'den tekrar eklemiyoruz
                if (message.Role == "system") continue;

                if (message.Role == "user")
                    history.AddUserMessage(message.Content);
                else if (message.Role == "assistant")
                    history.AddAssistantMessage(message.Content);
            }

            return history;
        }

        public async Task SaveMessageAsync(Guid sessionId, AuthorRole role, string content)
        {
            // boş asistan mesajını DB'ye yazmıyoruz (örn. tool-call-only turn)
            if (string.IsNullOrWhiteSpace(content)) return;

            var message = new MessageModel
            {
                SessionId = sessionId,
                Role = role.Label.ToLowerInvariant(),
                Content = content,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.Messages.Add(message);
            await _dbContext.SaveChangesAsync();
        }
    }
}
