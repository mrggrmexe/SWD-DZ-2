using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class EditCategoryCommand : ICommand
    {
        private readonly CategoryService _categoryService;

        public EditCategoryCommand(CategoryService categoryService)
        {
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        public string Name => "edit-category";

        public void Execute()
        {
            var id = ReadGuidWithAttempts("ID категории: ");
            if (id == Guid.Empty)
            {
                Console.WriteLine("Категория не изменена: не удалось прочитать корректный ID.");
                return;
            }

            var cat = _categoryService.GetById(id);
            if (cat is null)
            {
                Console.WriteLine("Категория не найдена.");
                return;
            }

            Console.Write($"Новое имя (было '{cat.Name}', Enter — оставить): ");
            var newName = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(newName))
            {
                Console.WriteLine("Имя не изменено.");
                return;
            }

            try
            {
                _categoryService.RenameCategory(id, newName.Trim());
                Console.WriteLine("Категория обновлена.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при обновлении категории: {ex.Message}");
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
