using System;
using System.Collections.Generic;
using Components.Abstraction;
using Domain.Entity;
using Domain.Factory;

namespace Components.Service
{
    /// <summary>
    /// Фасад для управления категориями доходов и расходов.
    /// </summary>
    public class CategoryService
    {
        private readonly IRepo<Category> _categories;
        private readonly IDomainFactory _factory;

        public CategoryService(IRepo<Category> categories, IDomainFactory factory)
        {
            _categories = categories ?? throw new ArgumentNullException(nameof(categories));
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
        }

        public Category CreateCategory(string name, MonyFlowOption flowType)
        {
            var category = _factory.CreateCategory(name, flowType);
            _categories.Add(category);
            return category;
        }

        public void RenameCategory(Guid categoryId, string newName)
        {
            var category = _categories.GetById(categoryId)
                           ?? throw new InvalidOperationException("Category not found.");

            category.Rename(newName);
            _categories.Update(category);
        }

        public void DeleteCategory(Guid categoryId)
        {
            _categories.Delete(categoryId);
        }

        public Category? GetById(Guid id) => _categories.GetById(id);

        public IReadOnlyCollection<Category> GetAll() => _categories.GetAll();
    }
}