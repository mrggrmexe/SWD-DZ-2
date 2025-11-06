using System;
using System.Linq;
using Components.Command;

namespace BankConsoleApp.Commands
{
    public class HelpCommand : ICommand
    {
        private readonly CommandInvoker _invoker;

        public HelpCommand(CommandInvoker invoker)
        {
            _invoker = invoker ?? throw new ArgumentNullException(nameof(invoker));
        }

        public string Name => "help";

        public void Execute()
        {
            Console.WriteLine("Доступные команды:");
            foreach (var name in _invoker.GetCommandNames().OrderBy(x => x))
            {
                Console.WriteLine($" - {name}");
            }
        }
    }
}