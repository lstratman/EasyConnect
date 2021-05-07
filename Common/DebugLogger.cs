using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;

namespace EasyConnect.Common
{
    public class DebugLoggerProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new DebugLogger();
        }

        public void Dispose()
        {
        }
    }

    public class DebugLogger : ILogger
    {
        public IDisposable BeginScope<TState>(TState state)
        {
            return new DebugLoggerScope();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Debug.WriteLine($"{logLevel.ToString("G")} - {formatter(state, exception)}");
        }
    }

    public class DebugLoggerScope : IDisposable
    {
        public void Dispose()
        {
        }
    }
}
