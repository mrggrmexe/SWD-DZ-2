using Components.Command;
using Components.Service;
using Domain.Entity;

namespace BankConsoleApp.Commands
{
    public class AddCategoryCommand : ICommand
    {
        private readonly CategoryService _categoryService;

        public AddCategoryCommand(CategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        public string Name => "add-category";

        public void Execute()
        {
            Console.Write("Название категории: ");
            var name = (Console.ReadLine() ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(name))
            {
                Console.WriteLine("Категория не создана: имя не может быть пустым.");
                return;
            }

            var type = ReadFlowType();
            if (type == MoneyFlowOption.Unknown)
            {
                Console.WriteLine("Категория не создана из-за некорректного типа.");
                return;
            }

            try
            {
                var cat = _categoryService.CreateCategory(name, type);
                Console.WriteLine($"Категория создана: {cat.Id} | {cat.Name} | {cat.FlowType}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось создать категорию: {ex.Message}");
            }
        }

        private static MoneyFlowOption ReadFlowType()
        {
            const int maxAttempts = 3;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write("Тип (income/expense): ");
                var input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

                if (input is "income" or "i" or "+")
                    return MoneyFlowOption.Income;

                if (input is "expense" or "e" or "-")
                    return MoneyFlowOption.Expense;

                Console.WriteLine("Некорректный тип. Ожидается: income или expense.");
            }

            return MoneyFlowOption.Unknown;
        }
    }
}
