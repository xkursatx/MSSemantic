namespace MSSemantic.Models
{
    public class SessionModel
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ICollection<MessageModel> Messages { get; set; } = new List<MessageModel>();
    }
}
