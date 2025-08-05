using ReceiptorCZCOICOP.Models;
using ReceiptorCZCOICOP.Services.DataExportServices;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

public class CsvDataExportServiceTests
{
    [Fact]
    public async Task ExportDataAsync_WritesValidCsvFile()
    {
        // arrange
        var service = new CsvDataExportService();
        var receipts = new List<Receipt>
        {
            new Receipt
            {
                Company = "Test Company",
                Date = new System.DateTime(2023, 10, 1),
                Currency = "USD",
                Items = new List<Item>
                {
                    new Item { Name = "Product A", Value = 10.5f, Coicop = "123" },
                    new Item { Name = "Product B", Value = 20.0f, Coicop = "456" }
                }
            }
        };
        var filePath = Path.GetTempPath();
        var fileName = "test_receipts_xunit";

        // export data
        await service.ExportDataAsync(receipts, filePath, fileName);

        // assert
        var fullPath = Path.Combine(filePath, $"{fileName}.csv");
        Assert.True(File.Exists(fullPath));
        var content = await File.ReadAllTextAsync(fullPath);
        Assert.Contains("Test Company", content);
        Assert.Contains("Product A", content);
        Assert.Contains("Product B", content);
        Assert.Contains("id,company,date,currency,product,price,coicop", content);

        // cleanup
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("Hello", "Hello")]
    [InlineData("a,b", "\"a,b\"")]
    [InlineData("multi\nline", "\"multi\nline\"")]
    [InlineData("quote\"test", "\"quote\"\"test\"")]
    public void EscapeForCsv_HandlesSpecialCharactersCorrectly(string? input, string expected)
    {
        // test private method
        var method = typeof(CsvDataExportService).GetMethod("EscapeForCsv", BindingFlags.NonPublic | BindingFlags.Static)!;

        var result = (string)method.Invoke(null, new object?[] { input })!;

        // assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task ExportDataAsync_WithEmptyList_WritesOnlyHeader()
    {
        // arrange
        var service = new CsvDataExportService();
        var filePath = Path.GetTempPath();
        var fileName = $"empty_export_test";
        var fullPath = Path.Combine(filePath, $"{fileName}.csv");

        // export data
        await service.ExportDataAsync(new List<Receipt>(), filePath, fileName);

        // assert
        Assert.True(File.Exists(fullPath));
        var content = await File.ReadAllTextAsync(fullPath);
        Assert.Equal("id,company,date,currency,product,price,coicop\r\n", content);

        // cleanup
        File.Delete(fullPath);
    }
}