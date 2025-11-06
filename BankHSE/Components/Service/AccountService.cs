using System;
using System.Collections.Generic;
using Components.Abstraction;
using Domain.Entity;
using Domain.Factory;

namespace Components.Service
{
    /// <summary>
    /// Фасад для операций над банковскими счетами.
    /// </summary>
    public class AccountService
    {
        private readonly IRepo<BankAccount> _accounts;
        private readonly IDomainFactory _factory;

        public AccountService(IRepo<BankAccount> accounts, IDomainFactory factory)
        {
            _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public BankAccount CreateAccount(string name, decimal initialBalance = 0m)
        {
            var account = _factory.CreateBankAccount(name, initialBalance);
            _accounts.Add(account);
            return account;
        }

        public void RenameAccount(Guid accountId, string newName)
        {
            var account = _accounts.GetById(accountId)
                          ?? throw new InvalidOperationException("Account not found.");

            account.Rename(newName);
            _accounts.Update(account);
        }

        public void DeleteAccount(Guid accountId)
        {
            _accounts.Delete(accountId);
        }

        public BankAccount? GetById(Guid accountId) => _accounts.GetById(accountId);

        public IReadOnlyCollection<BankAccount> GetAll() => _accounts.GetAll();
    }
}