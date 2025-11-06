using System;
using System.IO;
using Components.Template;
using Domain.Entity;
using Xunit;

namespace FinanceTracker.Tests
{
    public class OperationCsvImporterTests
    {
        [Fact]
        public void Import_ValidCsv_ParsesOperations()
        {
            var csv = 
                "Id;Type;AccountId;CategoryId;Amount;Date;Description\n" +
                $"{Guid.NewGuid()};Income;{Guid.NewGuid()};{Guid.NewGuid()};100.5;2025-01-01;Salary\n" +
                $"{Guid.NewGuid()};Expense;{Guid.NewGuid()};{Guid.NewGuid()};50.0;2025-01-02;Coffee";

            var path = Path.GetTempFileName();
            File.WriteAllText(path, csv);

            try
            {
                var importer = new OperationCsvImporter();
                var result = importer.Import(path);

                Assert.Equal(2, result.Count);
                Assert.Contains(result, o => o.Type == MonyFlowOption.Income && o.Amount == 100.5m);
                Assert.Contains(result, o => o.Type == MonyFlowOption.Expense && o.Amount == 50.0m);
            }
            finally
            {
                File.Delete(path);
            }
        }
    }
}