using Revalis.Models;
using Revalis.ViewModels;
using System;
using System.Collections.Generic;
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

namespace Revalis.Views
{
    /// <summary>
    /// Interaction logic for UserLibraryView.xaml
    /// </summary>
    public partial class UserLibraryView : UserControl
    {
        public UserLibraryView()
        {
            InitializeComponent();
        }

        private void Card_Click(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is GameDTO game && DataContext is UserLibraryViewModel vm)
            {
                vm.OpenDetailCommand.Execute(game);
            }
        }
    }
}
