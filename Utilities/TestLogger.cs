using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace RestfulApiDevTests.Utilities
{/// <summary>
 /// Enhanced logging utility for test execution
 /// </summary>
    public class TestLogger
    {
        private readonly ITestOutputHelper _output;
        private readonly List<LogEntry> _logEntries = new();

        public TestLogger(ITestOutputHelper output)
        {
            _output = output;
        }

        public void LogInfo(string message)
        {
            var entry = new LogEntry("INFO", message);
            _logEntries.Add(entry);
            _output.WriteLine($"[{entry.Timestamp:HH:mm:ss.fff}] [INFO] {message}");
        }

        public void LogError(string message, Exception? exception = null)
        {
            var entry = new LogEntry("ERROR", message, exception);
            _logEntries.Add(entry);
            _output.WriteLine($"[{entry.Timestamp:HH:mm:ss.fff}] [ERROR] {message}");
            if (exception != null)
            {
                _output.WriteLine($"  Exception: {exception.Message}");
                _output.WriteLine($"  StackTrace: {exception.StackTrace}");
            }
        }

        public void LogWarning(string message)
        {
            var entry = new LogEntry("WARNING", message);
            _logEntries.Add(entry);
            _output.WriteLine($"[{entry.Timestamp:HH:mm:ss.fff}] [WARN] {message}");
        }

        public void LogDebug(string message)
        {
            var entry = new LogEntry("DEBUG", message);
            _logEntries.Add(entry);
            _output.WriteLine($"[{entry.Timestamp:HH:mm:ss.fff}] [DEBUG] {message}");
        }

        public void LogApiRequest(string method, string endpoint, object? body = null)
        {
            var message = $"API Request: {method} {endpoint}";
            if (body != null)
            {
                message += $"\n  Body: {JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true })}";
            }
            LogInfo(message);
        }

        public void LogApiResponse(int statusCode, string statusText, object? body = null, long? durationMs = null)
        {
            var message = $"API Response: {statusCode} {statusText}";
            if (durationMs.HasValue)
            {
                message += $" ({durationMs}ms)";
            }
            if (body != null)
            {
                message += $"\n  Body: {JsonSerializer.Serialize(body, new JsonSerializerOptions { WriteIndented = true })}";
            }
            LogInfo(message);
        }

        public void LogTestStart(string testName)
        {
            _output.WriteLine(new string('=', 80));
            LogInfo($"TEST START: {testName}");
            _output.WriteLine(new string('=', 80));
        }

        public void LogTestEnd(string testName, bool passed)
        {
            _output.WriteLine(new string('=', 80));
            if (passed)
            {
                LogInfo($"TEST PASSED: {testName} ✓");
            }
            else
            {
                LogError($"TEST FAILED: {testName} ✗");
            }
            _output.WriteLine(new string('=', 80));
        }

        public string GetLogSummary()
        {
            var summary = $"Total Logs: {_logEntries.Count}\n";
            summary += $"  INFO: {_logEntries.Count(e => e.Level == "INFO")}\n";
            summary += $"  WARN: {_logEntries.Count(e => e.Level == "WARNING")}\n";
            summary += $"  ERROR: {_logEntries.Count(e => e.Level == "ERROR")}\n";
            summary += $"  DEBUG: {_logEntries.Count(e => e.Level == "DEBUG")}";
            return summary;
        }

        private class LogEntry
        {
            public DateTime Timestamp { get; }
            public string Level { get; }
            public string Message { get; }
            public Exception? Exception { get; }

            public LogEntry(string level, string message, Exception? exception = null)
            {
                Timestamp = DateTime.UtcNow;
                Level = level;
                Message = message;
                Exception = exception;
            }
        }
    }
}