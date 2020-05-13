using LightX_01.ViewModel;
using System.Windows;

namespace LightX_01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        public MainWindow()
        {
            _mainWindowViewModel = new MainWindowViewModel();
            
            InitializeComponent();
            FileNumber.Focus();
            DataContext = _mainWindowViewModel;
            this.Title = "LightX - Nouvel Examen";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
