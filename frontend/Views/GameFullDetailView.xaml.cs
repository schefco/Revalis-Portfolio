using Revalis.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Web.WebView2.Wpf;

namespace Revalis.Views
{
    /// <summary>
    /// Interaction logic for GameFullDetailView.xaml
    /// </summary>
    public partial class GameFullDetailView : UserControl
    {
        public GameFullDetailView()
        {
            InitializeComponent();

            if (DataContext is GameFullDetailViewModel vm)
            {
                vm.WalkthroughRequested += (s, url) =>
                {
                    InAppBrowser.Source = new Uri(url);
                };
            }
        }

        private void WalkthroughButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string url)
            {
                InAppBrowser.Source = new Uri(url);
            }
        }

        private void BrowserBack_Click(object sender, RoutedEventArgs e)
        {
            if (InAppBrowser.CoreWebView2 != null && InAppBrowser.CoreWebView2.CanGoBack)
                InAppBrowser.CoreWebView2.GoBack();
        }

        private void BrowserForward_Click(object sender, RoutedEventArgs e)
        {
            if (InAppBrowser.CoreWebView2 != null && InAppBrowser.CoreWebView2.CanGoForward)
                InAppBrowser.CoreWebView2.GoForward();
        }

        private void BrowserRefresh_Click(object sender, RoutedEventArgs e)
        {
            InAppBrowser.Reload();
        }

        // Helper method for walkthrough buttons
        public void NavigateBrowser(string url)
        {
            if (InAppBrowser.CoreWebView2 != null)
                InAppBrowser.CoreWebView2.Navigate(url);
            else
                InAppBrowser.Source = new Uri(url);
        }

    }
}
