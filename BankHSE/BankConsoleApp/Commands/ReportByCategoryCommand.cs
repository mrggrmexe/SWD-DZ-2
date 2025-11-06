using System;
using Components.Command;
using Components.Service;
using Components.Abstraction;
using Domain.Entity;

namespace BankConsoleApp.Commands
{
    public class ReportByCategoryCommand : ICommand
    {
        private readonly AnalysisService _analysisService;
        private readonly IRepo<Category> _categoryRepo;

        public ReportByCategoryCommand(AnalysisService analysisService, IRepo<Category> categoryRepo)
        {
            _analysisService = analysisService;
            _categoryRepo = categoryRepo;
        }

        public string Name => "report-category";

        public void Execute()
        {
            var sums = _analysisService.GetSumByCategory();
            if (sums.Count == 0)
            {
                Console.WriteLine("Нет данных для отчёта.");
                return;
            }

            Console.WriteLine("Суммы по категориям:");
            foreach (var (catId, amount) in sums)
            {
                var cat = _categoryRepo.GetById(catId);
                var name = cat?.Name ?? catId.ToString();
                Console.WriteLine($"{name}: {amount}");
            }
        }
    }
}