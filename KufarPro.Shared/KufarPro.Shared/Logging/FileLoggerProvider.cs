using Microsoft.Extensions.Logging;

namespace KufarPro.Shared.Logging
{
    public class FileLoggerProvider : ILoggerProvider
    {
        private readonly string _filePath;
        private readonly long _maxFileSize;

        public FileLoggerProvider(string filePath, long maxFileSize)
        {
            _filePath = filePath;
            _maxFileSize = maxFileSize;
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new FileLogger(_filePath, _maxFileSize);
        }

        public void Dispose() { }
    }
}
