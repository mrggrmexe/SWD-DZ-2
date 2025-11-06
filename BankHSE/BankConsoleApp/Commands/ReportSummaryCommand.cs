using System;
using Components.Command;
using Components.Service;

namespace BankConsoleApp.Commands
{
    public class ReportSummaryCommand : ICommand
    {
        private readonly AnalysisService _analysisService;

        public ReportSummaryCommand(AnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        public string Name => "report-summary";

        public void Execute()
        {
            Console.Write("Дата начала (yyyy-MM-dd): ");
            var from = DateTime.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            Console.Write("Дата конца (yyyy-MM-dd): ");
            var to = DateTime.Parse(Console.ReadLine() ?? throw new InvalidOperationException());

            var (income, expense, diff) = _analysisService.GetSummary(from, to);

            Console.WriteLine($"Доходы: {income}");
            Console.WriteLine($"Расходы: {expense}");
            Console.WriteLine($"Разница: {diff}");
        }
    }
}