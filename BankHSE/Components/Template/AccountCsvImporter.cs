using System;
using System.Collections.Generic;
using System.Globalization;
using Domain.Entity;

namespace Components.Template
{
    /// <summary>
    /// Импорт банковских счетов из CSV.
    /// Формат: Id;Name;Balance
    /// Первая строка может быть заголовком.
    /// </summary>
    public class AccountCsvImporter : ImportTemplate<BankAccount>
    {
        protected override IEnumerable<BankAccount> Parse(IEnumerable<string> lines)
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
                if (parts.Length < 3) continue;

                var id = Guid.Parse(parts[0]);
                var name = parts[1];
                var balance = decimal.Parse(parts[2], CultureInfo.InvariantCulture);

                yield return new BankAccount(id, name, balance);
            }
        }
    }
}