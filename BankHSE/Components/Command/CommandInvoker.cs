using System.Collections.Concurrent;

namespace Components.Command
{
    /// <summary>
    /// Потокобезопасный Invoker для паттерна Command.
    /// - Хранит зарегистрированные команды и позволяет выполнять их по имени.
    /// - Не зависит от конкретного UI.
    /// - Обрабатывает ошибки выполнения команд через callback'и, не падая всем приложением.
    /// </summary>
    public class CommandInvoker
    {
        private readonly ConcurrentDictionary<string, ICommand> _commands;
        private readonly Action<string>? _log;
        private readonly Action<Exception>? _errorHandler;

        /// <param name="log">Необязательный логгер для информационных сообщений.</param>
        /// <param name="errorHandler">Необязательный обработчик ошибок выполнения команд.</param>
        public CommandInvoker(Action<string>? log = null, Action<Exception>? errorHandler = null)
        {
            _commands = new ConcurrentDictionary<string, ICommand>(StringComparer.OrdinalIgnoreCase);
            _log = log;
            _errorHandler = errorHandler;
        }

        /// <summary>
        /// Регистрирует или перезаписывает команду по её имени.
        /// Имя не должно быть пустым.
        /// </summary>
        public void Register(ICommand command)
        {
            if (command is null)
                throw new ArgumentNullException(nameof(command));

            var name = command.Name;
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Command name must be non-empty.", nameof(command));

            _commands[name] = command;
        }

        public bool Contains(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            return _commands.ContainsKey(name);
        }

        /// <summary>
        /// Пытается выполнить команду по имени.
        /// Возвращает false, если команда не найдена или при выполнении произошла ошибка.
        /// Исключения не пробрасываются наружу, чтобы не "ронять" приложение.
        /// </summary>
        public bool TryExecute(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (!_commands.TryGetValue(name, out var command))
                return false;

            try
            {
                command.Execute();
                return true;
            }
            catch (Exception ex)
            {
                _errorHandler?.Invoke(ex);
                _log?.Invoke($"[CommandInvoker] Command '{command.Name}' failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Возвращает список имён зарегистрированных команд.
        /// </summary>
        public IEnumerable<string> GetCommandNames()
        {
            // ConcurrentDictionary позволяет безопасно перечислять ключи.
            return _commands.Keys;
        }
    }
}
