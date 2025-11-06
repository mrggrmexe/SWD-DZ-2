using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Components.Template
{
    /// <summary>
    /// Шаблонный метод для импорта сущностей из файлов.
    /// Общая логика чтения файла и разбиения на строки вынесена сюда,
    /// парсинг конкретного формата реализуется в подклассах.
    /// </summary>
    /// <typeparam name="T">Тип импортируемой сущности.</typeparam>
    public abstract class ImportTemplate<T>
    {
        public IReadOnlyCollection<T> Import(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));

            var content = File.ReadAllText(filePath);
            var lines = SplitLines(content);
            var items = Parse(lines).ToList();
            return items.AsReadOnly();
        }

        protected virtual IEnumerable<string> SplitLines(string content) =>
            content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        /// <summary>
        /// Парсинг строк файла в сущности.
        /// Реализуется конкретными импортёрами.
        /// </summary>
        protected abstract IEnumerable<T> Parse(IEnumerable<string> lines);
    }
}