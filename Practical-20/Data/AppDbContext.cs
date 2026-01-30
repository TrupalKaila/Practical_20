using Microsoft.EntityFrameworkCore;
using Practical_20.Models;

namespace Practical_20.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<Student> Students { get; set; }
    }
}
