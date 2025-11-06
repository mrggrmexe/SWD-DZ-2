using System;
using System.Threading;
using Components.Command;
using Xunit;

namespace FinanceTracker.Tests
{
    public class TimingCmdDecoratorTests
    {
        private class TestCommand : ICommand
        {
            private readonly int _delayMs;

            public TestCommand(int delayMs)
            {
                _delayMs = delayMs;
            }

            public string Name => "test";

            public void Execute()
            {
                Thread.Sleep(_delayMs);
            }
        }

        [Fact]
        public void Execute_LogsTiming()
        {
            var cmd = new TestCommand(5);
            string? logMessage = null;

            var timed = new TimingCmdDecorator(cmd, msg => logMessage = msg);

            timed.Execute();

            Assert.NotNull(logMessage);
            Assert.Contains("Command 'test' executed", logMessage!, StringComparison.OrdinalIgnoreCase);
        }
    }
}