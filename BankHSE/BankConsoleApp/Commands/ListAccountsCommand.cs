using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class ListAccountsCommand : ICommand
    {
        private readonly AccountService _accountService;

        public ListAccountsCommand(AccountService accountService)
        {
            _accountService = accountService;
        }

        public string Name => "list-accounts";

        public void Execute()
        {
            var all = _accountService.GetAll();
            if (all.Count == 0)
            {
                Console.WriteLine("Счета отсутствуют.");
                return;
            }

            foreach (var a in all)
            {
                Console.WriteLine($"{a.Id} | {a.Name} | Баланс: {a.Balance}");
            }
        }
    }
}