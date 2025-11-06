using System;
using Components.Command;

namespace BankConsoleApp.Commands
{
    public class ExitCommand : ICommand
    {
        private readonly dynamic _state; // маленький трюк для передачи ссылки из Program

        public ExitCommand(object state)
        {
            _state = state ?? throw new ArgumentNullException(nameof(state));
        }

        public string Name => "exit";

        public void Execute()
        {
            _state.IsRequested = true;
            Console.WriteLine("Завершение работы приложения...");
        }
    }
}