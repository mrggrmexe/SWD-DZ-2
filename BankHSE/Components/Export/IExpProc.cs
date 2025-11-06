using System.Collections.Generic;
using Domain.Entity;

namespace Components.Export
{
    /// <summary>
    /// Посетитель/процессор экспорта доменных сущностей.
    /// Позволяет реализовать разные форматы выгрузки.
    /// </summary>
    public interface IExpProc
    {
        string FormatName { get; }
        string FileExtension { get; }

        string ExportAccounts(IEnumerable<BankAccount> accounts);
        string ExportCategories(IEnumerable<Category> categories);
        string ExportOperations(IEnumerable<Operation> operations);
    }
}