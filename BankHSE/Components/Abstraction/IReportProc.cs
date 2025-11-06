using System.Collections.Generic;
using Domain.Entity;

namespace Components.Abstraction
{
    /// <summary>
    /// Поведенческая стратегия обработки набора операций для отчетов/аналитики.
    /// Применяется в анализе и экспортных сценариях.
    /// </summary>
    public interface IReportProc
    {
        /// <summary>
        /// Человекочитаемое имя процедуры (для выбора в UI или логах).
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Преобразует последовательность операций (сортировка, фильтрация, агрегация).
        /// </summary>
        IEnumerable<Operation> Process(IEnumerable<Operation> operations);
    }
}