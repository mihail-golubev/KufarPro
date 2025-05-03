using AvKufarCarParser.Helpers;
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
            string logMessage = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} [{logLevel}] {formatter(state, exception)}";

            lock (_lock)
            {
                try
                {
                    FileInfo fileInfo = new FileInfo(_filePath);
                    if (fileInfo.Exists && fileInfo.Length > AppHelper.MaxFileSize)
                    {
                        File.WriteAllText(_filePath, string.Empty);
                    }

                    using (var stream = new FileStream(_filePath, FileMode.Append, FileAccess.Write, FileShare.Read))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.WriteLine(logMessage);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Logging error: {ex.Message}");
                }
            }
        }
    }
}
