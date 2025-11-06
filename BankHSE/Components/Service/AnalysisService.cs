using System;
using System.Collections.Generic;
using System.Linq;
using Components.Abstraction;
using Domain.Entity;

namespace Components.Service
{
    /// <summary>
    /// Сервис аналитики: баланс, группировки, применение процедур отчётности.
    /// </summary>
    public class AnalysisService
    {
        private readonly IRepo<Operation> _operations;
        private readonly IEnumerable<IReportProc> _reportProcs;

        public AnalysisService(IRepo<Operation> operations, IEnumerable<IReportProc> reportProcs)
        {
            _operations = operations ?? throw new ArgumentNullException(nameof(operations));
            _reportProcs = reportProcs ?? Array.Empty<IReportProc>();
        }

        public (decimal income, decimal expense, decimal diff) GetSummary(DateTime from, DateTime to)
        {
            var range = _operations.GetAll()
                .Where(o => o.Date >= from && o.Date <= to);

            var income = range.Where(o => o.Type == MonyFlowOption.Income)
                .Sum(o => o.Amount);

            var expense = range.Where(o => o.Type == MonyFlowOption.Expense)
                .Sum(o => o.Amount);

            return (income, expense, income - expense);
        }

        public IDictionary<Guid, decimal> GetSumByCategory()
        {
            return _operations
                .GetAll()
                .GroupBy(o => o.CategoryId)
                .ToDictionary(g => g.Key, g => g.Sum(o => o.Amount));
        }

        public IEnumerable<IReportProc> GetAvailableProcedures() => _reportProcs;

        public IEnumerable<Operation> ApplyProcedure(string name)
        {
            var proc = _reportProcs.FirstOrDefault(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));

            if (proc is null)
                throw new InvalidOperationException($"Report procedure '{name}' not found.");

            return proc.Process(_operations.GetAll());
        }
    }
}