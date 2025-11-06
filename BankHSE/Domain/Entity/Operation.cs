using System;

namespace Domain.Entity
{
    /// <summary>
    /// Финансовая операция: доход или расход.
    /// Связана со счётом и категорией.
    /// </summary>
    public class Operation
    {
        public Guid Id { get; }
        public MonyFlowOption Type { get; }
        public Guid BankAccountId { get; }
        public Guid CategoryId { get; }
        public decimal Amount { get; }
        public DateTime Date { get; }
        public string? Description { get; }

        #region Конструкторы

        public Operation(
            Guid id,
            MonyFlowOption type,
            Guid bankAccountId,
            Guid categoryId,
            decimal amount,
            DateTime date,
            string? description)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be non-empty.", nameof(id));
            if (bankAccountId == Guid.Empty)
                throw new ArgumentException("BankAccountId must be non-empty.", nameof(bankAccountId));
            if (categoryId == Guid.Empty)
                throw new ArgumentException("CategoryId must be non-empty.", nameof(categoryId));
            if (amount <= 0)
                throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive.");

            Id = id;
            Type = type;
            BankAccountId = bankAccountId;
            CategoryId = categoryId;
            Amount = amount;
            Date = date;
            Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        }

        internal Operation(
            MonyFlowOption type,
            Guid bankAccountId,
            Guid categoryId,
            decimal amount,
            DateTime date,
            string? description)
            : this(Guid.NewGuid(), type, bankAccountId, categoryId, amount, date, description)
        {
        }

        #endregion
    }
}