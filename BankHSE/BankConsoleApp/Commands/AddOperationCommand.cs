using System;
using System.Linq;
using Components.Command;
using Components.Service;
using Domain.Entity;

namespace BankConsoleApp.Commands
{
    public class AddOperationCommand : ICommand
    {
        private readonly AccountService _accountService;
        private readonly CategoryService _categoryService;
        private readonly OperationService _operationService;

        public AddOperationCommand(
            AccountService accountService,
            CategoryService categoryService,
            OperationService operationService)
        {
            _accountService = accountService;
            _categoryService = categoryService;
            _operationService = operationService;
        }

        public string Name => "add-operation";

        public void Execute()
        {
            Console.WriteLine("Доступные счета:");
            foreach (var a in _accountService.GetAll())
                Console.WriteLine($" - {a.Id} | {a.Name} | {a.Balance}");

            Console.Write("ID счёта: ");
            var accId = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.WriteLine("Доступные категории:");
            foreach (var c in _categoryService.GetAll())
                Console.WriteLine($" - {c.Id} | {c.Name} | {c.FlowType}");

            Console.Write("ID категории: ");
            var catId = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Тип (income/expense): ");
            var typeInput = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
            var type = typeInput switch
            {
                "income" => MonyFlowOption.Income,
                "expense" => MonyFlowOption.Expense,
                _ => throw new InvalidOperationException("Некорректный тип.")
            };

            Console.Write("Сумма: ");
            var amount = decimal.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Дата (yyyy-MM-dd), пусто = сегодня: ");
            var dateInput = Console.ReadLine();
            var date = string.IsNullOrWhiteSpace(dateInput)
                ? DateTime.Today
                : DateTime.Parse(dateInput!);

            Console.Write("Описание (опционально): ");
            var desc = Console.ReadLine();

            var op = _operationService.CreateOperation(accId, catId, type, amount, date, desc);
            Console.WriteLine($"Операция создана: {op.Id} ({op.Type}) {op.Amount} на {op.Date:yyyy-MM-dd}");
        }
    }
}
