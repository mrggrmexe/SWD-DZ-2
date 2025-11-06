using System;
using System.Collections.Generic;
using Domain.Entity;
using Xunit;

namespace FinanceTracker.Tests
{
    public class BankAccountTests
    {
        [Fact]
        public void ApplyOperation_Income_IncreasesBalance()
        {
            var acc = new BankAccount(Guid.NewGuid(), "Основной", 0m);
            var op = new Operation(Guid.NewGuid(), MoneyFlowOption.Income, acc.Id,
                Guid.NewGuid(), 500m, DateTime.Today, null);

            acc.ApplyOperation(op);

            Assert.Equal(500m, acc.Balance);
        }

        [Fact]
        public void ApplyOperation_Expense_DecreasesBalance()
        {
            var acc = new BankAccount(Guid.NewGuid(), "Основной", 1000m);
            var op = new Operation(Guid.NewGuid(), MoneyFlowOption.Expense, acc.Id,
                Guid.NewGuid(), 300m, DateTime.Today, null);

            acc.ApplyOperation(op);

            Assert.Equal(700m, acc.Balance);
        }

        [Fact]
        public void ApplyOperation_WrongAccount_Throws()
        {
            var acc = new BankAccount(Guid.NewGuid(), "Основной", 0m);
            var op = new Operation(Guid.NewGuid(), MoneyFlowOption.Income,
                Guid.NewGuid(), Guid.NewGuid(), 100m, DateTime.Today, null);

            Assert.Throws<InvalidOperationException>(() => acc.ApplyOperation(op));
        }

        [Fact]
        public void RecalculateBalance_CorrectlyAggregatesOperations()
        {
            var id = Guid.NewGuid();
            var acc = new BankAccount(id, "Основной", 0m);

            var ops = new List<Operation>
            {
                new Operation(Guid.NewGuid(), MoneyFlowOption.Income, id, Guid.NewGuid(), 1000m, DateTime.Today, null),
                new Operation(Guid.NewGuid(), MoneyFlowOption.Expense, id, Guid.NewGuid(), 200m, DateTime.Today, null),
                new Operation(Guid.NewGuid(), MoneyFlowOption.Expense, Guid.NewGuid(), Guid.NewGuid(), 999m, DateTime.Today, null) // другой счёт
            };

            acc.RecalculateBalance(ops);

            Assert.Equal(800m, acc.Balance);
        }
    }
}
