using OpenCvSharp;

namespace ReceiptorCZCOICOP.Models
{
    /// <summary>
    /// Represents the output of the OCR service.
    /// </summary>
    internal class OcrServiceOutput
    {
        public string? RawOcrText { get; set; } = null;
        public Mat? PreprocessedMat { get; set; } = null;
        public Mat? OriginalCroppedMat { get; set; } = null;
    }
}
