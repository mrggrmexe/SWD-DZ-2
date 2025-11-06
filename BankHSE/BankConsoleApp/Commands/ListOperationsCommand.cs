using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class ListOperationsCommand : ICommand
    {
        private readonly OperationService _operationService;

        public ListOperationsCommand(OperationService operationService)
        {
            _operationService = operationService;
        }

        public string Name => "list-operations";

        public void Execute()
        {
            var all = _operationService.GetAll();
            if (all.Count == 0)
            {
                Console.WriteLine("Операции отсутствуют.");
                return;
            }

            foreach (var o in all)
            {
                Console.WriteLine($"{o.Id} | {o.Type} | Acc:{o.BankAccountId} | Cat:{o.CategoryId} | {o.Amount} | {o.Date:yyyy-MM-dd} | {o.Description}");
            }
        }
    }
}