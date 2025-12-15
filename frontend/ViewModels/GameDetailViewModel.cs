using Revalis.Models;
using System.ComponentModel;
using System.Windows.Input;

namespace Revalis.ViewModels
{
    public class GameDetailViewModel : INotifyPropertyChanged
    {
        private readonly Action _navigateBack;
        public string Name { get; set; }
        public double? Rating { get; set; }
        public string Released { get; set; }
        public string Developer { get; set; }
        public string Description { get; set; }
        public double Progress { get; set; }
        public List<string> Achievements { get; set; }
        public string Notes { get; set; }
        public ICommand BackCommand { get; }

        public GameDetailViewModel(GameDTO game, Action navigateBack)
        {
            Name = game.Name;
            Rating = game.Rating;
            Released = game.Released;
            Developer = "Placedholder";
            Description = "Placeholder";
            Progress = 66;
            Achievements = new List<string>() { "Campaign", "Easter Eggs", "Multiplayer" };
            Notes = "Write a note ...";

            BackCommand = new RelayCommand(_ => _navigateBack?.Invoke());
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
