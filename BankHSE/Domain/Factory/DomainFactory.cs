using Domain.Entity;

namespace Domain.Factory
{
    /// <summary>
    /// Реализация фабрики доменных объектов.
    /// Централизует создание и восстановление сущностей с валидацией.
    /// </summary>
    public class DomainFactory : IDomainFactory
    {
        public BankAccount CreateBankAccount(string name, decimal initialBalance = 0m)
        {
            if (initialBalance < 0)
                throw new ArgumentOutOfRangeException(nameof(initialBalance),
                    "Initial balance cannot be negative.");

            // Валидация имени выполняется внутри BankAccount.
            return new BankAccount(name, initialBalance);
        }

        public Category CreateCategory(string name, MoneyFlowOption flowType)
        {
            if (flowType == MoneyFlowOption.Unknown)
                throw new ArgumentException("Category flow type must be Income or Expense.", nameof(flowType));

            return new Category(name, flowType);
        }

        public Operation CreateOperation(
            BankAccount account,
            Category category,
            MoneyFlowOption type,
            decimal amount,
            DateTime date,
            string? description)
        {
            if (account is null)
                throw new ArgumentNullException(nameof(account));
            if (category is null)
                throw new ArgumentNullException(nameof(category));

            if (type == MoneyFlowOption.Unknown)
                throw new ArgumentException("Operation type must be Income or Expense.", nameof(type));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "Operation amount must be positive.");

            if (date == default)
                throw new ArgumentException("Operation date must be a valid non-default value.", nameof(date));

            // Проверяем согласованность типа операции и категории
            if (category.FlowType != type)
                throw new InvalidOperationException(
                    $"Operation type '{type}' does not match category flow type '{category.FlowType}'.");

            // Конструктор Operation дополнительно проверит инварианты.
            return new Operation(
                type,
                account.Id,
                category.Id,
                amount,
                date,
                description);
        }

        #region Restore (для импорта/персистентности)

        public BankAccount RestoreBankAccount(Guid id, string name, decimal balance)
        {
            // Здесь сохраняем жёсткие проверки конструктора:
            // если данные битые (пустой Id, пустое имя), он бросит исключение.
            return new BankAccount(id, name, balance);
        }

        public Category RestoreCategory(Guid id, string name, MoneyFlowOption flowType)
        {
            if (flowType == MoneyFlowOption.Unknown)
                throw new ArgumentException("Category flow type must be Income or Expense.", nameof(flowType));

            return new Category(id, name, flowType);
        }

        public Operation RestoreOperation(
            Guid id,
            MoneyFlowOption type,
            Guid accountId,
            Guid categoryId,
            decimal amount,
            DateTime date,
            string? description)
        {
            if (type == MoneyFlowOption.Unknown)
                throw new ArgumentException("Operation type must be Income or Expense.", nameof(type));

            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount),
                    "Operation amount must be positive.");

            if (id == Guid.Empty)
                throw new ArgumentException("Id must be non-empty.", nameof(id));

            if (accountId == Guid.Empty)
                throw new ArgumentException("AccountId must be non-empty.", nameof(accountId));

            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId must be non-empty.", nameof(categoryId));

            if (date == default)
                throw new ArgumentException("Operation date must be a valid non-default value.", nameof(date));

            return new Operation(id, type, accountId, categoryId, amount, date, description);
        }

        #endregion
    }
}
