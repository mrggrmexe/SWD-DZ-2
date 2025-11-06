using System;
using Components.Command;
using Components.Abstraction;
using Domain.Entity;

namespace BankConsoleApp.Commands
{
    /// <summary>
    /// Пересчитывает баланс всех счетов по операциям.
    /// Демонстрирует сценарий управления данными.
    /// </summary>
    public class UpdateBalanceCommand : ICommand
    {
        private readonly IRepo<BankAccount> _accountRepo;
        private readonly IRepo<Operation> _operationRepo;

        public UpdateBalanceCommand(IRepo<BankAccount> accountRepo, IRepo<Operation> operationRepo)
        {
            _accountRepo = accountRepo;
            _operationRepo = operationRepo;
        }

        public string Name => "update-balance";

        public void Execute()
        {
            var operations = _operationRepo.GetAll();

            foreach (var acc in _accountRepo.GetAll())
            {
                acc.RecalculateBalance(operations);
                _accountRepo.Update(acc);
            }

            Console.WriteLine("Баланс всех счетов пересчитан по текущим операциям.");
        }
    }
}