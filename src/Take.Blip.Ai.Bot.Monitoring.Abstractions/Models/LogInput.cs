namespace Take.Blip.Ai.Bot.Monitoring.Abstractions.Models
{
    public class LogInput
    {
        public required string Title { get; set; }
        public required string IdMessage { get; set; }
        public required string From { get; set; }
        public string? OriginalFrom { get; set; }
        public required string To { get; set; }
        public string? OriginalTo { get; set; }
        public int? FlowVersion { get; set; }
        public string? Channel { get; set; }
        public string? Operation { get; set; }
        public string? EventType { get; set; }
        public object? Data { get; set; }
    }
}
