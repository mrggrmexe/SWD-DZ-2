using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class BurnAccountCommand : ICommand
    {
        private readonly AccountService _accountService;

        public BurnAccountCommand(AccountService accountService)
        {
            _accountService = accountService;
        }

        public string Name => "burn-account";

        public void Execute()
        {
            Console.WriteLine("Счета:");
            foreach (var a in _accountService.GetAll())
                Console.WriteLine($" - {a.Id} | {a.Name} | {a.Balance}");

            Console.Write("ID счёта для удаления: ");
            var id = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            _accountService.DeleteAccount(id);
            Console.WriteLine("Счёт удалён (если существовал).");
        }
    }
}