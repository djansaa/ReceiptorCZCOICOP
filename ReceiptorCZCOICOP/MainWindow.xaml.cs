using ReceiptorCZCOICOP.Models;
using ReceiptorCZCOICOP.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ReceiptorCZCOICOP
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the double-click event on the suggestions ListBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Suggestions_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (DataContext is MainViewModel vm
             && sender is ListBox lb
             && lb.SelectedItem is Suggestion suggestion)
            {
                // apply suggestion
                vm.ApplySuggestionCommand.Execute(suggestion);
            }
        }

        /// <summary>
        /// Handles the key down event for the TextBox in the ManualBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // on left ctrl show suggestions
            if (e.Key == Key.LeftCtrl && DataContext is MainViewModel vm)
            {
                if ((sender as FrameworkElement)?.DataContext is ItemViewModel ivm)
                {
                    vm.ToggleSuggestionsCommand.Execute(ivm);
                    e.Handled = true;
                }
            }
        }

        /// <summary>
        /// Handles the GotFocus event for the TextBox in the ManualBox.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ManualBox_GotFocus(object sender, RoutedEventArgs e)
        {
            var mainVm = DataContext as MainViewModel;
            if (mainVm is null) return;

            // clear suggestions
            mainVm.Suggestions.Clear();

            // set the suggestion target
            var itemVm = (sender as TextBox)?.DataContext as ItemViewModel;
            mainVm.SetSuggestionTarget(itemVm);
        }
    }
}