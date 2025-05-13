namespace DemoFYP.Models
{
    public class EmailLog
    {
        public int EmailId { get; set; }

        public string From { get; set; } = null!;

        public string To { get; set; } = null!;

        public string Subject { get; set; } = null!;

        public string Body { get; set; } = null!;

        public bool IsSent { get; set; }

        public DateTime SentAt { get; set; }

        public string? ErrorMessage {  get; set; }
    }
}
