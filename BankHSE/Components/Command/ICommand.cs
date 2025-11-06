namespace Components.Command
{
    /// <summary>
    /// Базовый контракт для пользовательских сценариев (паттерн Command).
    /// Консольный слой вызывает команды через этот интерфейс.
    /// </summary>
    public interface ICommand
    {
        /// <summary>
        /// Имя команды (для регистрации и вывода help).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Выполнение команды.
        /// Все зависимости должны быть внедрены через конструктор.
        /// </summary>
        void Execute();
    }
}