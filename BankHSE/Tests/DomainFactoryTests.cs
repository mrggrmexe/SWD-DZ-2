using System;
using Domain.Entity;
using Domain.Factory;
using Xunit;

namespace FinanceTracker.Tests
{
    public class DomainFactoryTests
    {
        private readonly IDomainFactory _factory = new DomainFactory();

        [Fact]
        public void CreateBankAccount_ValidData_Success()
        {
            var account = _factory.CreateBankAccount("Основной", 1000m);

            Assert.NotEqual(Guid.Empty, account.Id);
            Assert.Equal("Основной", account.Name);
            Assert.Equal(1000m, account.Balance);
        }

        [Fact]
        public void CreateBankAccount_NegativeBalance_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                _factory.CreateBankAccount("Неверный", -10m));
        }

        [Fact]
        public void CreateCategory_ValidData_Success()
        {
            var cat = _factory.CreateCategory("Зарплата", MoneyFlowOption.Income);

            Assert.NotEqual(Guid.Empty, cat.Id);
            Assert.Equal("Зарплата", cat.Name);
            Assert.Equal(MoneyFlowOption.Income, cat.FlowType);
        }

        [Fact]
        public void CreateOperation_CategoryTypeMismatch_Throws()
        {
            var acc = _factory.CreateBankAccount("Основной");
            var cat = _factory.CreateCategory("Кафе", MoneyFlowOption.Expense);

            Assert.Throws<InvalidOperationException>(() =>
                _factory.CreateOperation(acc, cat, MoneyFlowOption.Income, 100m, DateTime.Today, "оценка"));
        }

        [Fact]
        public void RestoreOperation_ExactValues_RestoredCorrectly()
        {
            var id = Guid.NewGuid();
            var accId = Guid.NewGuid();
            var catId = Guid.NewGuid();
            var date = new DateTime(2025, 1, 1);

            var op = _factory.RestoreOperation(id, MoneyFlowOption.Expense, accId, catId, 250m, date, "test");

            Assert.Equal(id, op.Id);
            Assert.Equal(accId, op.BankAccountId);
            Assert.Equal(catId, op.CategoryId);
            Assert.Equal(250m, op.Amount);
            Assert.Equal(MoneyFlowOption.Expense, op.Type);
            Assert.Equal(date, op.Date);
            Assert.Equal("test", op.Description);
        }
    }
}
