using System;
using System.Collections.Generic;

namespace Components.Command
{
    /// <summary>
    /// Invoker для паттерна Command.
    /// Хранит зарегистрированные команды и позволяет выполнять их по имени.
    /// Не зависит от конкретного UI.
    /// </summary>
    public class CommandInvoker
    {
        private readonly Dictionary<string, ICommand> _commands =
            new(StringComparer.OrdinalIgnoreCase);

        public void Register(ICommand command)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            _commands[command.Name] = command;
        }

        public bool Contains(string name) => _commands.ContainsKey(name);

        public bool TryExecute(string name)
        {
            if (!_commands.TryGetValue(name, out var command))
                return false;

            command.Execute();
            return true;
        }

        public IEnumerable<string> GetCommandNames() => _commands.Keys;
    }
}