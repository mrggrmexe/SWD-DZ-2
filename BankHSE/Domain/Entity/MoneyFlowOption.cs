namespace Domain.Entity
{
    /// <summary>
    /// Тип денежного потока: доход или расход.
    /// Значение Unknown зарезервировано для защиты от некорректной инициализации.
    /// </summary>
    public enum MoneyFlowOption
    {
        Unknown = 0,
        Income = 1,
        Expense = 2
    }
}