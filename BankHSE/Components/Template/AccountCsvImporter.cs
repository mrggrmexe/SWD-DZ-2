using System.Globalization;
using Domain.Entity;

namespace Components.Template
{
    /// <summary>
    /// Импорт банковских счетов из CSV.
    /// Формат строк (после заголовка, если есть):
    /// Id;Name;Balance
    /// Некорректные строки пропускаются.
    /// </summary>
    public class AccountCsvImporter : ImportTemplate<BankAccount>
    {
        protected override IEnumerable<BankAccount> Parse(IEnumerable<string> lines)
        {
            var headerProcessed = false;

            foreach (var rawLine in lines)
            {
                var line = rawLine.Trim();
                if (string.IsNullOrEmpty(line))
                    continue;

                // Пропускаем первую строку-заголовок, если она начинается с Id;
                if (!headerProcessed && line.StartsWith("Id;", StringComparison.OrdinalIgnoreCase))
                {
                    headerProcessed = true;
                    continue;
                }

                headerProcessed = true;

                var parts = line.Split(';');
                if (parts.Length < 3)
                    continue;

                if (!Guid.TryParse(parts[0], out var id) || id == Guid.Empty)
                    continue;

                var name = parts[1].Trim();
                if (string.IsNullOrWhiteSpace(name))
                    continue;

                if (!decimal.TryParse(parts[2], NumberStyles.Number, CultureInfo.InvariantCulture, out var balance))
                    continue;

                // Создание сущности с защитой от выбросов
                BankAccount account;
                try
                {
                    account = new BankAccount(id, name, balance);
                }
                catch
                {
                    // Если данные некорректны для доменной модели — просто пропускаем строку.
                    continue;
                }

                yield return account;
            }
        }
    }
}
