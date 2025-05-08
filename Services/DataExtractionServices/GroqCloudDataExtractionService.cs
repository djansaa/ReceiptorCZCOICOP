using GroqSharp;
using GroqSharp.Models;
using ReceiptorCZCOICOP.Models;
using System.Text.Json;

namespace ReceiptorCZCOICOP.Services.DataExtractionServices
{
    /// <summary>
    /// GroqCloudDataExtractionService is a service that uses Groq Cloud to extract data from receipts.
    /// </summary>
    internal class GroqCloudDataExtractionService : IDataExtractionService
    {
        private readonly IGroqClient _groq;
        public GroqCloudDataExtractionService()
        {
            // check if the environment variable is set
            var apiKey = Environment.GetEnvironmentVariable("GROQCLOUD_API_KEY") ?? throw new InvalidOperationException("GROQ_API_KEY is not defined in environment variables");

            // set the model
            var apiModel = "llama-3.3-70b-versatile";

            // create the Groq client
            _groq = new GroqClient(apiKey, apiModel).SetTemperature(0); // set temperature to 0 is necessary for the JSON output
        }

        /// <summary>
        /// Extracts data from the provided receipt text using Groq Cloud.
        /// </summary>
        /// <param name="receiptText">ocred receipt text</param>
        /// <returns>list of receipts</returns>
        public async Task<Receipt?> ExtractDataAsync(string receiptText)
        {
            var jsonStructure = GetJsonStructure();

            try
            {
                var response = await _groq.GetStructuredChatCompletionAsync(jsonStructure,
                    new Message { Role = MessageRoleType.System, Content = GetSystemPrompt() },
                    new Message { Role = MessageRoleType.User, Content = GetUserPrompt(receiptText) }
                );

                if (response == null || response.Length == 0)
                {
                    return null;
                }

                var jsonResponse = CleanJson(response);
                var receipt = ConvertResponseToReceipt(jsonResponse);

                return receipt;

            } catch (Exception ex)
            {
                return null;
            }

        }

        /// <summary>
        /// Returns the system prompt for the Groq Cloud API.
        /// </summary>
        /// <returns>system prompt</returns>
        private string GetSystemPrompt()
        {
            return @"You are a receipt parser. You will be provided with unstructured receipt data, and your task is to parse it into JSON format.
            For example input will be:
            TESCO as.
            25/12/2025
            Ulice, 12.Praha
            Kč
            sprchovy gel 200 ml 89.90
            rohlik 12g 12.90
            rohlik 12g 12.90
            sleva jogurt -5.0
            -----------------
            cena celkem 110.70

            Your output must be:
            {
                'company': 'tesco',
                'date': '2025-12-25',
                'currency': 'CZK',
                'total': 110.70,
                'items': [
                    {
                        'name': 'sprchovy gel 200 ml',
                        'value': 89.90
                    }
                    {
                        'name': 'rohlik 12g',
                        'value': 12.90
                    },
                    {
                        'name': 'rohlik 12g',
                        'value': 12.90
                    },
                    {
                        'name': 'sleva jogurt',
                        'value': -5.0
                    }
                ]
            }.
            Include any discounts under 'items'.If an item has no value, leave it out. If a field's value cannot be found, set it to an empty string for text fields or zero for numeric fields. If the same item appears multiple times, list it multiple times. Convert date into YYYY-MM-DD format.";
        }

        /// <summary>
        /// Returns the user prompt for the Groq Cloud API.
        /// </summary>
        /// <param name="receiptText">ocred receipt text</param>
        /// <returns>user prompt</returns>
        private string GetUserPrompt(string receiptText)
        {
            return $"OCR receipt text:\n\n{receiptText}\n\n.";
        }

        /// <summary>
        /// Returns the JSON structure that the Groq Cloud should return.
        /// </summary>
        /// <returns>string json structure</returns>
        private string GetJsonStructure()
        {
            return @"
            {
                ""company"": ""string | The name of the company"",
                ""date"": ""string | The date of the receipt in YYYY-MM-DD format"",
                ""currency"": ""string | The currency code used (e.g. CZK)"",
                ""total"": ""float | The total amount of the receipt"",
                ""items"": [
                    {
                        ""name"": ""string | The product name"",
                        ""value"": ""float | The numeric product price""
                    }
                ]
            }";
        }

        /// <summary>
        /// Cleans the JSON response from Groq Cloud by removing leading/trailing whitespace and backticks.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private string CleanJson(string s)
        {
            // remove leading/trailing whitespace and back‐ticks
            s = s.Trim();

            if (s.StartsWith("```"))
            {
                var idx = s.IndexOf('\n');
                if (idx >= 0) s = s[(idx + 1)..];
            }
            // remove any trailing fence
            if (s.EndsWith("```")) s = s[..^3];
            return s.Trim();
        }

        /// <summary>
        /// Converts the JSON response from Groq Cloud to a Receipt object.
        /// </summary>
        /// <param name="jsonResponse">json response from groq cloud</param>
        /// <returns>receipt</returns>
        /// <exception cref="JsonException">invalid json format</exception>
        private Receipt ConvertResponseToReceipt(string jsonResponse)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            return JsonSerializer.Deserialize<Receipt>(jsonResponse, options) ?? throw new JsonException("Invalid JSON format");
        }
    }
}
