using Components.Abstraction;

namespace Infrastructure.Repository
{
    /// <summary>
    /// Потокобезопасный прокси-репозиторий с in-memory кэшем поверх любого IRepo&lt;T&gt;.
    /// - Кэширует результаты GetById.
    /// - Кэширует GetAll (ленивая материализация).
    /// - Инвалидирует кэш при изменениях.
    /// </summary>
    /// <typeparam name="T">Тип доменной сущности.</typeparam>
    public class CachedRepositoryProxy<T> : IRepo<T>
    {
        private readonly IRepo<T> _inner;
        private readonly Func<T, Guid> _idSelector;

        // Кэш по Id
        private readonly Dictionary<Guid, T> _cacheById = new();

        // Кэш для GetAll
        private IReadOnlyCollection<T>? _cacheAll;
        private bool _allDirty = true;

        private readonly object _sync = new();

        public CachedRepositoryProxy(IRepo<T> inner, Func<T, Guid> idSelector)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        public void Add(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var id = _idSelector(entity);

            lock (_sync)
            {
                // Пишем сначала во внутренний репозиторий.
                _inner.Add(entity);

                // Обновляем кэш по Id.
                _cacheById[id] = entity;

                // Инвалидируем кэш GetAll, он потенциально устарел.
                _cacheAll = null;
                _allDirty = true;
            }
        }

        public void Update(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            var id = _idSelector(entity);

            lock (_sync)
            {
                _inner.Update(entity);

                _cacheById[id] = entity;
                _cacheAll = null;
                _allDirty = true;
            }
        }

        public void Delete(Guid id)
        {
            lock (_sync)
            {
                _inner.Delete(id);

                _cacheById.Remove(id);
                _cacheAll = null;
                _allDirty = true;
            }
        }

        public T? GetById(Guid id)
        {
            lock (_sync)
            {
                if (_cacheById.TryGetValue(id, out var cached))
                    return cached;

                // Читаем из внутреннего репозитория один раз.
                var entity = _inner.GetById(id);

                if (entity is not null)
                {
                    _cacheById[id] = entity;
                }

                return entity;
            }
        }

        public IReadOnlyCollection<T> GetAll()
        {
            lock (_sync)
            {
                if (!_allDirty && _cacheAll is not null)
                    return _cacheAll;

                var all = _inner.GetAll();
                var list = all as List<T> ?? all.ToList();

                // Перестраиваем оба кэша единообразно.
                _cacheById.Clear();
                foreach (var e in list)
                {
                    var id = _idSelector(e);
                    _cacheById[id] = e;
                }

                _cacheAll = list.AsReadOnly();
                _allDirty = false;

                return _cacheAll;
            }
        }

        public bool Exists(Guid id)
        {
            lock (_sync)
            {
                if (_cacheById.ContainsKey(id))
                    return true;

                // Однократный вызов внутреннего репозитория.
                var entity = _inner.GetById(id);
                if (entity is null)
                    return false;

                // Если нашли, кладём в кэш.
                var actualId = _idSelector(entity);
                _cacheById[actualId] = entity;
                return true;
            }
        }

        /// <summary>
        /// Полная очистка кэша. Не трогает данные во внутреннем репозитории.
        /// </summary>
        public void ClearCache()
        {
            lock (_sync)
            {
                _cacheById.Clear();
                _cacheAll = null;
                _allDirty = true;
            }
        }
    }
}
