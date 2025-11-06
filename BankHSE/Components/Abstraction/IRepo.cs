using System;
using System.Collections.Generic;

namespace Components.Abstraction
{
    /// <summary>
    /// Базовый контракт репозитория для доменных сущностей.
    /// Реализации находятся в слое Infrastructure.
    /// </summary>
    /// <typeparam name="T">Тип доменной сущности.</typeparam>
    public interface IRepo<T>
    {
        void Add(T entity);
        void Update(T entity);
        void Delete(Guid id);
        T? GetById(Guid id);
        IReadOnlyCollection<T> GetAll();
    }
}