using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class EditCategoryCommand : ICommand
    {
        private readonly CategoryService _categoryService;

        public EditCategoryCommand(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public string Name => "edit-category";

        public void Execute()
        {
            Console.Write("ID категории: ");
            var id = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            var cat = _categoryService.GetById(id);
            if (cat is null)
            {
                Console.WriteLine("Категория не найдена.");
                return;
            }

            Console.Write($"Новое имя (было '{cat.Name}'): ");
            var newName = Console.ReadLine() ?? string.Empty;

            _categoryService.RenameCategory(id, newName);
            Console.WriteLine("Категория обновлена.");
        }
    }
}