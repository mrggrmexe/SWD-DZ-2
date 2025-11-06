using System;
using System.Collections.Generic;
using System.Linq;

namespace Domain.Entity
{
    /// <summary>
    /// Банковский счёт пользователя.
    /// Держит инварианты по имени и балансу.
    /// </summary>
    public class BankAccount
    {
        public Guid Id { get; }
        public string Name { get; private set; }

        /// <summary>
        /// Текущий баланс счёта.
        /// В учебных целях поддерживается как поле,
        /// которое обновляется при применении операций.
        /// </summary>
        public decimal Balance { get; private set; }

        #region Конструкторы

        public BankAccount(Guid id, string name, decimal balance)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be non-empty.", nameof(id));

            SetName(name);
            Balance = balance;

            Id = id;
        }

        /// <summary>
        /// Упрощённый конструктор для фабрики.
        /// </summary>
        internal BankAccount(string name, decimal initialBalance = 0m)
            : this(Guid.NewGuid(), name, initialBalance)
        {
        }

        #endregion

        #region Публичные методы домена

        public void Rename(string newName)
        {
            SetName(newName);
        }

        /// <summary>
        /// Применяет операцию к счёту и обновляет баланс.
        /// </summary>
        public void ApplyOperation(Operation operation)
        {
            if (operation == null)
                throw new ArgumentNullException(nameof(operation));

            if (operation.BankAccountId != Id)
                throw new InvalidOperationException("Operation does not belong to this account.");

            if (operation.Type == MonyFlowOption.Income)
            {
                Balance += operation.Amount;
            }
            else if (operation.Type == MonyFlowOption.Expense)
            {
                Balance -= operation.Amount;
            }
            else
            {
                throw new InvalidOperationException("Unknown money flow type.");
            }
        }

        /// <summary>
        /// Полный пересчёт баланса по набору операций.
        /// </summary>
        public void RecalculateBalance(IEnumerable<Operation> operations)
        {
            if (operations == null)
                throw new ArgumentNullException(nameof(operations));

            var related = operations.Where(o => o.BankAccountId == Id);

            decimal result = 0m;

            foreach (var op in related)
            {
                if (op.Type == MonyFlowOption.Income)
                    result += op.Amount;
                else if (op.Type == MonyFlowOption.Expense)
                    result -= op.Amount;
            }

            Balance = result;
        }

        #endregion

        #region Приватные помощники

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Account name must be non-empty.", nameof(name));

            Name = name.Trim();
        }

        #endregion
    }
}
