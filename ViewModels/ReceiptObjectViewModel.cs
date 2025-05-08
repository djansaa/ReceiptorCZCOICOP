using ReceiptorCZCOICOP.Models;
using System.Windows.Media;

namespace ReceiptorCZCOICOP.ViewModels
{
    /// <summary>
    /// ViewModel for a receipt object.
    /// </summary>
    internal class ReceiptObjectViewModel : BaseViewModel
    {
        public ReceiptObject Model { get; }
        public ImageSource OriginalImage => Model.OriginalImageSource;
        public ImageSource PreprocessedImage => Model.PreprocessedImageSource;
        public string? RawOcrText { get => Model.RawOcrText; set { Model.RawOcrText = value; OnPropertyChanged(); } }
        public bool BeingProcessed { get => Model.BeingProcessed; set { Model.BeingProcessed = value; OnPropertyChanged(); } }
        public bool IsSelected { get => Model.IsSelected; set { Model.IsSelected = value; OnPropertyChanged(); } }

        private ReceiptViewModel _receiptVm;
        public ReceiptViewModel ReceiptVm
        {
            get => _receiptVm;
            private set
            {
                if (_receiptVm != value)
                {
                    _receiptVm = value;
                    OnPropertyChanged();
                }
            }
        }

        public ReceiptObjectViewModel(ReceiptObject obj)
        {
            Model = obj;
            _receiptVm = new ReceiptViewModel(obj.Receipt);
        }

        public void UpdateModel(Receipt newReceipt)
        {
            Model.Receipt = newReceipt;
            ReceiptVm = new ReceiptViewModel(newReceipt);
        }
    }

}
