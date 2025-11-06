using System;
using Components.Command;
using Components.Service;
using Domain.Entity;

namespace BankConsoleApp.Commands
{
    public class EditOperationCommand : ICommand
    {
        private readonly OperationService _operationService;
        private readonly AccountService _accountService;
        private readonly CategoryService _categoryService;

        public EditOperationCommand(
            OperationService operationService,
            AccountService accountService,
            CategoryService categoryService)
        {
            _operationService = operationService;
            _accountService = accountService;
            _categoryService = categoryService;
        }

        public string Name => "edit-operation";

        public void Execute()
        {
            Console.Write("ID операции для редактирования (фактически пересоздаём): ");
            var id = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            var op = _operationService.GetById(id);
            if (op is null)
            {
                Console.WriteLine("Операция не найдена.");
                return;
            }

            // Простая реализация: удалить и добавить новую.
            _operationService.DeleteOperation(id);
            Console.WriteLine("Старая операция удалена. Введите новые параметры.");

            new AddOperationCommand(_accountService, _categoryService, _operationService).Execute();
        }
    }
}