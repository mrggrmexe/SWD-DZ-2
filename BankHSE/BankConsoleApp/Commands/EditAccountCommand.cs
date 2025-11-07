using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class EditAccountCommand : ICommand
    {
        private readonly AccountService _accountService;

        public EditAccountCommand(AccountService accountService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
        }

        public string Name => "edit-account";

        public void Execute()
        {
            var id = ReadGuidWithAttempts("ID счёта: ");
            if (id == Guid.Empty)
            {
                Console.WriteLine("Счёт не изменён: не удалось прочитать корректный ID.");
                return;
            }

            var acc = _accountService.GetById(id);
            if (acc is null)
            {
                Console.WriteLine("Счёт не найден.");
                return;
            }

            Console.Write($"Новое имя (было '{acc.Name}', Enter — оставить): ");
            var newName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("Имя не изменено.");
                return;
            }

            try
            {
                _accountService.RenameAccount(id, newName.Trim());
                Console.WriteLine("Счёт обновлён.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении счёта: {ex.Message}");
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
