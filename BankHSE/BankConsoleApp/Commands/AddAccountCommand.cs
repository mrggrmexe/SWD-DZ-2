using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class AddAccountCommand : ICommand
    {
        private readonly AccountService _accountService;

        public AddAccountCommand(AccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public string Name => "add-account";

        public void Execute()
        {
            Console.Write("Название счёта: ");
            var name = Console.ReadLine() ?? string.Empty;

            Console.Write("Начальный баланс (опционально, по умолчанию 0): ");
            var balanceInput = Console.ReadLine();
            decimal.TryParse(balanceInput, out var balance);

            var account = _accountService.CreateAccount(name, balance);
            Console.WriteLine($"Счёт создан: {account.Id} | {account.Name} | Баланс: {account.Balance}");
        }
    }
}