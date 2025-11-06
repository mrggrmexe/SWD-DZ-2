using System;
using Components.Command;
using Components.Template;
using Components.Abstraction;
using Domain.Entity;
using Domain.Factory;

namespace BankConsoleApp.Commands
{
    public class ImportAccountsCommand : ICommand
    {
        private readonly AccountCsvImporter _importer;
        private readonly IRepo<BankAccount> _repo;
        private readonly IDomainFactory _factory;

        public ImportAccountsCommand(AccountCsvImporter importer, IRepo<BankAccount> repo, IDomainFactory factory)
        {
            _importer = importer;
            _repo = repo;
            _factory = factory;
        }

        public string Name => "import-accounts";

        public void Execute()
        {
            Console.Write("Путь к CSV-файлу со счетами: ");
            var path = Console.ReadLine() ?? string.Empty;

            var items = _importer.Import(path);
            int count = 0;

            foreach (var a in items)
            {
                // В учебных целях считаем, что Id и данные валидны
                var restored = _factory.RestoreBankAccount(a.Id, a.Name, a.Balance);
                _repo.Add(restored);
                count++;
            }

            Console.WriteLine($"Импортировано счетов: {count}");
        }
    }
}