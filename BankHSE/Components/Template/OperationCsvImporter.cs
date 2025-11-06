using System;
using System.Collections.Generic;
using System.Globalization;
using Domain.Entity;

namespace Components.Template
{
    /// <summary>
    /// Импорт операций из CSV.
    /// Формат: Id;Type;AccountId;CategoryId;Amount;Date;Description
    /// Первая строка может быть заголовком.
    /// </summary>
    public class OperationCsvImporter : ImportTemplate<Operation>
    {
        protected override IEnumerable<Operation> Parse(IEnumerable<string> lines)
        {
            var isFirst = true;
            foreach (var line in lines)
            {
                if (isFirst && line.StartsWith("Id;", StringComparison.OrdinalIgnoreCase))
                {
                    isFirst = false;
                    continue;
                }

                isFirst = false;

                var parts = line.Split(';');
                if (parts.Length < 6) continue;

                var id = Guid.Parse(parts[0]);
                var type = Enum.Parse<MoneyFlowOption>(parts[1], ignoreCase: true);
                var accountId = Guid.Parse(parts[2]);
                var categoryId = Guid.Parse(parts[3]);
                var amount = decimal.Parse(parts[4], CultureInfo.InvariantCulture);
                var date = DateTime.Parse(parts[5], CultureInfo.InvariantCulture);
                var description = parts.Length >= 7 ? parts[6] : string.Empty;

                yield return new Operation(id, type, accountId, categoryId, amount, date, description);
            }
        }
    }
}