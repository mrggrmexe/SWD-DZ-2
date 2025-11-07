using System;
using Domain.Entity;

namespace Domain.Factory
{
    /// <summary>
    /// Абстракция фабрики для создания доменных объектов.
    /// Централизует валидацию и инварианты.
    /// </summary>
    public interface IDomainFactory
    {
        BankAccount CreateBankAccount(string name, decimal initialBalance = 0m);
        Category CreateCategory(string name, MoneyFlowOption flowType);

        Operation CreateOperation(
            BankAccount account,
            Category category,
            MoneyFlowOption type,
            decimal amount,
            DateTime date,
            string? description);

        // Методы для восстановления объектов из сохранённых данных (импорт).
        BankAccount RestoreBankAccount(Guid id, string name, decimal balance);
        Category RestoreCategory(Guid id, string name, MoneyFlowOption flowType);
        Operation RestoreOperation(
            Guid id,
            MoneyFlowOption type,
            Guid accountId,
            Guid categoryId,
            decimal amount,
            DateTime date,
            string? description);
    }
}