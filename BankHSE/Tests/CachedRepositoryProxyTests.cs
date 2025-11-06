using System;
using System.Linq;
using Components.Abstraction;
using Domain.Entity;
using Infrastructure.Repository;
using Xunit;

namespace FinanceTracker.Tests
{
    public class CachedRepositoryProxyTests
    {
        private IRepo<BankAccount> CreateRepo()
        {
            var inner = new StorageRepository<BankAccount>(a => a.Id);
            var proxy = new CachedRepositoryProxy<BankAccount>(inner, a => a.Id);
            return proxy;
        }

        [Fact]
        public void Add_And_GetById_ReturnsSameEntity()
        {
            var repo = CreateRepo();

            var acc = new BankAccount(Guid.NewGuid(), "Test", 100m);
            repo.Add(acc);

            var loaded = repo.GetById(acc.Id);

            Assert.NotNull(loaded);
            Assert.Equal("Test", loaded!.Name);
            Assert.Equal(100m, loaded.Balance);
        }

        [Fact]
        public void GetAll_AfterAdd_CountMatches()
        {
            var repo = CreateRepo();

            repo.Add(new BankAccount(Guid.NewGuid(), "A", 10m));
            repo.Add(new BankAccount(Guid.NewGuid(), "B", 20m));

            var all = repo.GetAll();

            Assert.Equal(2, all.Count);
        }

        [Fact]
        public void Delete_RemovesEntity()
        {
            var repo = CreateRepo();

            var acc = new BankAccount(Guid.NewGuid(), "Del", 50m);
            repo.Add(acc);

            repo.Delete(acc.Id);

            Assert.Null(repo.GetById(acc.Id));
            Assert.DoesNotContain(repo.GetAll(), x => x.Id == acc.Id);
        }

        [Fact]
        public void Update_ChangesReflected()
        {
            var repo = CreateRepo();

            var acc = new BankAccount(Guid.NewGuid(), "Old", 10m);
            repo.Add(acc);

            acc.Rename("New");
            repo.Update(acc);

            var loaded = repo.GetById(acc.Id);
            Assert.NotNull(loaded);
            Assert.Equal("New", loaded!.Name);
        }
    }
}