using System;

namespace Domain.Entity
{
    /// <summary>
    /// Категория дохода или расхода.
    /// Например: "Зарплата", "Кафе", "Здоровье".
    /// </summary>
    public class Category
    {
        public Guid Id { get; }
        public string Name { get; private set; }
        public MonyFlowOption FlowType { get; }

        #region Конструкторы

        public Category(Guid id, string name, MonyFlowOption flowType)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Id must be non-empty.", nameof(id));

            SetName(name);
            FlowType = flowType;
            Id = id;
        }

        internal Category(string name, MonyFlowOption flowType)
            : this(Guid.NewGuid(), name, flowType)
        {
        }

        #endregion

        public void Rename(string newName)
        {
            SetName(newName);
        }

        private void SetName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Category name must be non-empty.", nameof(name));

            Name = name.Trim();
        }
    }
}