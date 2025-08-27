using Microsoft.Extensions.Logging;

namespace KufarPro.Shared.Logging
{
    public class FileLogger : ILogger
    {
        private readonly string _filePath;
        private readonly long _maxFileSize;
        private static readonly object _lock = new object();


        public FileLogger(string filePath, long maxFileSize)
        {
            _filePath = filePath;
            _maxFileSize = maxFileSize;
        }

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
                    if (fileInfo.Exists && fileInfo.Length > _maxFileSize)
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
