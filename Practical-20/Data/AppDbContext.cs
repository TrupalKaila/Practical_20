using System.Text.Json;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Practical_20.Models;

namespace Practical_20.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor? _httpContextAccessor;

        public AppDbContext(DbContextOptions<AppDbContext> options, IHttpContextAccessor? httpContextAccessor = null) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<LogEntry> LogEntries { get; set; }
        public DbSet<AuditEntry> AuditEntries { get; set; }
        public DbSet<Student> Students { get; set; }

        public override int SaveChanges()
        {
            var pendingAudits = PrepareAuditEntries();
            var result = base.SaveChanges();
            PersistAuditEntriesAsync(pendingAudits, CancellationToken.None).GetAwaiter().GetResult();
            return result;
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var pendingAudits = PrepareAuditEntries();
            var result = await base.SaveChangesAsync(cancellationToken);
            await PersistAuditEntriesAsync(pendingAudits, cancellationToken);
            return result;
        }

        private List<PendingAuditEntry> PrepareAuditEntries()
        {
            ChangeTracker.DetectChanges();
            var pendingAudits = new List<PendingAuditEntry>();
            var entries = ChangeTracker.Entries()
                .Where(entry => entry.State != EntityState.Detached && entry.State != EntityState.Unchanged)
                .Where(entry => entry.Entity is not AuditEntry && entry.Entity is not LogEntry);

            foreach (var entry in entries)
            {
                var auditEntry = new AuditEntry
                {
                    UserId = _httpContextAccessor?.HttpContext?.User?.Identity?.Name ?? "System",
                    Action = entry.State.ToString(),
                    TableName = entry.Metadata.GetTableName() ?? entry.Entity.GetType().Name,
                    Timestamp = DateTime.UtcNow,
                    OldValues = string.Empty,
                    NewValues = string.Empty,
                    RecordId = string.Empty
                };

                var oldValues = new Dictionary<string, object?>();
                var newValues = new Dictionary<string, object?>();
                var hasTemporaryProperties = false;

                foreach (var property in entry.Properties)
                {
                    var propertyName = property.Metadata.Name;

                    if (property.IsTemporary)
                    {
                        hasTemporaryProperties = true;
                        continue;
                    }

                    if (property.Metadata.IsPrimaryKey())
                    {
                        auditEntry.RecordId = Convert.ToString(property.CurrentValue);
                        continue;
                    }

                    switch (entry.State)
                    {
                        case EntityState.Added:
                            newValues[propertyName] = property.CurrentValue;
                            break;
                        case EntityState.Deleted:
                            oldValues[propertyName] = property.OriginalValue;
                            break;
                        case EntityState.Modified:
                            if (property.IsModified)
                            {
                                oldValues[propertyName] = property.OriginalValue;
                                newValues[propertyName] = property.CurrentValue;
                            }
                            break;
                    }
                }

                auditEntry.OldValues = JsonSerializer.Serialize(oldValues);
                auditEntry.NewValues = JsonSerializer.Serialize(newValues);

                pendingAudits.Add(new PendingAuditEntry(entry, auditEntry, hasTemporaryProperties));
            }

            return pendingAudits;
        }

        private async Task PersistAuditEntriesAsync(IEnumerable<PendingAuditEntry> pendingAudits, CancellationToken cancellationToken)
        {
            var auditsToSave = pendingAudits.ToList();
            if (!auditsToSave.Any())
            {
                return;
            }

            foreach (var pending in auditsToSave.Where(pending => pending.HasTemporaryProperties))
            {
                var keyProperty = pending.Entry.Properties.FirstOrDefault(property => property.Metadata.IsPrimaryKey());
                if (keyProperty != null)
                {
                    pending.AuditEntry.RecordId = Convert.ToString(keyProperty.CurrentValue);
                }
            }

            AuditEntries.AddRange(auditsToSave.Select(pending => pending.AuditEntry));
            await base.SaveChangesAsync(cancellationToken);
        }

        private sealed record PendingAuditEntry(EntityEntry Entry, AuditEntry AuditEntry, bool HasTemporaryProperties);
    }
}
