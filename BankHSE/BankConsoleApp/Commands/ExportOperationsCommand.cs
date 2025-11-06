using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class ExportOperationsCommand : ICommand
    {
        private readonly ExportService _exportService;

        public ExportOperationsCommand(ExportService exportService)
        {
            _exportService = exportService;
        }

        public string Name => "export-operations";

        public void Execute()
        {
            Console.Write("Путь для сохранения файла: ");
            var path = Console.ReadLine() ?? string.Empty;

            var content = _exportService.ExportOperations();
            System.IO.File.WriteAllText(path, content);

            Console.WriteLine($"Операции экспортированы в {path}");
        }
    }
}