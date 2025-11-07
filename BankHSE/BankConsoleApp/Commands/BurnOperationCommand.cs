using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class BurnOperationCommand : ICommand
    {
        private readonly OperationService _operationService;

        public BurnOperationCommand(OperationService operationService)
        {
            _operationService = operationService ?? throw new ArgumentNullException(nameof(operationService));
        }

        public string Name => "burn-operation";

        public void Execute()
        {
            Console.Write("ID операции для удаления: ");

            var id = ReadGuidWithAttempts();
            if (id == Guid.Empty)
            {
                Console.WriteLine("Операция не удалена: не удалось прочитать корректный ID.");
                return;
            }

            try
            {
                _operationService.DeleteOperation(id);
                Console.WriteLine("Операция удалена (если существовала).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении операции: {ex.Message}");
            }
        }

        private static Guid ReadGuidWithAttempts(int maxAttempts = 3)
        {
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                var input = Console.ReadLine();

                if (Guid.TryParse(input, out var id) && id != Guid.Empty)
                    return id;

                Console.WriteLine("Некорректный формат GUID. Попробуйте ещё раз:");
            }

            return Guid.Empty;
        }
    }
}