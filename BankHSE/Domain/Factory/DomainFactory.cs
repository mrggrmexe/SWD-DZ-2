using System;
using Domain.Entity;

namespace Domain.Factory
{
    /// <summary>
    /// Реализация фабрики доменных объектов.
    /// Все доменные сущности создаются через этот класс,
    /// что упрощает поддержку инвариантов.
    /// </summary>
    public class DomainFactory : IDomainFactory
    {
        public BankAccount CreateBankAccount(string name, decimal initialBalance = 0m)
        {
            if (initialBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBalance),
                    "Initial balance cannot be negative.");

            return new BankAccount(name, initialBalance);
        }

        public Category CreateCategory(string name, MonyFlowOption flowType)
        {
            return new Category(name, flowType);
        }

        public Operation CreateOperation(
            BankAccount account,
            Category category,
            MonyFlowOption type,
            decimal amount,
            DateTime date,
            string? description)
        {
            if (account == null)
                throw new ArgumentNullException(nameof(account));
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            // Проверяем согласованность типа операции и категории
            if (category.FlowType != type)
                throw new InvalidOperationException(
                    $"Operation type '{type}' does not match category flow type '{category.FlowType}'.");

            var op = new Operation(
                type,
                account.Id,
                category.Id,
                amount,
                date,
                description);

            return op;
        }

        #region Restore (для импорта/персистентности)

        public BankAccount RestoreBankAccount(Guid id, string name, decimal balance)
        {
            return new BankAccount(id, name, balance);
        }

        public Category RestoreCategory(Guid id, string name, MonyFlowOption flowType)
        {
            return new Category(id, name, flowType);
        }

        public Operation RestoreOperation(
            Guid id,
            MonyFlowOption type,
            Guid accountId,
            Guid categoryId,
            decimal amount,
            DateTime date,
            string? description)
        {
            return new Operation(id, type, accountId, categoryId, amount, date, description);
        }

        #endregion
    }
}
