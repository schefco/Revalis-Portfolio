using Revalis.ViewModels;
using System.Net.Http;
using System.Windows;
using System.Windows.Input;

namespace Revalis
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ApiService _apiService;
        private readonly MainWindow _mainWindow;
        public MainWindow()
        {
            InitializeComponent();

            var httpClient = new HttpClient { BaseAddress = new Uri("http://127.0.0.1:8000") };
            _apiService = new ApiService(httpClient);

            DataContext = new ViewModels.MainWindowViewModel(_apiService, this);

            Loaded += (_, _) =>
            {
                var workingArea = SystemParameters.WorkArea;
                this.Left = workingArea.Left;
                this.Top = workingArea.Top;
                this.Width = workingArea.Width;
                this.Height = workingArea.Height;
            };

            StateChanged += (_, _) => 
            {
                if (WindowState == WindowState.Maximized)
                {
                    var workingArea = SystemParameters.WorkArea;
                    this.Left = workingArea.Left;
                    this.Top = workingArea.Top;
                    this.Width = workingArea.Width;
                    this.Height = workingArea.Height;
                }
            };
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Toggle maximize/restore with taskbar-safe sizing
                var win = Application.Current.MainWindow;
                if (win.WindowState == WindowState.Maximized)
                {
                    win.WindowState = WindowState.Normal;
                }
                else
                {
                    var workingArea = SystemParameters.WorkArea;
                    win.WindowState = WindowState.Normal; // reset first
                    win.Left = workingArea.Left;
                    win.Top = workingArea.Top;
                    win.Width = workingArea.Width;
                    win.Height = workingArea.Height;
                }
            }
            else if (e.ButtonState == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}