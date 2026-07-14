using System;
using System.Collections.Concurrent;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace DataTechnologies.Utils
{
    public sealed class MemoryLoggerProvider : ILoggerProvider
    {
        private static readonly ConcurrentQueue<string> _logs = new();
        private static readonly HttpClient HttpClientInstance = new();

        public static IReadOnlyCollection<string> GetLogs() => _logs.ToArray();

        public static void Clear() => _logs.Clear();

        public static void AddLog(string dbPrefix, string logLevel, string category, string message)
        {
            message = Sanitize(message);
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
            var logEntry = $"[{timestamp}] [{dbPrefix}] [{logLevel}] [{category}] {message}";
            _logs.Enqueue(logEntry);
            _ = SendToSolarWindsAsync(logEntry);
        }

        public static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = Regex.Replace(input, @"\b(Host|Port|Server|User Id|Password)\s*=\s*[^;]*", "$1=[redacted]", RegexOptions.IgnoreCase);
            input = Regex.Replace(input, @"(?:\[redacted\]|[a-zA-Z0-9\-\.]+):(\d+)\b", "[redacted]:[redacted]");
            input = Regex.Replace(input, @"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", "[redacted]");

            return input;
        }

        private static async Task SendToSolarWindsAsync(string log)
        {
            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Post, "https://logs.collector.eu-01.cloud.solarwinds.com/v1/logs");
                var papertrailToken = Environment.GetEnvironmentVariable("PAPERTRAIL");
                if (string.IsNullOrWhiteSpace(papertrailToken))
                    return;

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", papertrailToken);

                var content = new ByteArrayContent(Encoding.UTF8.GetBytes(log));
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                request.Content = content;

                using var response = await HttpClientInstance.SendAsync(request);
            }
            catch
            {
            }
        }

        public ILogger CreateLogger(string categoryName) => new MemoryLogger(categoryName);

        public void Dispose() { }

        private sealed class MemoryLogger : ILogger
        {
            private readonly string _categoryName;

            public MemoryLogger(string categoryName)
            {
                _categoryName = categoryName;
            }

            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) => true;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (formatter == null) return;
                var message = Sanitize(formatter(state, exception));
                var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture);
                var dbPrefix = _categoryName.StartsWith("Npgsql", StringComparison.OrdinalIgnoreCase) ? "PostgreSQL" : "Other";
                var logEntry = $"[{timestamp}] [{dbPrefix}] [{logLevel}] [{_categoryName}] {message}";
                if (exception != null)
                {
                    logEntry += Environment.NewLine + Sanitize(exception.ToString());
                }
                _logs.Enqueue(logEntry);
                _ = SendToSolarWindsAsync(logEntry);
            }
        }
    }
}
