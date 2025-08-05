using ReceiptorCZCOICOP.Models;
using ReceiptorCZCOICOP.Services.DataExtractionServices;
using System;
using System.Reflection;
using Xunit;

public class GroqCloudDataExtractionServiceTests
{
    private readonly object _serviceInstance;
    private readonly Type _serviceType;

    public GroqCloudDataExtractionServiceTests()
    {
        _serviceType = typeof(GroqCloudDataExtractionService);
        _serviceInstance = Activator.CreateInstance(_serviceType, nonPublic: true)!;
    }

    [Fact]
    public void GetSystemPrompt_ReturnsText()
    {
        var method = _serviceType.GetMethod("GetSystemPrompt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_serviceInstance, null)!;
        Assert.Contains("You are a receipt parser", result);
    }

    [Fact]
    public void GetUserPrompt_WrapsInputCorrectly()
    {
        var method = _serviceType.GetMethod("GetUserPrompt", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_serviceInstance, new object[] { "abc" })!;
        Assert.Contains("OCR receipt text:", result);
        Assert.Contains("abc", result);
    }

    [Fact]
    public void GetJsonStructure_ReturnsExpectedSchema()
    {
        var method = _serviceType.GetMethod("GetJsonStructure", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_serviceInstance, null)!;
        Assert.Contains("\"company\"", result);
        Assert.Contains("\"items\"", result);
    }

    [Theory]
    [InlineData("```json\n{\"company\":\"X\"}\n```", "{\"company\":\"X\"}")]
    [InlineData("  {\"company\":\"Y\"}  ", "{\"company\":\"Y\"}")]
    public void CleanJson_RemovesFormatting(string input, string expected)
    {
        var method = _serviceType.GetMethod("CleanJson", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var result = (string)method.Invoke(_serviceInstance, new object[] { input })!;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ConvertResponseToReceipt_ParsesJsonCorrectly()
    {
        var method = _serviceType.GetMethod("ConvertResponseToReceipt", BindingFlags.NonPublic | BindingFlags.Instance)!;

        string json = """
        {
            "company": "Test Co",
            "date": "2023-10-01",
            "currency": "USD",
            "total": 30.5,
            "items": [
                { "name": "Item A", "value": 10.0 },
                { "name": "Item B", "value": 20.5 }
            ]
        }
        """;

        var result = (Receipt)method.Invoke(_serviceInstance, new object[] { json })!;
        Assert.Equal("Test Co", result.Company);
        Assert.Equal("USD", result.Currency);
        Assert.Equal(30.5f, result.Total);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Item A", result.Items[0].Name);
    }
}