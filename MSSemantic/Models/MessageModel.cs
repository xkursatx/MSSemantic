namespace MSSemantic.Models
{
    public class MessageModel
    {
        public long Id { get; set; }
        public Guid SessionId { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public SessionModel? Session { get; set; }
    }
}
