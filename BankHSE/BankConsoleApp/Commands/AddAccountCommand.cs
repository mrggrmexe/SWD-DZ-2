using System.Globalization;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class AddAccountCommand : ICommand
    {
        private readonly AccountService _accountService;

        public AddAccountCommand(AccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public string Name => "add-account";

        public void Execute()
        {
            Console.Write("Название счёта: ");
            var name = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Счёт не создан: имя не может быть пустым.");
                return;
            }

            Console.Write("Начальный баланс (по умолчанию 0): ");
            var balanceInput = Console.ReadLine();

            decimal balance = 0m;
            if (!string.IsNullOrWhiteSpace(balanceInput))
            {
                // Пытаемся парсить в инвариантной и локальной культурe
                if (!decimal.TryParse(balanceInput, NumberStyles.Number, CultureInfo.InvariantCulture, out balance) &&
                    !decimal.TryParse(balanceInput, NumberStyles.Number, CultureInfo.CurrentCulture, out balance))
                {
                    Console.WriteLine("Некорректное значение баланса. Используется 0.");
                    balance = 0m;
                }
            }

            try
            {
                var account = _accountService.CreateAccount(name, balance);
                Console.WriteLine($"Счёт создан: {account.Id} | {account.Name} | Баланс: {account.Balance}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось создать счёт: {ex.Message}");
            }
        }
    }
}