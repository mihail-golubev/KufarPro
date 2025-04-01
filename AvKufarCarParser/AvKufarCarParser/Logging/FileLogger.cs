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
            if (!File.Exists(_filePath))
            {
                File.Create(_filePath).Dispose();
            }

            FileInfo fileInfo = new FileInfo(_filePath);

            if (fileInfo.Length > Util.MaxFileSize)
            {
                File.WriteAllText(_filePath, string.Empty);
            }

            string logMessage = $"{DateTime.Now:dd/MM/yyyy HH:mm:ss} [{logLevel}] {formatter(state, exception)}{Environment.NewLine}";

            File.AppendAllText(_filePath, logMessage, System.Text.Encoding.UTF8);
        }
    }
}
