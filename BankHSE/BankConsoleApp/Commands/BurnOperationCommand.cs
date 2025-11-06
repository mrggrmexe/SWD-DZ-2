using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class BurnOperationCommand : ICommand
    {
        private readonly OperationService _operationService;

        public BurnOperationCommand(OperationService operationService)
        {
            _operationService = operationService;
        }

        public string Name => "burn-operation";

        public void Execute()
        {
            Console.Write("ID операции для удаления: ");
            var id = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            _operationService.DeleteOperation(id);
            Console.WriteLine("Операция удалена (если существовала).");
        }
    }
}