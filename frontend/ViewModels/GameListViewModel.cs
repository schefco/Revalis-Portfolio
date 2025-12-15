using Revalis.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Revalis.ViewModels
{
    public class GameListViewModel : BaseViewModel, INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly MainWindow _mainWindow;
        private readonly Action<GameDTO> _addToLibrary;
        private int _offset = 0;
        private const int PageSize = 20;
        private bool _isLoading;
        private bool _hasMore = true;
        private bool _isDetailVisible;
        private object _currentView;
        private int _currentPage = 1;
        private bool _requestScrollToTop;

        private GameDTO _selectedGame;
        private ObservableCollection<IListItem> _games = new();
        private IListItem _selectedItem;

        public string SearchQuery { get; set; }
        public GenreDTO SelectedGenre { get; set; }
        public PlatformDTO SelectedPlatform { get; set; }
        public string SelectedSort { get; set; }
        public double? MinRating { get; set; }
        public double? MaxRating { get; set; }
        public event Action SearchCompleted;
        public int GridRowCount => (Games?.Count ?? 0 + 3) / 4;

        public ObservableCollection<GenreDTO> Genres { get; set; } = new();
        public ObservableCollection<PlatformDTO> Platforms { get; } = new();
        public ObservableCollection<string> SortOptions { get; } = new() { "", "Name", "Release Date", "Rating" };

        public ObservableCollection<IListItem> Games
        {
            get => _games;
            set
            {
                if (_games != value)
                {
                    if (_games != null)
                        _games.CollectionChanged -= Games_CollectionChanged;

                    _games = value;

                    if (_games != null)
                        _games.CollectionChanged += Games_CollectionChanged;

                    OnPropertyChanged(nameof(Games));
                    OnPropertyChanged(nameof(GridRowCount));
                }
            }
        }

        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(nameof(CurrentView)); }
        }

        private void Games_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            // Whenever items are added or removed, recalculate rows
            OnPropertyChanged(nameof(GridRowCount));
        }

        public GameDTO SelectedGame
        {
            get => _selectedGame;
            private set
            {
                if (_selectedGame != value)
                {
                    _selectedGame = value;
                    OnPropertyChanged(nameof(SelectedGame));
                    IsDetailVisible = _selectedGame != null;

                    // Notify commands that depend on SelectedGame
                    (UpdateGameCommand as RelayCommand)?.RaiseCanExecuteChanged();
                    (DeleteGameCommand as RelayCommand)?.RaiseCanExecuteChanged();
                }
            }
        }

        public IListItem SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (_selectedItem != value)
                {
                    _selectedItem = value;
                    OnPropertyChanged(nameof(SelectedItem));
                    SelectedGame = value as GameDTO;
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }

        public bool IsDetailVisible
        {
            get => _isDetailVisible;
            set
            {
                _isDetailVisible = value;
                OnPropertyChanged(nameof(IsDetailVisible));
            }
        }

        public bool RequestScrollToTop
        {
            get => _requestScrollToTop;
            set
            {
                if (_requestScrollToTop != value)
                {
                    _requestScrollToTop = value;
                    OnPropertyChanged(nameof(RequestScrollToTop));
                }
            }
        }

        public ICommand AddGameCommand { get; }
        public ICommand UpdateGameCommand { get; }
        public ICommand DeleteGameCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand CloseDetailCommand { get; }
        public ICommand ToggleDetailCommand { get; }
        public ICommand OpenFullDetailCommand { get; }
        public ICommand BackToListCommand { get; }
        public ICommand LoadMoreGamesCommand { get; }

        public GameListViewModel(ApiService apiService, MainWindow mainWindow, Action<GameDTO> addToLibrary)
        {
            _apiService = apiService;
            _mainWindow = mainWindow;
            _addToLibrary = addToLibrary;
            Games = new ObservableCollection<IListItem>();

            CurrentView = this;

            AddGameCommand = new RelayCommand(_ =>
            {
                if (SelectedGame != null)
                {
                    _addToLibrary(SelectedGame);
                }
            });

            UpdateGameCommand = new RelayCommand(async _ => await UpdateGame(), _ => SelectedGame != null);
            DeleteGameCommand = new RelayCommand(async _ => await DeleteGame(), _ => SelectedGame != null);
            SearchCommand = new RelayCommand(async _ => await SearchGames());

            CloseDetailCommand = new RelayCommand(_ =>
            {
                SelectedItem = null;
                SelectedGame = null;
                IsDetailVisible = false;
            });

            ToggleDetailCommand = new RelayCommand(_ =>
            {
                if (IsDetailVisible)
                {
                    SelectedItem = null;
                    SelectedGame = null;
                }
                IsDetailVisible = !IsDetailVisible;
            });

            LoadMoreGamesCommand = new RelayCommand(async _ => await OnLoadMoreGames());

            OpenFullDetailCommand = new RelayCommand(_ => OpenFullDetail());
            BackToListCommand = new RelayCommand(async _ => 
            {
                CurrentView = this;

                if (!string.IsNullOrEmpty(SearchQuery))
                    await SearchGames();
                else
                    await RefreshGames();
            });

            LoadGames();
            _ = LoadFilterAsync();
        }

        private async Task OpenFullDetail()
        {
            if (SelectedGame != null)
            {
                var walkthroughsList = await _apiService.GetWalkthroughForGameAsync(SelectedGame.Name);

                CurrentView = new GameFullDetailViewModel(SelectedGame, walkthroughsList, BackToListCommand);
            }
        }

        private async void LoadGames()
        {
            await RefreshGames();
        }

        private async Task AddGame()
        {
            var newGame = new GameDTO
            {
                Name = "New Game",
                Released = DateTime.Now.ToShortDateString(),
                Rating = 0,
                Platforms = new List<PlatformDTO> { new PlatformDTO { Id = 1, Name = "PC" } },
                Genres = new List<GenreDTO> { new GenreDTO { Id = 4, Name = "Action" } }
            };

            await _apiService.AddGameAsync(newGame);
            await RefreshGames();
        }

        private async Task UpdateGame()
        {
            if (SelectedGame == null) return;
            SelectedGame.Notes = "Updated from frontend";
            await _apiService.UpdateGameAsync(SelectedGame);
            await RefreshGames();
        }

        private async Task DeleteGame()
        {
            if (SelectedGame == null) return;
            await _apiService.DeleteGameAsync(SelectedGame.Id);
            await RefreshGames();
        }

        private async Task RefreshGames()
        {
            _offset = 0;
            _currentPage = 1;
            _hasMore = true;
            var games = await _apiService.GetGamesAsync(_currentPage, PageSize);
            Games.Clear();
            foreach (var g in games)
            {
                Games.Add(g);
            }
        }

        private async Task SearchGames()
        {
            // Reset pagination state
            _offset = 0;
            _hasMore = true;
            _currentPage = 1;
            Games.Clear();

            var results = await _apiService.SearchGamesAsync(SearchQuery, PageSize, _offset);
            foreach (var g in results)
            {
                Games.Add(g);
            }

            await LoadFilterAsync();

            SearchCompleted?.Invoke();

            RequestScrollToTop = true;
        }

        public async Task OnLoadMoreGames()
        {
            if (_isLoading || !_hasMore) return;

            IsLoading = true;
            Games.Add(new LoadingItem());

            try
            {
                var moreGames = string.IsNullOrEmpty(SearchQuery)
                    ? await _apiService.GetGamesAsync(PageSize, _offset)
                    : await _apiService.SearchGamesAsync(SearchQuery, PageSize, _offset);

                var loadingItem = Games.OfType<LoadingItem>().FirstOrDefault();
                if (loadingItem != null) Games.Remove(loadingItem);

                foreach (var g in moreGames)
                {
                    Games.Add(g);
                }

                if (moreGames.Count < PageSize)
                    _hasMore = false;

                _currentPage++;
                _offset += PageSize;
            }
            finally
            {
                _isLoading = false;
            }
        }

        private async Task LoadFilterAsync()
        {
            try
            {
                // -- Load Genres --
                Genres.Clear();
                var genres = await _apiService.GetGenresAsync();
                // -- Add option for All Genres
                Genres.Add(new GenreDTO { Id = 0, Name = "All" });
                foreach (var g in genres)
                {
                    Genres.Add(g);
                }

                // -- Load Platforms --
                Platforms.Clear();
                var platforms = await _apiService.GetPlatformsAsync();
                // -- Add option for All Platforms
                Platforms.Add(new PlatformDTO { Id = 0, Name = "All" });
                foreach (var p in platforms)
                {
                    Platforms.Add(p);
                }

                // -- Set defaults in filter drop downs --
                SelectedGenre = Genres.FirstOrDefault();
                SelectedPlatform = Platforms.FirstOrDefault();
            }
            catch (Exception ex)
            {
                // -- Handle the errors
                Console.WriteLine($"Error loading filters: {ex.Message}");
            }
        }

        private void OnGameSelected(GameDTO game)
        {
            // _mainWindow.ShowGameDetail(game);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
