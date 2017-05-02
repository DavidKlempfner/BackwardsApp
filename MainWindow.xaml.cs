using System.Windows;
using BackwardsApp.Backwards;

namespace BackwardsApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Reverser.Start();
        }
    }
}
