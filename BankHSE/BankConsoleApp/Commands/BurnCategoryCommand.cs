using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class BurnCategoryCommand : ICommand
    {
        private readonly CategoryService _categoryService;

        public BurnCategoryCommand(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public string Name => "burn-category";

        public void Execute()
        {
            Console.WriteLine("Категории:");
            foreach (var c in _categoryService.GetAll())
                Console.WriteLine($" - {c.Id} | {c.Name} | {c.FlowType}");

            Console.Write("ID категории для удаления: ");
            var id = Guid.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            _categoryService.DeleteCategory(id);
            Console.WriteLine("Категория удалена (если существовала).");
        }
    }
}