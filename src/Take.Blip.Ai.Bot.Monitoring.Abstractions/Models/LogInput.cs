namespace Take.Blip.Ai.Bot.Monitoring.Abstractions.Models
{
    public class LogInput
    {
        public string Title { get; set; }
        public string IdMessage { get; set; }
        public string From { get; set; }
        public string To { get; set; }
        public string? Operation { get; set; }
        public string? EventType { get; set; }
        public object? Data { get; set; }
    }
}
