using System;
using Components.Abstraction;
using Components.Export;
using Domain.Entity;

namespace Components.Service
{
    /// <summary>
    /// Фасад для сценариев экспорта данных.
    /// Использует IExpProc (Visitor/Strategy) и репозитории.
    /// </summary>
    public class ExportService
    {
        private readonly IRepo<BankAccount> _accounts;
        private readonly IRepo<Category> _categories;
        private readonly IRepo<Operation> _operations;
        private readonly IExpProc _expProc;

        public ExportService(
            IRepo<BankAccount> accounts,
            IRepo<Category> categories,
            IRepo<Operation> operations,
            IExpProc expProc)
        {
            _accounts = accounts ?? throw new ArgumentNullException(nameof(accounts));
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _expProc = expProc ?? throw new ArgumentNullException(nameof(expProc));
        }

        public string ExportAccounts() =>
            _expProc.ExportAccounts(_accounts.GetAll());

        public string ExportCategories() =>
            _expProc.ExportCategories(_categories.GetAll());

        public string ExportOperations() =>
            _expProc.ExportOperations(_operations.GetAll());

        public string FormatName => _expProc.FormatName;
        public string FileExtension => _expProc.FileExtension;
    }
}