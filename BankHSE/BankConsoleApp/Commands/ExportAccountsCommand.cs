using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class ExportAccountsCommand : ICommand
    {
        private readonly ExportService _exportService;

        public ExportAccountsCommand(ExportService exportService)
        {
            _exportService = exportService;
        }

        public string Name => "export-accounts";

        public void Execute()
        {
            Console.Write("Путь для сохранения файла: ");
            var path = Console.ReadLine() ?? string.Empty;

            var content = _exportService.ExportAccounts();
            System.IO.File.WriteAllText(path, content);

            Console.WriteLine($"Счета экспортированы в {path}");
        }
    }
}