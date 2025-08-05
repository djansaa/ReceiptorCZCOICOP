using OpenCvSharp;
using ReceiptorCZCOICOP.Services.OcrServices;
using System;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

public class TesseractOcrServiceTests
{
    [Fact]
    public async Task OcrAsync_ReturnsOcrServiceOutput()
    {
        var service = new TesseractOcrService();
        var img = new Mat(100, 100, MatType.CV_8UC3, new Scalar(255, 255, 255));

        var result = await service.OcrAsync(img);

        Assert.NotNull(result);
        Assert.NotNull(result.RawOcrText);
        Assert.NotNull(result.PreprocessedMat);
        Assert.NotNull(result.OriginalCroppedMat);
    }

    [Fact]
    public void PreprocessMat_ReturnsTupleWithProcessedMats()
    {
        var serviceType = typeof(TesseractOcrService);
        var service = Activator.CreateInstance(serviceType, nonPublic: true)!;

        var method = serviceType.GetMethod("PreprocessMat", BindingFlags.NonPublic | BindingFlags.Instance)!;
        var img = new Mat(100, 100, MatType.CV_8UC3, new Scalar(255, 255, 255));

        var result = (Tuple<Mat, Mat>)method.Invoke(service, new object[] { img })!;

        Assert.NotNull(result);
        Assert.IsType<Mat>(result.Item1); // Preprocessed
        Assert.IsType<Mat>(result.Item2); // Original cropped
        Assert.False(result.Item1.Empty());
        Assert.False(result.Item2.Empty());
    }

    [Fact]
    public async Task OcrAsync_HandlesEmptyMatSafely()
    {
        var service = new TesseractOcrService();
        var emptyImg = new Mat(); // empty image

        var result = await service.OcrAsync(emptyImg);

        Assert.NotNull(result); // should not throw
        Assert.Null(result.RawOcrText); // nothing detected
    }
}