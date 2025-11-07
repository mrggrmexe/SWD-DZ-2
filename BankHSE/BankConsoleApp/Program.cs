using System.Text;
using BankConsoleApp.Commands;
using Components.Abstraction;
using Components.Command;
using Components.Export;
using Components.Service;
using Components.Service.ReportProc;
using Components.Template;
using Domain.Entity;
using Domain.Factory;
using Infrastructure.Repository;

namespace BankConsoleApp
{
    internal static class Program
    {
        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            try
            {
                Run();
            }
            catch (Exception ex)
            {
                // Финальный страховочный барьер: чтобы приложение не падало без сообщений.
                Console.WriteLine("Критическая ошибка приложения:");
                Console.WriteLine(ex.Message);
            }
        }

        private static void Run()
        {
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

            // 7. Состояние выхода
            var exitState = new ExitState();

            // 8. Логгер и обработчик ошибок команд
            void Log(string msg)
            {
                if (!string.IsNullOrWhiteSpace(msg))
                    Console.WriteLine(msg);
            }

            void OnCommandError(Exception ex)
            {
                Console.WriteLine($"Ошибка при выполнении команды: {ex.Message}");
            }

            // 9. Командный инвокер (стрессоустойчивый)
            var invoker = new CommandInvoker(Log, OnCommandError);

            // 10. Регистрация команд через общий метод
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

            Register(invoker, new HelpCommand(invoker), measure: false);
            Register(invoker, new ExitCommand(exitState), measure: false);

            // 11. Обработка Ctrl+C — мягкое завершение
            Console.CancelKeyPress += (_, e) =>
            {
                e.Cancel = true;
                exitState.IsRequested = true;
                Log("Запрошено завершение (Ctrl+C).");
            };

            // --- Main loop ---

            Console.WriteLine("HSE Bank · Finance Tracking Console");
            Console.WriteLine("Введите 'help' для списка команд, 'exit' для выхода.");
            Console.WriteLine();

            const StringComparison ordIgnore = StringComparison.OrdinalIgnoreCase;

            while (!exitState.IsRequested)
            {
                Console.Write("> ");

                string? input;
                try
                {
                    input = Console.ReadLine();
                }
                catch (Exception ex) when (ex is IOException || ex is ObjectDisposedException)
                {
                    // Поток ввода недоступен — корректно завершаем.
                    Log("Поток ввода недоступен. Завершение работы.");
                    break;
                }

                if (input is null)
                {
                    // EOF (например, Ctrl+Z в Windows) — завершаем аккуратно.
                    Log("Обнаружен конец ввода. Завершение работы.");
                    break;
                }

                var cmdName = input.Trim();
                if (cmdName.Length == 0)
                    continue;

                // Дополнительные синонимы выхода
                if (cmdName.Equals("q", ordIgnore) || cmdName.Equals("quit", ordIgnore))
                {
                    exitState.IsRequested = true;
                    continue;
                }

                // TryExecute теперь:
                // - вернет false, если команда не найдена,
                // - вернет false, если внутри команды было исключение (оно залогировано OnCommandError).
                if (!invoker.TryExecute(cmdName))
                {
                    Console.WriteLine("Команда не найдена или завершилась с ошибкой. Введите 'help' для списка доступных команд.");
                }

                Console.WriteLine();
            }

            Console.WriteLine("До встречи!");
        }

        /// <summary>
        /// Общий разделяемый объект для ExitCommand.
        /// </summary>
        private sealed class ExitState
        {
            public bool IsRequested { get; set; }
        }

        /// <summary>
        /// Регистрация команд с опциональным оборачиванием в TimingCmdDecorator.
        /// Защита изменения служебных комманд
        /// </summary>
        private static void Register(CommandInvoker? invoker, ICommand? command, bool measure = true)
        {
            if (invoker is null || command is null)
                return;

            var name = command.Name;
            if (string.IsNullOrWhiteSpace(name))
                return;

            // Не измеряем служебные команды или если явно отключено.
            if (!measure ||
                name.Equals("help", StringComparison.OrdinalIgnoreCase) ||
                name.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                invoker.Register(command);
            }
            else
            {
                invoker.Register(new TimingCmdDecorator(command, msg =>
                {
                    if (!string.IsNullOrWhiteSpace(msg))
                        Console.WriteLine(msg);
                }));
            }
        }
    }
}
