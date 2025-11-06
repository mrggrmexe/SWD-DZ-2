namespace Domain.Entity
{
    /// <summary>
    /// Банковский счёт пользователя.
    /// Держит инварианты по имени и корректно обновляемому балансу.
    /// Потокобезопасен относительно своих операций.
    /// </summary>
    public class BankAccount
    {
        private readonly object _sync = new();

        public Guid Id { get; }
        public string Name { get; private set; }

        /// <summary>
        /// Текущий баланс счёта.
        /// Обновляется только через доменные методы.
        /// </summary>
        public decimal Balance { get; private set; }

        #region Конструкторы

        public BankAccount(Guid id, string name, decimal balance)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be non-empty.", nameof(id));

            Name = ValidateName(name);
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
            var valid = ValidateName(newName);

            lock (_sync)
            {
                Name = valid;
            }
        }

        /// <summary>
        /// Применяет операцию к счёту и обновляет баланс.
        /// Бросает исключение при несоответствии счёта или некорректном типе операции.
        /// </summary>
        public void ApplyOperation(Operation operation)
        {
            if (operation is null)
                throw new ArgumentNullException(nameof(operation));

            if (operation.BankAccountId != Id)
                throw new InvalidOperationException("Operation does not belong to this account.");

            lock (_sync)
            {
                switch (operation.Type)
                {
                    case MoneyFlowOption.Income:
                        Balance += operation.Amount;
                        break;

                    case MoneyFlowOption.Expense:
                        Balance -= operation.Amount;
                        break;

                    default:
                        throw new InvalidOperationException("Unknown money flow type.");
                }
            }
        }

        /// <summary>
        /// Полный пересчёт баланса по набору операций.
        /// Используется для восстановления согласованности данных.
        /// Операции по другим счетам игнорируются.
        /// </summary>
        public void RecalculateBalance(IEnumerable<Operation> operations)
        {
            if (operations is null)
                throw new ArgumentNullException(nameof(operations));

            decimal result = 0m;

            foreach (var op in operations)
            {
                if (op.BankAccountId != Id)
                    continue;

                switch (op.Type)
                {
                    case MoneyFlowOption.Income:
                        result += op.Amount;
                        break;

                    case MoneyFlowOption.Expense:
                        result -= op.Amount;
                        break;

                    default:
                        throw new InvalidOperationException("Unknown money flow type.");
                }
            }

            lock (_sync)
            {
                Balance = result;
            }
        }

        #endregion

        #region Приватные помощники

        private static string ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Account name must be non-empty.", nameof(name));

            return name.Trim();
        }

        #endregion
    }
}
