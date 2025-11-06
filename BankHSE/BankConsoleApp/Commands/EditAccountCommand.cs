using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class EditAccountCommand : ICommand
    {
        private readonly AccountService _accountService;

        public EditAccountCommand(AccountService accountService)
        {
            _accountService = accountService;
        }

        public string Name => "edit-account";

        public void Execute()
        {
            Console.Write("ID счёта: ");
            var id = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            var acc = _accountService.GetById(id);
            if (acc is null)
            {
                Console.WriteLine("Счёт не найден.");
                return;
            }

            Console.Write($"Новое имя (было '{acc.Name}'): ");
            var newName = Console.ReadLine() ?? string.Empty;

            _accountService.RenameAccount(id, newName);
            Console.WriteLine("Счёт обновлён.");
        }
    }
}