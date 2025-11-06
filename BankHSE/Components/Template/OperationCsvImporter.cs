using System.Globalization;
using Domain.Entity;

namespace Components.Template
{
    /// <summary>
    /// Импорт операций из CSV.
    /// Формат строк (после заголовка, если есть):
    /// Id;Type;AccountId;CategoryId;Amount;Date;Description
    /// Type: Income / Expense
    /// Некорректные строки пропускаются.
    /// </summary>
    public class OperationCsvImporter : ImportTemplate<Operation>
    {
        protected override IEnumerable<Operation> Parse(IEnumerable<string> lines)
        {
            var headerProcessed = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // Пропускаем заголовок
                if (!headerProcessed && line.StartsWith("Id;", StringComparison.OrdinalIgnoreCase))
                {
                    headerProcessed = true;
                    continue;
                }

                headerProcessed = true;

                var parts = line.Split(';');
                if (parts.Length < 6)
                    continue;

                // Id
                if (!Guid.TryParse(parts[0], out var id) || id == Guid.Empty)
                    continue;

                // Type
                var typeRaw = parts[1].Trim();
                if (!Enum.TryParse<MoneyFlowOption>(typeRaw, true, out var type) ||
                    type == MoneyFlowOption.Unknown)
                    continue;

                // AccountId
                if (!Guid.TryParse(parts[2], out var accountId) || accountId == Guid.Empty)
                    continue;

                // CategoryId
                if (!Guid.TryParse(parts[3], out var categoryId) || categoryId == Guid.Empty)
                    continue;

                // Amount
                if (!decimal.TryParse(parts[4], NumberStyles.Number, CultureInfo.InvariantCulture, out var amount) ||
                    amount <= 0)
                    continue;

                // Date
                var dateRaw = parts[5].Trim();
                if (!DateTime.TryParse(dateRaw,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal,
                        out var date))
                    continue;

                // Description (может отсутствовать)
                var description = parts.Length >= 7
                    ? (string.IsNullOrWhiteSpace(parts[6]) ? null : parts[6].Trim())
                    : null;

                Operation operation;
                try
                {
                    operation = new Operation(id, type, accountId, categoryId, amount, date, description);
                }
                catch
                {
                    // Если доменная модель не приняла данные — пропускаем строку.
                    continue;
                }

                yield return operation;
            }
        }
    }
}
