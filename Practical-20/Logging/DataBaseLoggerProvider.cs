using System.IO;
using Microsoft.AspNetCore.Http;
using Practical_20.Data;
using Practical_20.Models;

namespace Practical_20.Logging
{
    public class DatabaseLoggerProvider : ILoggerProvider
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _fallbackFilePath;

        public DatabaseLoggerProvider(IServiceProvider serviceProvider, string fallbackFilePath)
        {
            _serviceProvider = serviceProvider;
            _fallbackFilePath = fallbackFilePath;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new DatabaseLogger(_serviceProvider, categoryName, _fallbackFilePath);
        }

        public void Dispose() { }
    }

    public class DatabaseLogger : ILogger
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly string _categoryName;
        private readonly string _fallbackFilePath;

        public DatabaseLogger(IServiceProvider serviceProvider, string categoryName, string fallbackFilePath)
        {
            _serviceProvider = serviceProvider;
            _categoryName = categoryName;
            _fallbackFilePath = fallbackFilePath;
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
            if (formatter == null)
            {
                return;
            }

            using (var scope = _serviceProvider.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var httpContextAccessor = scope.ServiceProvider.GetService<IHttpContextAccessor>();
                var requestPath = httpContextAccessor?.HttpContext?.Request?.Path.Value ?? string.Empty;

                var logEntry = new LogEntry
                {
                    Timestamp = DateTime.UtcNow,
                    Level = logLevel.ToString(),
                    Message = formatter(state, exception),
                    Exception = exception?.ToString(),
                    Logger = _categoryName,
                    Url = requestPath
                };

                try
                {
                    dbContext.LogEntries.Add(logEntry);
                    dbContext.SaveChanges();
                }
                catch (Exception fallbackException)
                {
                    var fallbackMessage = $"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} [{logLevel}] {formatter(state, exception)} | DB logging failed: {fallbackException}{Environment.NewLine}";
                    Directory.CreateDirectory(Path.GetDirectoryName(_fallbackFilePath) ?? string.Empty);
                    File.AppendAllText(_fallbackFilePath, fallbackMessage);
                }
            }
        }
    }
}
