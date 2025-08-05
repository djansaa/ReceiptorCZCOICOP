using ReceiptorCZCOICOP.Services.ClassificationServices;
using System.Threading.Tasks;
using Xunit;

public class MainModelClassificationServiceTests
{
    [Fact]
    public async Task ClassifyAsync_ReturnsClassificationServiceOutput()
    {
        // classificator service
        var service = new MainModelClassificationService();
        var productName = "Test Product";

        // classify product name
        var result = await service.ClassifyAsync(productName);

        // assert
        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Coicop));
        Assert.True(result.Confidence > 0);
    }

    [Theory]
    [InlineData("Sprchovy gel 200 ml", "query: sprchovy gel LIQUID")]
    [InlineData("Rohlik 12g", "query: rohlik SOLID")]
    [InlineData("Mleko 1l", "query: mleko LIQUID")]
    [InlineData("Deska drevo. 15x20x35mm", "query: deska drevo DIMN")]
    public void PreprocessProductName_TransformsInput(string input, string output)
    {
        // preprocess product name
        var result = MainModelClassificationService.PreprocessProductName(input);

        // assert
        Assert.Equal(result, output);
    }

    [Theory]
    [InlineData("ml", " LIQUID ")]
    [InlineData("kg", " SOLID ")]
    [InlineData("ks", " PIECE ")]
    [InlineData("mm", " LENGTH ")]
    [InlineData("tbl", " TBL ")]
    [InlineData("custom", " custom ")] // default case
    public void UnitReplacement_TransformsSingleUnitsCorrectly(string input, string expected)
    {
        // test private method
        var result = typeof(MainModelClassificationService)
            .GetMethod("UnitReplacement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
            .Invoke(null, new object[] { input }) as string;

        // assert
        Assert.Equal(expected, result);
    }
}