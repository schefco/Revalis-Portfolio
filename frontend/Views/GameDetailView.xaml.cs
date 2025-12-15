using Revalis.Models;
using System.Windows;
using System.Windows.Controls;

namespace Revalis.Views
{
    /// <summary>
    /// Interaction logic for GameDetailView.xaml
    /// </summary>
    public partial class GameDetailView : UserControl
    {
        public GameDetailView()
        {
            InitializeComponent();
        }

        public GameDTO SelectedGame
        {
            get { return (GameDTO)GetValue(SelectedGameProperty); }
            set { SetValue(SelectedGameProperty, value); }
        }

        public static readonly DependencyProperty SelectedGameProperty =
            DependencyProperty.Register(nameof(SelectedGame), typeof(GameDTO), typeof(GameDetailView));
    }
}
