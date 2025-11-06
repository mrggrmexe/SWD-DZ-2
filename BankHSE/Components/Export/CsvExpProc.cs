using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Domain.Entity;

namespace Components.Export
{
    /// <summary>
    /// Экспорт доменных сущностей в CSV-формате.
    /// Демонстрирует применение Visitor/Strategy для операции экспорта.
    /// </summary>
    public class CsvExpProc : IExpProc
    {
        public string FormatName => "CSV";
        public string FileExtension => ".csv";

        public string ExportAccounts(IEnumerable<BankAccount> accounts)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id;Name;Balance");

            foreach (var a in accounts ?? Array.Empty<BankAccount>())
            {
                sb.AppendLine($"{a.Id};{Escape(a.Name)};{a.Balance.ToString(CultureInfo.InvariantCulture)}");
            }

            return sb.ToString();
        }

        public string ExportCategories(IEnumerable<Category> categories)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id;Name;FlowType");

            foreach (var c in categories ?? Array.Empty<Category>())
            {
                sb.AppendLine($"{c.Id};{Escape(c.Name)};{c.FlowType}");
            }

            return sb.ToString();
        }

        public string ExportOperations(IEnumerable<Operation> operations)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id;Type;AccountId;CategoryId;Amount;Date;Description");

            foreach (var o in operations ?? Array.Empty<Operation>())
            {
                sb.AppendLine(string.Join(";",
                    o.Id,
                    o.Type,
                    o.BankAccountId,
                    o.CategoryId,
                    o.Amount.ToString(CultureInfo.InvariantCulture),
                    o.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    Escape(o.Description ?? string.Empty)));
            }

            return sb.ToString();
        }

        private static string Escape(string value)
        {
            if (value.Contains(';') || value.Contains('"'))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }

            return value;
        }
    }
}
