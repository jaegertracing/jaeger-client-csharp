using System;
using Microsoft.Extensions.Logging;

namespace Jaeger.Core.Tests.Util
{
    public abstract class MockLogger : ILogger
    {
        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            => Log(logLevel, formatter(state, exception));
        public abstract void Log(LogLevel logLevel, string message);
        public virtual bool IsEnabled(LogLevel logLevel) => true;
        public abstract IDisposable BeginScope<TState>(TState state);
    }
}
