using System.Globalization;
using Components.Command;
using Components.Service;
using Domain.Entity;

namespace BankConsoleApp.Commands
{
    public class EditOperationCommand : ICommand
    {
        private readonly OperationService _operationService;
        private readonly AccountService _accountService;
        private readonly CategoryService _categoryService;

        public EditOperationCommand(
            OperationService operationService,
            AccountService accountService,
            CategoryService categoryService)
        {
            _operationService = operationService ?? throw new ArgumentNullException(nameof(operationService));
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
        }

        public string Name => "edit-operation";

        public void Execute()
        {
            var id = ReadGuidWithAttempts("ID операции для редактирования: ");
            if (id == Guid.Empty)
            {
                Console.WriteLine("Операция не изменена: не удалось прочитать корректный ID.");
                return;
            }

            var existing = _operationService.GetById(id);
            if (existing is null)
            {
                Console.WriteLine("Операция не найдена.");
                return;
            }

            Console.WriteLine("Текущие значения операции:");
            Console.WriteLine($" - Счёт:      {existing.BankAccountId}");
            Console.WriteLine($" - Категория: {existing.CategoryId}");
            Console.WriteLine($" - Тип:       {existing.Type}");
            Console.WriteLine($" - Сумма:     {existing.Amount}");
            Console.WriteLine($" - Дата:      {existing.Date:yyyy-MM-dd}");
            Console.WriteLine($" - Описание:  {existing.Description}");

            // 1. Новый счёт (Enter — оставить)
            var newAccountId = ReadOptionalGuid(
                "Новый ID счёта (Enter — оставить текущий): ",
                existing.BankAccountId,
                id => _accountService.GetById(id) != null,
                "Счёт с таким ID не найден. Оставлено исходное значение."
            );

            // 2. Новая категория (Enter — оставить)
            var newCategoryId = ReadOptionalGuid(
                "Новый ID категории (Enter — оставить текущую): ",
                existing.CategoryId,
                id => _categoryService.GetById(id) != null,
                "Категория с таким ID не найдена. Оставлено исходное значение."
            );

            var category = _categoryService.GetById(newCategoryId);
            if (category is null)
            {
                Console.WriteLine("Ошибка: выбранная категория не существует.");
                return;
            }

            // 3. Новый тип, согласованный с категорией
            var newType = ReadMoneyFlowTypeWithCategory(
                category,
                $"Тип операции (income/expense, Enter — {category.FlowType}): ");

            if (newType == MoneyFlowOption.Unknown)
            {
                Console.WriteLine("Операция не изменена: не удалось выбрать корректный тип.");
                return;
            }

            // 4. Новая сумма
            var newAmount = ReadOptionalPositiveDecimal(
                $"Новая сумма (Enter — {existing.Amount}): ",
                existing.Amount);

            if (newAmount <= 0)
            {
                Console.WriteLine("Операция не изменена: сумма должна быть положительной.");
                return;
            }

            // 5. Новая дата
            var newDate = ReadOptionalDate(
                $"Новая дата (yyyy-MM-dd, Enter — {existing.Date:yyyy-MM-dd}): ",
                existing.Date);

            // 6. Новое описание
            Console.Write($"Новое описание (Enter — оставить текущее): ");
            var newDescRaw = Console.ReadLine();
            var newDesc = string.IsNullOrWhiteSpace(newDescRaw)
                ? existing.Description
                : newDescRaw.Trim();

            // 7. Применяем изменения
            try
            {
                var oldAccountId = existing.BankAccountId;

                // Создаём новую операцию (может бросить исключение — тогда старую не трогаем)
                var newOp = _operationService.CreateOperation(
                    newAccountId,
                    newCategoryId,
                    newType,
                    newAmount,
                    newDate,
                    newDesc);

                // Удаляем старую только после успешного создания новой
                _operationService.DeleteOperation(id);

                // Пересчитываем балансы только для задействованных счетов
                var allOps = _operationService.GetAll();

                RecalculateAccountIfExists(oldAccountId, allOps);
                if (newAccountId != oldAccountId)
                    RecalculateAccountIfExists(newAccountId, allOps);

                Console.WriteLine($"Операция обновлена. Новый ID: {newOp.Id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при изменении операции: {ex.Message}");
            }
        }

        #region Вспомогательные методы

        private static Guid ReadGuidWithAttempts(string prompt, int maxAttempts = 3)
        {
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (Guid.TryParse(input, out var id) && id != Guid.Empty)
                    return id;

                Console.WriteLine("Некорректный формат GUID. Попробуйте ещё раз.");
            }

            return Guid.Empty;
        }

        private static Guid ReadOptionalGuid(
            string prompt,
            Guid current,
            Func<Guid, bool> existsPredicate,
            string notFoundMessage,
            int maxAttempts = 3)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return current;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                if (Guid.TryParse(input, out var id) && id != Guid.Empty)
                {
                    if (existsPredicate(id))
                        return id;

                    Console.WriteLine(notFoundMessage);
                    return current;
                }

                Console.WriteLine("Некорректный формат GUID. Попробуйте ещё раз или оставьте пустым для сохранения текущего.");
                Console.Write(prompt);
                input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                    return current;
            }

            return current;
        }

        private static MoneyFlowOption ReadMoneyFlowTypeWithCategory(Category category, string prompt, int maxAttempts = 3)
        {
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write(prompt);
                var input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(input))
                {
                    // Enter — использовать тип категории (сохраняем инвариант)
                    return category.FlowType;
                }

                if (input is "income" or "i" or "+")
                {
                    if (category.FlowType == MoneyFlowOption.Income)
                        return MoneyFlowOption.Income;

                    Console.WriteLine("Тип операции не соответствует типу категории. Ожидается income.");
                    continue;
                }

                if (input is "expense" or "e" or "-")
                {
                    if (category.FlowType == MoneyFlowOption.Expense)
                        return MoneyFlowOption.Expense;

                    Console.WriteLine("Тип операции не соответствует типу категории. Ожидается expense.");
                    continue;
                }

                Console.WriteLine("Некорректный ввод. Допустимо: income / expense.");
            }

            return MoneyFlowOption.Unknown;
        }

        private static decimal ReadOptionalPositiveDecimal(string prompt, decimal current, int maxAttempts = 3)
        {
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return current;

                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ||
                    decimal.TryParse(input, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
                {
                    if (value > 0)
                        return value;

                    Console.WriteLine("Значение должно быть положительным.");
                }
                else
                {
                    Console.WriteLine("Некорректный формат числа.");
                }
            }

            return current;
        }

        private static DateTime ReadOptionalDate(string prompt, DateTime current, int maxAttempts = 3)
        {
            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(input))
                    return current;

                if (DateTime.TryParseExact(input.Trim(), "yyyy-MM-dd",
                        CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                    DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
                {
                    return date;
                }

                Console.WriteLine("Некорректная дата. Ожидается yyyy-MM-dd или локальный формат.");
            }

            return current;
        }

        private void RecalculateAccountIfExists(Guid accountId, IReadOnlyCollection<Operation> operations)
        {
            if (accountId == Guid.Empty)
                return;

            var acc = _accountService.GetById(accountId);
            if (acc is null)
                return;

            acc.RecalculateBalance(operations);
        }

        #endregion
    }
}
