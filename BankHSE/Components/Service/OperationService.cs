using System;
using System.Collections.Generic;
using Components.Abstraction;
using Domain.Entity;
using Domain.Factory;

namespace Components.Service
{
    /// <summary>
    /// Фасад для создания и управления финансовыми операциями.
    /// </summary>
    public class OperationService
    {
        private readonly IRepo<Operation> _operations;
        private readonly IRepo<BankAccount> _accounts;
        private readonly IRepo<Category> _categories;
        private readonly IDomainFactory _factory;

        public OperationService(
            IRepo<Operation> operations,
            IRepo<BankAccount> accounts,
            IRepo<Category> categories,
            IDomainFactory factory)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public Operation CreateOperation(
            Guid accountId,
            Guid categoryId,
            MoneyFlowOption type,
            decimal amount,
            DateTime date,
            string? description)
        {
            var account = _accounts.GetById(accountId)
                          ?? throw new InvalidOperationException("Account not found.");
            var category = _categories.GetById(categoryId)
                           ?? throw new InvalidOperationException("Category not found.");

            var operation = _factory.CreateOperation(account, category, type, amount, date, description);

            _operations.Add(operation);
            account.ApplyOperation(operation);
            _accounts.Update(account);

            return operation;
        }

        public void DeleteOperation(Guid operationId)
        {
            _operations.Delete(operationId);
        }

        public Operation? GetById(Guid id) => _operations.GetById(id);

        public IReadOnlyCollection<Operation> GetAll() => _operations.GetAll();
    }
}
