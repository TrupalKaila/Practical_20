using System.ComponentModel.DataAnnotations;

namespace Practical_20.Models
{
    public class LogEntry
    {
        [Key]
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; }
        public string Message { get; set; }
        public string? Exception { get; set; }
        public string Logger { get; set; }
        public string Url { get; set; }
    }
}
