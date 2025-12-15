using Revalis.Models;
using Revalis.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.PropertyGrid.Converters;

namespace Revalis.ViewModels
{
    public class MainWindowViewModel : BaseViewModel
    {
        private readonly MainWindow _mainWindow;
        private readonly ApiService _apiService;
        private readonly UserLibraryViewModel _userLibraryVM;

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set
            {
                _currentView = value;
                OnPropertyChanged();
            }
        }

        public ICommand CloseCommand { get; }
        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand SaveDbCommand { get; }
        public ICommand ImportDbCommand { get; }
        public ICommand OpenUserLibraryCommand { get; }
        public ICommand SearchRevalisCommand { get; }
        public ICommand LinkXboxCommand { get; }
        public ICommand LinkPlayStationCommand { get; }
        public ICommand LinkSteamCommand { get; }

        public MainWindowViewModel(ApiService apiService, MainWindow mainWindow)
        {
            _apiService = apiService;
            _mainWindow = mainWindow;

            _userLibraryVM = new UserLibraryViewModel(this);

            // Title Bar
            CloseCommand = new RelayCommand(_ => _mainWindow.Close());
            MinimizeCommand = new RelayCommand(_ => Application.Current.MainWindow.WindowState = WindowState.Minimized);
            MaximizeCommand = new RelayCommand(_ =>
            {
                var win = Application.Current.MainWindow;
                if (win.WindowState == WindowState.Maximized)
                {
                    win.WindowState = WindowState.Normal;
                }
                else
                {
                    // Manually size to working area so taskbar is respected
                    var workingArea = SystemParameters.WorkArea;
                    win.WindowState = WindowState.Normal; // reset first
                    win.Left = workingArea.Left;
                    win.Top = workingArea.Top;
                    win.Width = workingArea.Width;
                    win.Height = workingArea.Height;
                }
            });

            // File menu
            SaveDbCommand = new RelayCommand(_ => SaveDatabase());
            ImportDbCommand = new RelayCommand(_ => ImportDatabase());

            // Library and Search
            OpenUserLibraryCommand = new RelayCommand(_ => ShowUserLibrary());
            SearchRevalisCommand = new RelayCommand(_ => ShowGameList());

            // Future integrations
            LinkXboxCommand = new RelayCommand(_ => MessageBox.Show("Xbox Live integration coming soon!"));
            LinkPlayStationCommand = new RelayCommand(_ => MessageBox.Show("PlayStation Plus integration coming soon!"));
            LinkSteamCommand = new RelayCommand(_ => MessageBox.Show("Steam integration coming soon!"));

            // Default view
            ShowGameList();
        }

        public void ShowGameList()
        {
            var vm = new GameListViewModel(_apiService, _mainWindow, AddGameToLibrary);
            CurrentView = new Views.GameListView { DataContext = vm };
        }

        private void ShowUserLibrary()
        {
            CurrentView = new Views.UserLibraryView { DataContext = _userLibraryVM };
        }

        public void ShowGameDetail(GameDTO game)
        {
            var vm = new GameDetailViewModel(game, ShowGameList);
            CurrentView = new Views.GameDetailView { DataContext = vm };
        }

        public async Task ShowGameFullDetail(GameDTO game)
        {
            // Load walkthroughs from API
            List<WalkthroughLinkDTO> walkthroughs = await _apiService.GetWalkthroughForGameAsync(game.Name);

            // Back button command returns to library
            ICommand backCommand = new RelayCommand(_ => ShowUserLibrary());

            // Create the detail view model
            var vm = new GameFullDetailViewModel(game, walkthroughs, backCommand);

            // Swap the view
            CurrentView = new GameFullDetailView { DataContext = vm };
        }

        private void SaveDatabase()
        {
            MessageBox.Show("Save DB not implemented yet.");
        }

        private void ImportDatabase()
        {
            MessageBox.Show("Import DB not implemented yet.");
        }

        public void AddGameToLibrary(GameDTO game)
        {
            _userLibraryVM.AddGame(game);
        }
    }
}
