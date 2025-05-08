using ReceiptorCZCOICOP.Models;

namespace ReceiptorCZCOICOP.ViewModels
{
    /// <summary>
    /// ViewModel for an item in a receipt.
    /// </summary>
    internal class ItemViewModel : BaseViewModel
    {
        public Item Model { get; }
        public ItemViewModel(Item item) => Model = item;

        public string? Name { get => Model.Name; set { Model.Name = value; OnPropertyChanged(); } }
        public string ManualName
        {
            get => Model.ManualName ?? string.Empty;
            set
            {
                if (Model.ManualName != value)
                {
                    if (!string.IsNullOrEmpty(Model.Coicop) && Model.ManualName is null)
                    {
                        Model.Coicop = string.Empty;
                        OnPropertyChanged(nameof(Coicop));
                        Model.Confidence = 0;
                        OnPropertyChanged(nameof(Confidence));
                    }

                    Model.ManualName = value;
                    OnPropertyChanged(nameof(ManualName));
                }
            }
        }
        public float Value { get => Model.Value; set { Model.Value = value; OnPropertyChanged(); } }
        public string Coicop
        {
            get => Model.Coicop ?? string.Empty;
            set
            {
                if (Model.Coicop != value)
                {
                    Model.Coicop = value;
                    OnPropertyChanged(nameof(Coicop));
                    if (string.IsNullOrEmpty(value))
                    {
                        Model.Confidence = -1;
                        OnPropertyChanged(nameof(Confidence));
                    }
                }
            }
        }
        public decimal? Confidence { get => Model.Confidence; set { Model.Confidence = value; OnPropertyChanged(); } }
    }
}
