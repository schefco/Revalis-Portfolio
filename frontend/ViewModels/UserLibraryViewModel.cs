using Revalis.Models;
using Revalis.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace Revalis.ViewModels
{
    public class UserLibraryViewModel : INotifyPropertyChanged
    {
        private readonly MainWindowViewModel _mainVM;
        public ObservableCollection<GameDTO> MyGames { get; }
        public ICommand RemoveGameCommand { get; }
        public ICommand OpenDetailCommand { get; }

        public UserLibraryViewModel(MainWindowViewModel mainVM)
        {
            _mainVM = mainVM;
            MyGames = new ObservableCollection<GameDTO>();
            RemoveGameCommand = new RelayCommand(RemoveGame);
            OpenDetailCommand = new RelayCommand(OpenDetail);
        }

        private async void OpenDetail(object parameter)
        {
            if (parameter is GameDTO game)
            {
                _mainVM.ShowGameFullDetail(game);
            }
        }

        public void AddGame(GameDTO game)
        {
            if (game != null && !MyGames.Contains(game))
                MyGames.Add(game);
        }

        private void RemoveGame(object parameter)
        {
            if (parameter is GameDTO game && MyGames.Contains(game))
                MyGames.Remove(game);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
