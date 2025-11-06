using System.Globalization;
using Components.Command;
using Components.Service;
using Domain.Entity;

namespace BankConsoleApp.Commands
{
    public class AddOperationCommand : ICommand
    {
        private readonly AccountService _accountService;
        private readonly CategoryService _categoryService;
        private readonly OperationService _operationService;

        public AddOperationCommand(
            AccountService accountService,
            CategoryService categoryService,
            OperationService operationService)
        {
            _accountService = accountService ?? throw new ArgumentNullException(nameof(accountService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _operationService = operationService ?? throw new ArgumentNullException(nameof(operationService));
        }

        public string Name => "add-operation";

        public void Execute()
        {
            var accounts = _accountService.GetAll();
            if (accounts.Count == 0)
            {
                Console.WriteLine("Нет доступных счетов. Сначала создайте счёт (add-account).");
                return;
            }

            var categories = _categoryService.GetAll();
            if (categories.Count == 0)
            {
                Console.WriteLine("Нет доступных категорий. Сначала создайте категорию (add-category).");
                return;
            }

            Console.WriteLine("Доступные счета:");
            foreach (var a in accounts)
                Console.WriteLine($" - {a.Id} | {a.Name} | Баланс: {a.Balance}");

            var accId = ReadExistingGuid("ID счёта: ", id => _accountService.GetById(id) != null);
            if (accId == Guid.Empty)
            {
                Console.WriteLine("Операция не создана: не удалось выбрать счёт.");
                return;
            }

            Console.WriteLine("Доступные категории:");
            foreach (var c in categories)
                Console.WriteLine($" - {c.Id} | {c.Name} | {c.FlowType}");

            var catId = ReadExistingGuid("ID категории: ", id => _categoryService.GetById(id) != null);
            if (catId == Guid.Empty)
            {
                Console.WriteLine("Операция не создана: не удалось выбрать категорию.");
                return;
            }

            var category = _categoryService.GetById(catId)!;

            var type = ReadFlowTypeWithCategory(category);
            if (type == MoneyFlowOption.Unknown)
            {
                Console.WriteLine("Операция не создана: некорректный тип операции.");
                return;
            }

            var amount = ReadPositiveDecimal("Сумма: ");
            if (amount <= 0)
            {
                Console.WriteLine("Операция не создана: сумма должна быть положительной.");
                return;
            }

            var date = ReadDateOrToday("Дата (yyyy-MM-dd), пусто = сегодня: ");

            Console.Write("Описание (опционально): ");
            var desc = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(desc))
                desc = null;

            try
            {
                var op = _operationService.CreateOperation(accId, catId, type, amount, date, desc);
                Console.WriteLine($"Операция создана: {op.Id} ({op.Type}) {op.Amount} на {op.Date:yyyy-MM-dd}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Не удалось создать операцию: {ex.Message}");
            }
        }

        private static Guid ReadExistingGuid(string prompt, Func<Guid, bool> existsPredicate)
        {
            const int maxAttempts = 3;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (!Guid.TryParse(input, out var id) || id == Guid.Empty)
                {
                    Console.WriteLine("Некорректный формат GUID. Попробуйте ещё раз.");
                    continue;
                }

                if (!existsPredicate(id))
                {
                    Console.WriteLine("Объект с таким ID не найден. Попробуйте ещё раз.");
                    continue;
                }

                return id;
            }

            return Guid.Empty;
        }

        private static MoneyFlowOption ReadFlowTypeWithCategory(Category category)
        {
            const int maxAttempts = 3;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write($"Тип (income/expense), по умолчанию {category.FlowType}: ");
                var input = (Console.ReadLine() ?? string.Empty).Trim().ToLowerInvariant();

                if (string.IsNullOrEmpty(input))
                {
                    // Если пользователь ничего не ввёл — используем тип категории,
                    // чтобы не нарушать инвариант (Category.FlowType == Operation.Type).
                    return category.FlowType;
                }

                if (input is "income" or "i" or "+" && category.FlowType == MoneyFlowOption.Income)
                    return MoneyFlowOption.Income;

                if (input is "expense" or "e" or "-" && category.FlowType == MoneyFlowOption.Expense)
                    return MoneyFlowOption.Expense;

                Console.WriteLine("Тип операции не совпадает с типом категории. Попробуйте ещё раз.");
            }

            return MoneyFlowOption.Unknown;
        }

        private static decimal ReadPositiveDecimal(string prompt)
        {
            const int maxAttempts = 3;

            for (var attempt = 0; attempt < maxAttempts; attempt++)
            {
                Console.Write(prompt);
                var input = Console.ReadLine();

                if (decimal.TryParse(input, NumberStyles.Number, CultureInfo.InvariantCulture, out var value) ||
                    decimal.TryParse(input ?? string.Empty, NumberStyles.Number, CultureInfo.CurrentCulture, out value))
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

            return -1m;
        }

        private static DateTime ReadDateOrToday(string prompt)
        {
            Console.Write(prompt);
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input))
                return DateTime.Today;

            if (DateTime.TryParseExact(input.Trim(), "yyyy-MM-dd",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;

            if (DateTime.TryParse(input, CultureInfo.CurrentCulture, DateTimeStyles.None, out date))
                return date;

            Console.WriteLine("Некорректная дата. Используется сегодняшняя.");
            return DateTime.Today;
        }
    }
}
