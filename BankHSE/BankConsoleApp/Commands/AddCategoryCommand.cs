using System;
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
            var name = Console.ReadLine() ?? string.Empty;

            Console.Write("Тип (income/expense): ");
            var typeInput = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

            var type = typeInput switch
            {
                "income" => MonyFlowOption.Income,
                "expense" => MonyFlowOption.Expense,
                _ => throw new InvalidOperationException("Некорректный тип. Ожидается income или expense.")
            };

            var cat = _categoryService.CreateCategory(name, type);
            Console.WriteLine($"Категория создана: {cat.Id} | {cat.Name} | {cat.FlowType}");
        }
    }
}