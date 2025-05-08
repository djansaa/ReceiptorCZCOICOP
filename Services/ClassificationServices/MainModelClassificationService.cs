using ReceiptorCZCOICOP.Models;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using Tokenizers.DotNet;
using TorchSharp;
using static TorchSharp.torch;
using static TorchSharp.torch.jit;

namespace ReceiptorCZCOICOP.Services.ClassificationServices
{
    /// <summary>
    /// MainModelClassificationService is a service that uses a pre-trained model to classify product names into COICOP categories.
    /// </summary>
    internal class MainModelClassificationService : IClassificationService
    {
        private readonly Tokenizer _tokenizer;
        private readonly ScriptModule _model;
        private readonly Device _device;
        private readonly string[] _labelDict;

        public MainModelClassificationService()
        {
            var baseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "classifierdata");

            // paths to model, tokenizer and label encodings
            var modelPath = Path.Combine(baseDir, "model.pt");
            var tokenizerPath = Path.Combine(baseDir, "tokenizer.json");
            var labelPath = Path.Combine(baseDir, "model_label_encoding.json");

            if (!File.Exists(modelPath)) throw new FileNotFoundException("Model file not found", modelPath);
            if (!File.Exists(tokenizerPath)) throw new FileNotFoundException("Tokenizer file not found", tokenizerPath);
            if (!File.Exists(labelPath)) throw new FileNotFoundException("Label encodings file not found", labelPath);

            // load tokenizer
            _tokenizer = new Tokenizer(vocabPath: tokenizerPath);

            // load device (CUDA or CPU)
            _device = cuda.is_available() ? CUDA : CPU;

            // load model
            _model = torch.jit.load(modelPath).to(_device);
            _model.eval();

            // load label encodings for model output (coicop encodings)
            var labelJson = File.ReadAllText(labelPath);
            var labelDict = JsonSerializer.Deserialize<Dictionary<string, string>>(labelJson) ?? throw new InvalidOperationException("Invalid label_encodings.json");
            _labelDict = labelDict
                .OrderBy(kv => int.Parse(kv.Key))
                .Select(kv => kv.Value)
                .ToArray();
        }

        /// <summary>
        /// Classifies a product name into a COICOP category using the pre-trained model.
        /// </summary>
        /// <param name="productName"></param>
        /// <returns>classification output</returns>
        public Task<ClassificationServiceOutput> ClassifyAsync(string productName) =>
        Task.Run(() =>
        {
            var output = new ClassificationServiceOutput();

            try
            {
                // preprocess product name
                var preprocessedProductName = PreprocessProductName(productName);

                // encode product name via tokenizer
                var rawIds = _tokenizer.Encode(preprocessedProductName).ToArray();
                var ids = new long[32];

                // manual masking (.net Tokenizer does not support masking)
                var mask = new long[32];
                for (int i = 0; i < 32; i++)
                {
                    if (i < rawIds.Length)
                    {
                        ids[i] = rawIds[i];
                        mask[i] = 1;
                    }
                    else
                    {
                        ids[i] = 0;
                        mask[i] = 0;
                    }
                }

                // convert to tensors
                using var idsTensor = tensor(ids, dtype: ScalarType.Int64, device: _device).reshape(1, 32);
                using var maskTensor = tensor(mask, dtype: ScalarType.Int64, device: _device).reshape(1, 32);

                // run model
                using var logits = ((Tensor)_model.call(idsTensor, maskTensor)).to(CPU);

                // get probabilities from results
                using var probs = softmax(logits, dim: 1);
                var scores = probs.data<float>().ToArray();

                // get best score and corresponding index
                int bestIdx = Array.IndexOf(scores, scores.Max());

                // get COICOP code and confidence
                output.Coicop = _labelDict[bestIdx];
                output.Confidence = (decimal)scores[bestIdx];

                return output;

            } catch
            {
                return output;
            }
        });

        /// <summary>
        /// Preprocesses the product name for classification.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>preprocessed product name</returns>
        public static string PreprocessProductName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // lower + trim
            var txt = name.ToLowerInvariant().Trim();

            // replace dimension with units
            txt = Regex.Replace(txt, @"\b\d+(?:x\d+)+\s*(?:mm|cm|m)\b", " DIMN ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // replace dimension without units
            txt = Regex.Replace(txt, @"\b\d+(?:\s*x\s*\d+)+\b", " DIMN ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // replace units
            txt = Regex.Replace(
                txt,
                @"(?:\d+(?:[,.]\d+)?\s*(?:x\s*\d+(?:[,.]\d+)?\s*)?\s*|(?!\d)\b)(ml|l|mg|gr|g|ks|kg|pck|mm|cm|tbl)\b",
                m => UnitReplacement(m.Groups[1].Value),
                RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // replace percentage
            txt = Regex.Replace(txt, @"\b\d+\s*%(?=$|\s)", " PRCNT ", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            // delete punctuation
            txt = Regex.Replace(txt, @"[\/\\\.,*?!+]", " ");

            // delete isolated number tokens
            txt = Regex.Replace(txt, @"(?:^|\s)(\d+)(?=\s|$)", " ");

            // collapse multiple whitespaces
            txt = Regex.Replace(txt, @"\s+", " ").Trim();

            // add query prefix prefix
            txt = "query: " + txt;

            return txt;
        }

        /// <summary>
        /// Replaces the unit in the product name with a code term.
        /// </summary>
        /// <param name="unit"></param>
        /// <returns>unit code term</returns>
        private static string UnitReplacement(string unit)
        {
            switch (unit.ToLowerInvariant())
            {
                case "ml":
                case "l":
                    return " LIQUID ";
                case "mg":
                case "gr":
                case "g":
                case "kg":
                    return " SOLID ";
                case "ks":
                case "pck":
                    return " PIECE ";
                case "mm":
                case "cm":
                    return " LENGTH ";
                case "tbl":
                    return " TBL ";
                default:
                    return $" {unit} ";
            }
        }
    }
}
