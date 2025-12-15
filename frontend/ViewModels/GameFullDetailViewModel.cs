using Revalis.Models;
using Revalis.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Revalis.ViewModels
{
    public class GameFullDetailViewModel : INotifyPropertyChanged
    {
        private GameDTO _game;
        public GameDTO Game 
        { 
            get => _game;
            set
            {
                _game = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<AchievementDTO> Achievements { get; }
        public ObservableCollection<WalkthroughLinkDTO> WalkthroughLinks { get; }

        public ICommand BackToListCommand { get; }
        public ICommand OpenWalkthroughCommand { get; }
        public event EventHandler<string> WalkthroughRequested;

        public GameFullDetailViewModel(GameDTO game, IEnumerable<WalkthroughLinkDTO> walkthroughs, ICommand backToListCommand)
        {
            Game = game;
            // Achievements = new ObservableCollection<AchievementDTO>(achievements ?? Enumerable.Empty<AchievementDTO>());
            WalkthroughLinks = new ObservableCollection<WalkthroughLinkDTO>(walkthroughs ?? Enumerable.Empty<WalkthroughLinkDTO>());
            BackToListCommand = backToListCommand;

            OpenWalkthroughCommand = new RelayCommand(url =>
            {
                WalkthroughRequested?.Invoke(this, url.ToString());
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) 
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class AchievementDTO
    {
        public string Title { get; set; }
        public int Progress { get; set; } // percentage
    }
}
