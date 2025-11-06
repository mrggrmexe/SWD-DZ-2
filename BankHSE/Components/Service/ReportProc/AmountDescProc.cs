using System.Collections.Generic;
using System.Linq;
using Components.Abstraction;
using Domain.Entity;

namespace Components.Service.ReportProc
{
    /// <summary>
    /// Сортировка операций по сумме по убыванию.
    /// </summary>
    public class AmountDescProc : IReportProc
    {
        public string Name => "AmountDesc";

        public IEnumerable<Operation> Process(IEnumerable<Operation> operations)
        {
            return (operations ?? Enumerable.Empty<Operation>())
                .OrderByDescending(o => o.Amount);
        }
    }
}