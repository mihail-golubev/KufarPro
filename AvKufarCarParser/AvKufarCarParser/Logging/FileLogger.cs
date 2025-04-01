using Microsoft.Extensions.Logging;

namespace AvKufarCarParser.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private static readonly object _lock = new object();

        public FileLogger(string filePath) => _filePath = filePath;

        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null) return;

            var message = $"[{DateTime.Now:HH:mm:ss dd/MM/yyyy}] [{logLevel}] {formatter(state, exception)}{Environment.NewLine}";

            lock (_lock)
            {
                File.AppendAllText(_filePath, message);
            }
        }
    }
}
