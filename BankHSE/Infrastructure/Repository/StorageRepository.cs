using System;
using System.Collections.Generic;
using System.Linq;
using Components.Abstraction;

namespace Infrastructure.Repository
{
    /// <summary>
    /// Базовый in-memory репозиторий.
    /// Используется как основное хранилище доменных сущностей.
    /// </summary>
    /// <typeparam name="T">Тип доменной сущности.</typeparam>
    public class StorageRepository<T> : IRepo<T>
    {
        private readonly Dictionary<Guid, T> _storage = new();
        private readonly Func<T, Guid> _idSelector;

        /// <summary>
        /// Создаёт новый in-memory репозиторий.
        /// </summary>
        /// <param name="idSelector">
        /// Функция получения идентификатора сущности.
        /// Например: entity => entity.Id
        /// </param>
        public StorageRepository(Func<T, Guid> idSelector)
        {
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        public void Add(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var id = _idSelector(entity);

            if (_storage.ContainsKey(id))
                throw new InvalidOperationException($"Entity with id {id} already exists.");

            _storage[id] = entity;
        }

        public void Update(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var id = _idSelector(entity);

            if (!_storage.ContainsKey(id))
                throw new KeyNotFoundException($"Entity with id {id} not found.");

            _storage[id] = entity;
        }

        public void Delete(Guid id)
        {
            _storage.Remove(id);
        }

        public T? GetById(Guid id)
        {
            _storage.TryGetValue(id, out var entity);
            return entity;
        }

        public IReadOnlyCollection<T> GetAll()
        {
            // Возвращаем копию в виде read-only коллекции,
            // чтобы внешние клиенты не могли мутировать хранилище.
            return _storage.Values.ToList().AsReadOnly();
        }

        public bool Exists(Guid id)
        {
            return _storage.ContainsKey(id);
        }

        public void Clear()
        {
            _storage.Clear();
        }
    }
}
