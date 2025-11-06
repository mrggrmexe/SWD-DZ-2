namespace Components.Template
{
    /// <summary>
    /// Базовый шаблонный метод для импорта сущностей из файлов.
    /// Общая логика:
    /// - проверка пути
    /// - потокобезопасное чтение файла
    /// - передача строк в конкретный парсер
    /// Реализации должны корректно обрабатывать мусорные строки.
    /// </summary>
    /// <typeparam name="T">Тип импортируемой сущности.</typeparam>
    public abstract class ImportTemplate<T>
    {
        /// <summary>
        /// Импорт из файла. При ошибках парсинга отдельных строк они должны
        /// обрабатываться внутри Parse и не ронять весь импорт.
        /// </summary>
        public IReadOnlyCollection<T> Import(string filePath)
        {
            return Import(filePath, onError: null);
        }

        /// <summary>
        /// Импорт с возможностью получения сообщения об ошибке верхнего уровня.
        /// </summary>
        public IReadOnlyCollection<T> Import(string filePath, Action<string>? onError)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));

            if (!File.Exists(filePath))
                throw new FileNotFoundException("Input file not found.", filePath);

            var result = new List<T>();

            try
            {
                foreach (var item in Parse(ReadLines(filePath)))
                {
                    if (item is not null)
                        result.Add(item);
                }
            }
            catch (Exception ex)
            {
                onError?.Invoke($"Import failed: {ex.Message}");
                throw;
            }

            return result.AsReadOnly();
        }

        /// <summary>
        /// Потоковое чтение строк файла.
        /// Пустые строки отбрасываются.
        /// </summary>
        private static IEnumerable<string> ReadLines(string filePath)
        {
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var reader = new StreamReader(stream);

            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    yield return line;
            }
        }

        /// <summary>
        /// Парсинг строк файла в сущности.
        /// Реализуется конкретными импортёрами.
        /// Должен быть устойчив к некорректным строкам.
        /// </summary>
        protected abstract IEnumerable<T> Parse(IEnumerable<string> lines);
    }
}
