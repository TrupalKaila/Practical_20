using System.ComponentModel.DataAnnotations;

namespace Practical_20.Models
{
    public class AuditEntry
    {
        [Key]
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public string TableName { get; set; }
        public string RecordId { get; set; }
        public DateTime Timestamp { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
    }
}
