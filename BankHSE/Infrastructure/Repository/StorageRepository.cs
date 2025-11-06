using Components.Abstraction;

namespace Infrastructure.Repository
{
    /// <summary>
    /// Потокобезопасный in-memory репозиторий.
    /// Используется как основное хранилище доменных сущностей.
    /// </summary>
    /// <typeparam name="T">Тип доменной сущности.</typeparam>
    public class StorageRepository<T> : IRepo<T>
    {
        private readonly Dictionary<Guid, T> _storage = new();
        private readonly Func<T, Guid> _idSelector;
        private readonly object _sync = new();

        public StorageRepository(Func<T, Guid> idSelector)
        {
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        public void Add(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var id = _idSelector(entity);

            lock (_sync)
            {
                if (_storage.ContainsKey(id))
                    throw new InvalidOperationException($"Entity with id {id} already exists.");

                _storage[id] = entity;
            }
        }

        public void Update(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var id = _idSelector(entity);

            lock (_sync)
            {
                if (!_storage.ContainsKey(id))
                    throw new KeyNotFoundException($"Entity with id {id} not found.");

                _storage[id] = entity;
            }
        }

        public void Delete(Guid id)
        {
            lock (_sync)
            {
                // Удаление идемпотентно: отсутствие сущности не считается ошибкой
                _storage.Remove(id);
            }
        }

        public T? GetById(Guid id)
        {
            lock (_sync)
            {
                _storage.TryGetValue(id, out var entity);
                return entity;
            }
        }

        public IReadOnlyCollection<T> GetAll()
        {
            lock (_sync)
            {
                // Копируем в отдельный список, наружу отдаём read-only,
                // чтобы не нарушили инварианты.
                return _storage.Values.ToList().AsReadOnly();
            }
        }

        public bool Exists(Guid id)
        {
            lock (_sync)
            {
                return _storage.ContainsKey(id);
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _storage.Clear();
            }
        }
    }
}
