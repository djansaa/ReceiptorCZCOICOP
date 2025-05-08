using Microsoft.Win32;
using OpenCvSharp;
using ReceiptorCZCOICOP.Db;
using ReceiptorCZCOICOP.Helpers;
using ReceiptorCZCOICOP.Models;
using ReceiptorCZCOICOP.Services.ClassificationServices;
using ReceiptorCZCOICOP.Services.DataExportServices;
using ReceiptorCZCOICOP.Services.DataExtractionServices;
using ReceiptorCZCOICOP.Services.OcrServices;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace ReceiptorCZCOICOP.ViewModels
{
    /// <summary>
    /// ViewModel for the main window.
    /// </summary>
    internal class MainViewModel : BaseViewModel
    {
        public ObservableCollection<ReceiptObjectViewModel> ReceiptObjects { get; } = new ObservableCollection<ReceiptObjectViewModel>();

        // selected receipt property
        private ReceiptObjectViewModel? _selectedReceipt;
        public ReceiptObjectViewModel? SelectedReceipt
        {
            get => _selectedReceipt;
            set
            {
                // select new active receipt
                if (_selectedReceipt == value) return;

                foreach (var r in ReceiptObjects) r.IsSelected = false;

                _selectedReceipt = value;

                if (_selectedReceipt != null) _selectedReceipt.IsSelected = true;

                OnPropertyChanged();
            }
        }

        private ItemViewModel? _suggestionTarget;

        public ObservableCollection<Suggestion> Suggestions { get; } = new ObservableCollection<Suggestion>();

        // COMMANDS
        public ICommand LoadImagesCommand { get; }
        public ICommand LoadImagesAdditionalyCommand { get; }
        public ICommand AddItemAtEndCommand { get; }
        public ICommand AddItemAtCommand { get; }
        public ICommand RemoveItemCommand { get; }
        public ICommand SelectReceiptCommand { get; }
        public ICommand DeleteReceiptCommand { get; }
        public ICommand ToggleSuggestionsCommand { get; }
        public ICommand ApplySuggestionCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand SaveToDbCommand { get; }

        // SERVICES
        private readonly IOcrService _ocr;
        private readonly IDataExtractionService _extract;
        private readonly IClassificationService _classify;
        private readonly IDataExportService _export;
        private readonly ReceiptDbContext _db;

        public MainViewModel(IOcrService ocr, IDataExtractionService extract, IClassificationService classify, IDataExportService export, ReceiptDbContext db)
        {
            _ocr = ocr;
            _extract = extract;
            _classify = classify;
            _export = export;
            _db = db;

            // initialize commands
            LoadImagesCommand = new RelayCommand(async _ => await LoadReceiptsAsync(true));
            LoadImagesAdditionalyCommand = new RelayCommand(async _ => await LoadReceiptsAsync(false));
            AddItemAtEndCommand = new RelayCommand(_ => AddItemAtEnd());
            AddItemAtCommand = new RelayCommand(p => AddItemAt(p as Item));
            RemoveItemCommand = new RelayCommand(p => RemoveItem(p as Item));
            SelectReceiptCommand = new RelayCommand(p => SelectReceipt(p as ReceiptObjectViewModel));
            DeleteReceiptCommand = new RelayCommand(p => DeleteReceipt(p as ReceiptObjectViewModel));
            ToggleSuggestionsCommand = new RelayCommand(ToggleSuggestions);
            ApplySuggestionCommand = new RelayCommand(param => ApplySuggestion(param));
            ExportDataCommand = new RelayCommand(async _ => await ExportDataAsync(ReceiptObjects.Select(r => r.ReceiptVm.Model).ToList()));
            SaveToDbCommand = new RelayCommand(async _ => await SaveToDatabaseAsync(ReceiptObjects.Select(r => r.ReceiptVm.Model).ToList()));
        }

        /// <summary>
        /// Load receipts from files.
        /// </summary>
        /// <param name="clear">if new set of receipts are added - clear old receipts</param>
        /// <returns></returns>
        private async Task LoadReceiptsAsync(bool clear)
        {
            // file dialog - select .jpg files
            var dlg = new OpenFileDialog
            {
                Filter = "Images|*.jpg|All Files|*.*",
                Multiselect = true
            };
            if (dlg.ShowDialog() != true) return;

            if (clear) ReceiptObjects.Clear();

            var newVMs = new List<ReceiptObjectViewModel>();

            // load images
            foreach (var path in dlg.FileNames)
            {
                var model = new ReceiptObject
                {
                    OriginalMat = Cv2.ImRead(path, ImreadModes.Color),
                    BeingProcessed = true
                };
                var vm = new ReceiptObjectViewModel(model);
                ReceiptObjects.Add(vm);
                newVMs.Add(vm);
            }

            // process new added receipts
            foreach (var vm in newVMs)
            {
                await ProcessOneReceiptAsync(vm);
                vm.BeingProcessed = false;
            }
        }

        /// <summary>
        /// Process one receipt.
        /// </summary>
        /// <param name="vm">receipt view model</param>
        /// <returns></returns>
        private async Task ProcessOneReceiptAsync(ReceiptObjectViewModel vm)
        {
            try
            {
                // ocr
                var ocrOut = await _ocr.OcrAsync(vm.Model.OriginalMat!);
                vm.Model.OriginalMat = ocrOut.OriginalCroppedMat;
                vm.Model.PreprocessedMat = ocrOut.PreprocessedMat;
                vm.RawOcrText = ocrOut.RawOcrText!;

                // data extraction
                var receipt = await _extract.ExtractDataAsync(vm.RawOcrText);
                if (receipt != null)
                    vm.UpdateModel(receipt);      // metoda přidaná v ReceiptObjectViewModel

                // product names classification
                foreach (var itemVM in vm.ReceiptVm.Items)
                {
                    var output = await _classify.ClassifyAsync(itemVM.Name!);

                    // if confidence is high enough -> set the COICOP code
                    if (output != null && output.Confidence.HasValue && output.Confidence.Value >= 0.95m)
                    {
                        itemVM.Coicop = output.Coicop ?? string.Empty;
                        itemVM.Confidence = Math.Round(output.Confidence ?? 0, 3);
                    }

                    if (SelectedReceipt == null) SelectedReceipt = vm;
                }
            } catch (Exception ex)
            {
                MessageBox.Show($"Error processing receipt: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Add new item at the end of the receipt.
        /// </summary>
        private void AddItemAtEnd()
        {
            SelectedReceipt?.ReceiptVm.Items.Add(new ItemViewModel(new Item()));
        }

        /// <summary>
        /// Add new item at the specified position.
        /// </summary>
        /// <param name="itm">previous item</param>
        private void AddItemAt(Item? itm)
        {
            if (itm == null || SelectedReceipt == null) return;
            var items = SelectedReceipt.ReceiptVm.Items;
            int idx = items.ToList().FindIndex(ivm => ivm.Model == itm);
            if (idx >= 0) items.Insert(idx, new ItemViewModel(new Item()));
        }

        /// <summary>
        /// Remove item from the receipt.
        /// </summary>
        /// <param name="itm">actual item to be removed</param>
        private void RemoveItem(Item? itm)
        {
            if (itm == null || SelectedReceipt == null) return;
            var target = SelectedReceipt.ReceiptVm.Items.FirstOrDefault(ivm => ivm.Model == itm);
            if (target != null)
                SelectedReceipt.ReceiptVm.Items.Remove(target);
        }

        /// <summary>
        /// Select receipt from the list as active.
        /// </summary>
        /// <param name="vm">receipt view model</param>
        private void SelectReceipt(ReceiptObjectViewModel? vm)
        {
            if (vm != null)
            {
                SelectedReceipt = vm;
                Suggestions.Clear();
            }
        }

        /// <summary>
        /// Delete selected receipt.
        /// </summary>
        /// <param name="vm">actual receipt</param>
        private void DeleteReceipt(ReceiptObjectViewModel? vm)
        {
            if (vm != null)
            {
                if ((MessageBox.Show("Are you sure you want to delete this receipt?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes))
                {
                    ReceiptObjects.Remove(vm);
                    if (SelectedReceipt == vm) SelectedReceipt = null;
                    Suggestions.Clear();
                }
            }
        }

        /// <summary>
        /// Toggle suggestions for the selected item.
        /// </summary>
        /// <param name="p"></param>
        private void ToggleSuggestions(object? p)
        {
            if (p is ItemViewModel ivm)
            {
                _suggestionTarget = ivm;
                Suggestions.Clear();

                // search for suggestions
                foreach (var e in CoicopDictionary.Search(ivm.ManualName)) Suggestions.Add(new Suggestion() { Name = e.Name, Coicop = e.Coicop });
            }
        }

        /// <summary>
        /// Apply selected suggestion to the item.
        /// </summary>
        /// <param name="parameter">selected suggestion</param>
        private void ApplySuggestion(object? parameter)
        {
            if (parameter is Suggestion entry && _suggestionTarget != null)
            {
                _suggestionTarget.ManualName = entry.Name;
                _suggestionTarget.Coicop = entry.Coicop;
                _suggestionTarget.Confidence = -1;
                Suggestions.Clear();
            }
        }

        /// <summary>
        /// Set the target for suggestions.
        /// </summary>
        /// <param name="ivm">target item data for suggestion</param>
        public void SetSuggestionTarget(ItemViewModel? ivm)
        {
            _suggestionTarget = ivm;
        }

        /// <summary>
        /// Export data to CSV file.
        /// </summary>
        /// <param name="receipts">list of all receipts</param>
        /// <returns></returns>
        private async Task ExportDataAsync(List<Receipt> receipts)
        {
            if (receipts == null || receipts.Count == 0)
            {
                MessageBox.Show("No receipts to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // save file dialog
                var dlg = new SaveFileDialog
                {
                    Title = "Save receipts as…",
                    Filter = "CSV file|*.csv",
                    DefaultExt = ".csv",
                    FileName = $"receipts_{DateTime.Now:dd_MM_yyyy}",
                };
                if (dlg.ShowDialog() != true)
                    return;

                // filepath
                var full = dlg.FileName;
                var dir = Path.GetDirectoryName(full)!;
                var name = Path.GetFileNameWithoutExtension(full);

                // exporter
                await _export.ExportDataAsync(receipts, dir, name);

                MessageBox.Show($"Export completed successfully into: {dlg.FileName}.\n Row count: {receipts.Count}", "Export", MessageBoxButton.OK, MessageBoxImage.Information);

            } catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

        }

        /// <summary>
        /// THIS IS JUST EXAMPLE WITH ONE TABLE - RECEIPTS.
        /// Save receipts to the database.
        /// </summary>
        /// <param name="receipts">list of all receipts</param>
        /// <returns></returns>
        private async Task SaveToDatabaseAsync(List<Receipt> receipts)
        {
            if (receipts == null || receipts.Count == 0)
            {
                MessageBox.Show("No receipts to save.", "Database Save", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var receiptRows = receipts
                    .SelectMany(r => r.Items.Select(item => new ReceiptDbModel
                    {
                        Company = r.Company ?? string.Empty,
                        Date = r.Date.HasValue ? r.Date.Value.ToString("dd_MM_yyyy") : string.Empty,
                        Currency = r.Currency ?? string.Empty,
                        Product = item.Name ?? string.Empty,
                        Price = (decimal)item.Value,
                        Coicop = item.Coicop ?? string.Empty
                    }))
                    .ToList();

                await _db.Receipts.AddRangeAsync(receiptRows);
                await _db.SaveChangesAsync();

                MessageBox.Show($"All receipts have been successfully saved to the database.\nRow count: {receiptRows.Count}", "Database Save", MessageBoxButton.OK, MessageBoxImage.Information);

            } catch (Exception ex)
            {
                MessageBox.Show($"Error saving to database: {ex.Message}", "Database Save Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }
    }
}
