using Revalis;
using Revalis.Models;
using Revalis.ViewModels;
using System.Runtime.Intrinsics.X86;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;


namespace Revalis.Views
{
    /// <summary>
    /// Interaction logic for GameListView.xaml
    /// </summary>
    public partial class GameListView : UserControl
    {
        public GameListView()
        {
            InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (DataContext is GameListViewModel vm)
            {
                vm.SearchCompleted -= OnSearchCompleted;
                vm.SearchCompleted += OnSearchCompleted;
            }
        }
        private void OnSearchCompleted()
        {
            if (DataContext is GameListViewModel vm)
            {
                // Signal to ViewModel that scroll should reset
                vm.RequestScrollToTop = true;
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T tChild)
                    return tChild;

                var result = FindVisualChild<T>(child);
                if (result != null)
                    return result;
            }
            return null;
        }
    }
}
