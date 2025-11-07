using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class BurnCategoryCommand : ICommand
    {
        private readonly CategoryService _categoryService;

        public BurnCategoryCommand(CategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        public string Name => "burn-category";

        public void Execute()
        {
            var categories = _categoryService.GetAll();
            if (categories.Count == 0)
            {
                Console.WriteLine("Категории отсутствуют. Удалять нечего.");
                return;
            }

            Console.WriteLine("Категории:");
            foreach (var c in categories)
                Console.WriteLine($" - {c.Id} | {c.Name} | {c.FlowType}");

            var id = ReadGuidWithAttempts("ID категории для удаления: ");
            if (id == Guid.Empty)
            {
                Console.WriteLine("Категория не удалена: не удалось прочитать корректный ID.");
                return;
            }

            try
            {
                _categoryService.DeleteCategory(id);
                Console.WriteLine("Категория удалена (если существовала).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении категории: {ex.Message}");
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