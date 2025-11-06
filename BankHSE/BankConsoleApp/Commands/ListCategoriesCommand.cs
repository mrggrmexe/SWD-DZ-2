using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class ListCategoriesCommand : ICommand
    {
        private readonly CategoryService _categoryService;

        public ListCategoriesCommand(CategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public string Name => "list-categories";

        public void Execute()
        {
            var all = _categoryService.GetAll();
            if (all.Count == 0)
            {
                Console.WriteLine("Категории отсутствуют.");
                return;
            }

            foreach (var c in all)
            {
                Console.WriteLine($"{c.Id} | {c.Name} | {c.FlowType}");
            }
        }
    }
}