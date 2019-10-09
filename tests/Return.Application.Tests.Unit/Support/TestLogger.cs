// ******************************************************************************
//  © 2019 Sebastiaan Dammann | damsteen.nl
// 
//  File:           : TestLogger.cs
//  Project         : Return.Application.Tests.Unit
// ******************************************************************************

namespace Return.Application.Tests.Unit.Support {
    using System;
    using Microsoft.Extensions.Logging;
    using NUnit.Framework;

    internal sealed class TestLogger<T> : ILogger<T> {
        private struct FakeDisposable : IDisposable {
            public void Dispose() {
            }
        }

        public IDisposable BeginScope<TState>(TState state) => new FakeDisposable();

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            TestContext.WriteLine(
                $"[{typeof(T).Name}]: {logLevel} {eventId} {state}: {formatter(state, exception)} \r\n{exception}"
            );
        }
    }

    internal static class TestLogger {
        public static ILogger<T> For<T>() => new TestLogger<T>();
    }
}
