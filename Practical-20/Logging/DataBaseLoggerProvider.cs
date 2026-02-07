using Practical_20.Data;
using Practical_20.Models;

namespace Practical_20.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;

        public DatabaseLoggerProvider(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(_serviceProvider, categoryName);
        }

        public void Dispose() { }
    }

    public class DatabaseLogger : ILogger
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _categoryName;

        public DatabaseLogger(IServiceProvider serviceProvider, string categoryName)
        {
            _serviceProvider = serviceProvider;
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = logLevel.ToString(),
                    Message = formatter(state, exception),
                    Exception = exception?.ToString(),
                    Logger = _categoryName,
                    Url = "" 
                };

                dbContext.LogEntries.Add(logEntry);
                dbContext.SaveChanges();
            }
        }
    }
}
