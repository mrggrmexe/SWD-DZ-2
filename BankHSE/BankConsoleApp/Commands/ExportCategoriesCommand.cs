using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class ExportCategoriesCommand : ICommand
    {
        private readonly ExportService _exportService;

        public ExportCategoriesCommand(ExportService exportService)
        {
            _exportService = exportService;
        }

        public string Name => "export-categories";

        public void Execute()
        {
            Console.Write("Путь для сохранения файла: ");
            var path = Console.ReadLine() ?? string.Empty;

            var content = _exportService.ExportCategories();
            System.IO.File.WriteAllText(path, content);

            Console.WriteLine($"Категории экспортированы в {path}");
        }
    }
}