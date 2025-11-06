using System;
using Components.Command;
using Components.Template;
using Components.Abstraction;
using Domain.Entity;
using Domain.Factory;

namespace BankConsoleApp.Commands
{
    public class ImportOperationsCommand : ICommand
    {
        private readonly OperationCsvImporter _importer;
        private readonly IRepo<Operation> _repo;
        private readonly IDomainFactory _factory;

        public ImportOperationsCommand(OperationCsvImporter importer, IRepo<Operation> repo, IDomainFactory factory)
        {
            _importer = importer;
            _repo = repo;
            _factory = factory;
        }

        public string Name => "import-operations";

        public void Execute()
        {
            Console.Write("Путь к CSV-файлу с операциями: ");
            var path = Console.ReadLine() ?? string.Empty;

            var items = _importer.Import(path);
            int count = 0;

            foreach (var o in items)
            {
                var restored = _factory.RestoreOperation(
                    o.Id, o.Type, o.BankAccountId, o.CategoryId, o.Amount, o.Date, o.Description);
                _repo.Add(restored);
                count++;
            }

            Console.WriteLine($"Импортировано операций: {count}");
        }
    }
}