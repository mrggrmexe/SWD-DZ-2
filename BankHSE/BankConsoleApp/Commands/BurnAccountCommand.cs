using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class BurnAccountCommand : ICommand
    {
        private readonly AccountService _accountService;

        public BurnAccountCommand(AccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public string Name => "burn-account";

        public void Execute()
        {
            var accounts = _accountService.GetAll();
            if (accounts.Count == 0)
            {
                Console.WriteLine("Счета отсутствуют. Удалять нечего.");
                return;
            }

            Console.WriteLine("Счета:");
            foreach (var a in accounts)
                Console.WriteLine($" - {a.Id} | {a.Name} | Баланс: {a.Balance}");

            var id = ReadGuidWithAttempts("ID счёта для удаления: ");
            if (id == Guid.Empty)
            {
                Console.WriteLine("Счёт не удалён: не удалось прочитать корректный ID.");
                return;
            }

            try
            {
                _accountService.DeleteAccount(id);
                Console.WriteLine("Счёт удалён (если существовал).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении счёта: {ex.Message}");
            }
        }

        private static Guid ReadGuidWithAttempts(string prompt, int maxAttempts = 3)
        {
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (Guid.TryParse(input, out var id) && id != Guid.Empty)
                    return id;

                Console.WriteLine("Некорректный формат GUID. Попробуйте ещё раз.");
            }

            return Guid.Empty;
        }
    }
}