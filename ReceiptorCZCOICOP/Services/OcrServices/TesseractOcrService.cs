using OpenCvSharp;
using ReceiptorCZCOICOP.Models;
using System.IO;
using Tesseract;

namespace ReceiptorCZCOICOP.Services.OcrServices
{
    /// <summary>
    /// TesseractOcrService is a service that uses Tesseract OCR engine to perform OCR on images.
    /// </summary>
    internal class TesseractOcrService : IOcrService
    {
        private readonly TesseractEngine _engine;
        public TesseractOcrService()
        {
            var tessDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");

            // load tess data and config tesseract engine
            _engine = new TesseractEngine(tessDataPath, "ces", EngineMode.LstmOnly);
            _engine.DefaultPageSegMode = PageSegMode.SingleBlock;
        }

        /// <summary>
        /// Performs OCR on the provided image using Tesseract OCR engine.
        /// </summary>
        /// <param name="img">receipt image</param>
        /// <returns>ocr service output</returns>
        public Task<OcrServiceOutput> OcrAsync(Mat img) =>
        Task.Run(() =>
        {
            var output = new OcrServiceOutput();
            try
            {
                var mats = PreprocessMat(img);

                output.OriginalCroppedMat = mats.Item2.Clone();
                output.PreprocessedMat = mats.Item1.Clone();

                Cv2.ImEncode(".jpg", mats.Item1, out byte[] imgBytes);

                using Pix pix = Pix.LoadFromMemory(imgBytes);

                using var page = _engine.Process(pix);

                output.RawOcrText = page.GetText();

                return output;
            } catch
            {
                return output;
            }
        });

        /// <summary>
        /// Preprocesses the image for OCR.
        /// </summary>
        /// <param name="src">receipt image</param>
        /// <returns>original and preprocessed receipt image</returns>
        private Tuple<Mat,Mat> PreprocessMat(Mat src)
        {
            // grayscale
            var gray = new Mat();
            Cv2.CvtColor(src, gray, ColorConversionCodes.BGR2GRAY);

            // thresholding
            Mat thresh;
            if (Cv2.Mean(gray).Val0 < 242)  // is_dark_receipt
            {
                thresh = new Mat();
                Cv2.Threshold(gray, thresh, 77, 255, ThresholdTypes.Binary);
            }
            else
            {
                thresh = new Mat();
                Cv2.Threshold(gray, thresh, 0, 255, ThresholdTypes.Binary | ThresholdTypes.Otsu);
            }

            // deskew
            Mat nonZeroMat = new Mat();
            Cv2.FindNonZero(thresh, nonZeroMat);

            var pts = new OpenCvSharp.Point[nonZeroMat.Rows];
            for (int i = 0; i < nonZeroMat.Rows; i++)
            {
                var v = nonZeroMat.At<Vec2i>(i, 0);
                pts[i] = new OpenCvSharp.Point(v.Item0, v.Item1);
            }

            var box = Cv2.MinAreaRect(pts);
            double angle = box.Angle < -45 ? box.Angle + 90 : box.Angle;
            var M = Cv2.GetRotationMatrix2D(box.Center, angle, 1);
            var deskewed = new Mat();
            Cv2.WarpAffine(thresh, deskewed, M, thresh.Size(), InterpolationFlags.Linear, BorderTypes.Constant, new Scalar(255, 255, 255));

            // gaussian blur
            var blurred = new Mat();
            Cv2.GaussianBlur(thresh, blurred, new OpenCvSharp.Size(3, 3), 0);

            // opening
            var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(3, 3));
            var denoised = new Mat();
            Cv2.MorphologyEx(blurred, denoised, MorphTypes.Open, kernel);

            // crop
            var inv = new Mat();
            Cv2.BitwiseNot(denoised, inv);
            var vKernel = Cv2.GetStructuringElement(MorphShapes.Rect, new OpenCvSharp.Size(5, 20));
            var dilated = new Mat();
            Cv2.Dilate(inv, dilated, vKernel, iterations: 14);

            Cv2.FindContours(dilated, out var contours, out _, RetrievalModes.External, ContourApproximationModes.ApproxSimple);
            var c = contours
                .OrderByDescending(cn => Cv2.ContourArea(cn))
                .FirstOrDefault();
            if (c != null)
            {
                var r = Cv2.BoundingRect(c);
                return new Tuple<Mat,Mat>(new Mat(denoised, r), new Mat(src, r));
            }

            // if no contours found, return the original image
            return new Tuple<Mat, Mat>(denoised, src);
        }
    }
}
