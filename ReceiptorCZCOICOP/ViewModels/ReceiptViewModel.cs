using ReceiptorCZCOICOP.Models;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ReceiptorCZCOICOP.ViewModels
{
    /// <summary>
    /// ViewModel for a receipt.
    /// </summary>
    internal class ReceiptViewModel : BaseViewModel
    {
        public Receipt Model { get; }

        public ObservableCollection<ItemViewModel> Items { get; }

        public ReceiptViewModel(Receipt r)
        {
            Model = r;

            // create a collection of ItemViewModel from the receipt items
            Items = new ObservableCollection<ItemViewModel>(r.Items.Select(i => new ItemViewModel(i)));

            // add collection changed listener on Items
            Items.CollectionChanged += Items_CollectionChanged;

            // add vaůie changed listener on each item
            foreach (var ivm in Items) ivm.PropertyChanged += Item_PropertyChanged;

            // check total value
            CheckConsistency();
        }

        public string? Company
        {
            get => Model.Company;
            set
            {
                if (Model.Company != value)
                {
                    Model.Company = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime? Date
        {
            get => Model.Date;
            set
            {
                if (Model.Date != value)
                {
                    Model.Date = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Currency
        {
            get => Model.Currency;
            set
            {
                if (Model.Currency != value)
                {
                    Model.Currency = value;
                    OnPropertyChanged();
                }
            }
        }

        public float Total
        {
            get => Model.Total;
            set
            {
                if (Model.Total != value)
                {
                    Model.Total = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isTotalOk;
        public bool IsTotalOk
        {
            get => _isTotalOk;
            private set { _isTotalOk = value; OnPropertyChanged(); }
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            // add new item
            if (e.NewItems != null)
            {
                foreach (ItemViewModel ivm in e.NewItems) ivm.PropertyChanged += Item_PropertyChanged;
            }

            // delete item
            if (e.OldItems != null)
            {
                foreach (ItemViewModel ivm in e.OldItems) ivm.PropertyChanged -= Item_PropertyChanged;
                // check total value
                CheckConsistency();
                    
            }
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ItemViewModel.Value))
            {
                // recalculate total value
                OnPropertyChanged(nameof(Total));
                // check total value
                CheckConsistency();
            }
        }

        /// <summary>
        /// Checks if the total value of the receipt is consistent with the sum of the item values.
        /// </summary>
        private void CheckConsistency()
        {
            var sum = Items.Sum(i => i.Value);
            IsTotalOk = Math.Abs(sum - Model.Total) < 0.01;
        }
    }
}
