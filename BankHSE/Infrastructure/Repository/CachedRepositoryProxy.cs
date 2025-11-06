using System;
using System.Collections.Generic;
using System.Linq;
using Components.Abstraction;

namespace Infrastructure.Repository
{
    /// <summary>
    /// Прокси-репозиторий с in-memory кэшем поверх любого IRepo&lt;T&gt;.
    /// Демонстрирует паттерн Proxy:
    /// - читает из кэша при повторных обращениях;
    /// - инвалидирует кэш при изменениях.
    /// </summary>
    /// <typeparam name="T">Тип доменной сущности.</typeparam>
    public class CachedRepositoryProxy<T> : IRepo<T>
    {
        private readonly IRepo<T> _inner;
        private readonly Func<T, Guid> _idSelector;

        private readonly Dictionary<Guid, T> _cacheById = new();
        private IReadOnlyCollection<T>? _cacheAll;
        private bool _allDirty = true;

        public CachedRepositoryProxy(IRepo<T> inner, Func<T, Guid> idSelector)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
            _idSelector = idSelector ?? throw new ArgumentNullException(nameof(idSelector));
        }

        public void Add(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _inner.Add(entity);

            var id = _idSelector(entity);
            _cacheById[id] = entity;
            _allDirty = true;
        }

        public void Update(T entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity));

            _inner.Update(entity);

            var id = _idSelector(entity);
            _cacheById[id] = entity;
            _allDirty = true;
        }

        public void Delete(Guid id)
        {
            _inner.Delete(id);

            _cacheById.Remove(id);
            _allDirty = true;
        }

        public T? GetById(Guid id)
        {
            if (_cacheById.TryGetValue(id, out var cached))
                return cached;

            var entity = _inner.GetById(id);
            if (entity is not null)
            {
                _cacheById[id] = entity;
            }

            return entity;
        }

        public IReadOnlyCollection<T> GetAll()
        {
            if (!_allDirty && _cacheAll is not null)
                return _cacheAll;

            var all = _inner.GetAll() ?? Array.Empty<T>();
            var list = all.ToList();

            _cacheAll = list.AsReadOnly();
            _cacheById.Clear();

            foreach (var e in list)
            {
                var id = _idSelector(e);
                _cacheById[id] = e;
            }

            _allDirty = false;
            return _cacheAll;
        }

        public bool Exists(Guid id)
        {
            if (_cacheById.ContainsKey(id))
                return true;

            var exists = _inner is { } && _inner.GetById(id) is not null;
            if (exists)
            {
                var entity = _inner.GetById(id);
                if (entity is not null)
                    _cacheById[id] = entity;
            }

            return exists;
        }

        public void ClearCache()
        {
            _cacheById.Clear();
            _cacheAll = null;
            _allDirty = true;
        }
    }
}
