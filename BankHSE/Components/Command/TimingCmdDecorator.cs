using System;
using System.Diagnostics;

namespace Components.Command
{
    /// <summary>
    /// Декоратор для измерения времени выполнения команд.
    /// Демонстрирует паттерн Decorator поверх Command.
    /// </summary>
    public class TimingCmdDecorator : ICommand
    {
        private readonly ICommand _inner;
        private readonly Action<string>? _log;

        public TimingCmdDecorator(ICommand inner, Action<string>? log = null)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _log = log;
        }

        public string Name => _inner.Name;

        public void Execute()
        {
            var sw = Stopwatch.StartNew();
            _inner.Execute();
            sw.Stop();

            _log?.Invoke($"[Timing] Command '{Name}' executed in {sw.ElapsedMilliseconds} ms.");
        }
    }
}