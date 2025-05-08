# ReceiptorCZCOICOP

**ReceiptorCZCOICOP** is a desktop WPF application designed for automated receipt processing.  
It loads receipt images, performs OCR, extracts structured data, classifies items into COICOP categories, and allows exporting data to CSV or saving it into a SQLite database. In addition, manual editing and correction of extracted items is supported, including COICOP suggestion and override functionality.

---

## Key Features

- **OCR**  
  Uses Tesseract (implemented via `TesseractOcrService`) to extract text from scanned receipts. `TesseractOcrService` uses the `tessdata/` folder as its language data directory.

- **Data Extraction**  
  Calls an external service (e.g., GroqCloud) (implemented via `GroqCloudDataExtractionService`) to extract structured information from OCR result such as shop name, date, total price, items and prices via LLM models.

- **Item Classification**  
  Each extracted item is preprocessed (using regex and tokenization) and passed through a TorchSharp model (`model.pt`) (implemented via `MainModelClassificationService`) to predict a COICOP code.

- **COICOP Suggestions via Dictionary**  
  A local CSV dictionary (`Assets/coicop_dict.csv`) is used to search for similar names using Levenshtein distance and offer alternative COICOP suggestions.

- **Data Export**  
  You can export the data as a CSV file (implemented via `CsvDataExportService`) or save it directly into a SQLite database (implemented via `ReceiptDbContext`). A simple one-table implementation (`receipts.db`) is included as an example within the project, but it is strongly recommended to define your own database connection and schema tailored to your application's needs.

- **MVVM Architecture + Dependency Injection**  
  The app uses the .NET Generic Host. Services (OCR, extraction, classification, export, DbContext) are registered in `App.xaml.cs` and injected into `MainViewModel`.

- **Extensible Service Interfaces**  
  The application defines service interfaces like `IOcrService`, `IDataExtractionService`, `IClassificationService`, and `IDataExportService`, which you can implement to provide your own custom logic for OCR, data extraction, classification, or export. Then you can register these implementations into .NET Generic Host app in `App.xaml.cs`.

---

## Before Start
- **Important**: Before building the project, unzip the `.7z` archive files in the `classifierdata` folder. These files contain the trained COICOP classification model (`model.pt`) used by `MainModelClassificationService`.  
- **Important**: If you use the default `GroqCloudDataExtractionService`, you must define an environment variable `GROQCLOUD_API_KEY` containing your GroqCloud API key. This is required for the `GroqCloudDataExtractionService` to function correctly.  

---

## Used External Packages

| Package                                                               | Version   | Description                                                     | License    |
|-----------------------------------------------------------------------|-----------|-----------------------------------------------------------------|------------|
| [GroqSharp](https://github.com/Sarel-Esterhuizen/GroqSharp)           | 1.1.2     | C# client library for interacting with GroqCloud                | [MIT](license_mit.txt)        |
| [OpenCvSharp4](https://github.com/shimat/opencvsharp)                 | 4.10.0    | .NET wrapper for OpenCV (image processing)                      | [Apache 2.0](license_apache.txt) |
| [Tesseract](https://github.com/charlesw/tesseract)                    | 5.2.0     | .NET wrapper for tesseract-ocr 5.2.0                            | [Apache 2.0](license_apache.txt) |
| [Tokenizers.DotNet](https://github.com/sappho192/Tokenizers.DotNet)   | 1.2.0     | .NET wrapper of HuggingFace Tokenizers library                  | [MIT](license_mit.txt)        |
| [TorchSharp](https://github.com/dotnet/TorchSharp)                    | 0.105.0   | .NET library that provides access to the library behind PyTorch | [MIT](license_mit.txt)        |



---

## Requirements

- [.NET 8.0](https://dotnet.microsoft.com/) or newer
- Windows OS
- If you want to use the default `GroqCloudDataExtractionService`, you must define an environment variable `GROQCLOUD_API_KEY` containing your GroqCloud API key. More information about the GroqCloud API can be found on official site https://console.groq.com/.
