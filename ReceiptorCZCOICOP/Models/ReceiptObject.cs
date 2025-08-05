using System.Windows.Media;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;

namespace ReceiptorCZCOICOP.Models
{
    /// <summary>
    /// Represents a receipt object that contains the original and preprocessed images, as well as the OCR text and classification result.
    /// </summary>
    internal class ReceiptObject
    {
        public Mat? OriginalMat { get; set; }

        public Mat? PreprocessedMat { get; set; }

        public ImageSource OriginalImageSource => OriginalMat != null ? BitmapSourceConverter.ToBitmapSource(OriginalMat) : null!;

        public ImageSource PreprocessedImageSource => PreprocessedMat != null ? BitmapSourceConverter.ToBitmapSource(PreprocessedMat) : null!;

        public bool BeingProcessed { get; set; }
        public bool IsSelected { get; set; } = false;

        public Receipt Receipt { get; set; } = new Receipt();

        public string? RawOcrText { get; set; }
    }
}
