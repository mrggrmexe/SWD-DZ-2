using System.Collections.Generic;
using System.Linq;
using Components.Abstraction;
using Domain.Entity;

namespace Components.Service.ReportProc
{
    /// <summary>
    /// Сортировка операций по описанию (или имени) по возрастанию.
    /// </summary>
    public class NameAscProc : IReportProc
    {
        public string Name => "NameAsc";

        public IEnumerable<Operation> Process(IEnumerable<Operation> operations)
        {
            return (operations ?? Enumerable.Empty<Operation>())
                .OrderBy(o => o.Description);
        }
    }
}