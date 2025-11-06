using System;
using System.Collections.Generic;
using Components.Abstraction;
using Components.Command;
using Components.Export;
using Components.Service;
using Components.Service.ReportProc;
using Components.Template;
using Domain.Entity;
using Domain.Factory;
using Infrastructure.Repository;
using BankConsoleApp.Commands;

namespace BankConsoleApp
{
    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;

            // --- Composition Root ---

            // 1. Фабрика домена
            IDomainFactory domainFactory = new DomainFactory();

            // 2. Репозитории (Storage + Cached Proxy)
            IRepo<BankAccount> accountRepo =
                new CachedRepositoryProxy<BankAccount>(
                    new StorageRepository<BankAccount>(a => a.Id),
                    a => a.Id);

            IRepo<Category> categoryRepo =
                new CachedRepositoryProxy<Category>(
                    new StorageRepository<Category>(c => c.Id),
                    c => c.Id);

            IRepo<Operation> operationRepo =
                new CachedRepositoryProxy<Operation>(
                    new StorageRepository<Operation>(o => o.Id),
                    o => o.Id);

            // 3. Процедуры отчётов (Strategy для AnalysisService)
            var reportProcs = new List<IReportProc>
            {
                new AmountDescProc(),
                new NameAscProc()
            };

            // 4. Экспортёр (Visitor/Strategy)
            var csvExporter = new CsvExpProc();

            // 5. Сервисы (Facade-слой)
            var accountService = new AccountService(accountRepo, domainFactory);
            var categoryService = new CategoryService(categoryRepo, domainFactory);
            var operationService = new OperationService(operationRepo, accountRepo, categoryRepo, domainFactory);
            var analysisService = new AnalysisService(operationRepo, reportProcs);
            var exportService = new ExportService(accountRepo, categoryRepo, operationRepo, csvExporter);

            // 6. Импорт шаблоны
            var accountImporter = new AccountCsvImporter();
            var operationImporter = new OperationCsvImporter();

            // 7. Командный инвокер
            var invoker = new CommandInvoker();

            // Логгер для декоратора времени
            void Log(string msg) => Console.WriteLine(msg);

            // Регистрация команд
            var exitState = new ExitState();

            Register(invoker, new AddAccountCommand(accountService));
            Register(invoker, new AddCategoryCommand(categoryService));
            Register(invoker, new AddOperationCommand(accountService, categoryService, operationService));

            Register(invoker, new BurnAccountCommand(accountService));
            Register(invoker, new BurnCategoryCommand(categoryService));
            Register(invoker, new BurnOperationCommand(operationService));

            Register(invoker, new EditAccountCommand(accountService));
            Register(invoker, new EditCategoryCommand(categoryService));
            Register(invoker, new EditOperationCommand(operationService, accountService, categoryService));

            Register(invoker, new ListAccountsCommand(accountService));
            Register(invoker, new ListCategoriesCommand(categoryService));
            Register(invoker, new ListOperationsCommand(operationService));

            Register(invoker, new ImportAccountsCommand(accountImporter, accountRepo, domainFactory));
            Register(invoker, new ImportOperationsCommand(operationImporter, operationRepo, domainFactory));

            Register(invoker, new ExportAccountsCommand(exportService));
            Register(invoker, new ExportCategoriesCommand(exportService));
            Register(invoker, new ExportOperationsCommand(exportService));

            Register(invoker, new ReportSummaryCommand(analysisService));
            Register(invoker, new ReportByCategoryCommand(analysisService, categoryRepo));

            Register(invoker, new UpdateBalanceCommand(accountRepo, operationRepo));

            Register(invoker, new HelpCommand(invoker));
            Register(invoker, new ExitCommand(exitState));

            // Обёртка декоратором времени для ключевых сценариев:
            void Register(CommandInvoker cmdInvoker, Components.Command.ICommand cmd)
            {
                // замеряем только "бизнесовые" сценарии, help/exit можно не оборачивать
                if (!string.Equals(cmd.Name, "help", StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(cmd.Name, "exit", StringComparison.OrdinalIgnoreCase))
                {
                    cmdInvoker.Register(new TimingCmdDecorator(cmd, Log));
                }
                else
                {
                    cmdInvoker.Register(cmd);
                }
            }

            // --- Main loop ---

            Console.WriteLine("HSE Bank · Finance Tracking Console");
            Console.WriteLine("Введите 'help' для списка команд, 'exit' для выхода.");
            Console.WriteLine();

            while (!exitState.IsRequested)
            {
                Console.Write("> ");
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    continue;

                var cmdName = input.Trim();

                if (!invoker.TryExecute(cmdName))
                {
                    Console.WriteLine("Неизвестная команда. Введите 'help' для списка доступных команд.");
                }

                Console.WriteLine();
            }

            Console.WriteLine("До встречи!");
        }

        /// <summary>
        /// Общий разделяемый объект для ExitCommand.
        /// </summary>
        private class ExitState
        {
            public bool IsRequested { get; set; }
        }
    }
}
