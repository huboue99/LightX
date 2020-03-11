using LightX_01.ViewModel;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LightX_01
{
    public partial class ReviewWindow : Window
    {
        private readonly ReviewWindowViewModel _reviewWindowViewModel;

        public string Comment
        {
            get { return _reviewWindowViewModel.CurrentComment; }
        }

        public ObservableCollection<bool> SelectedImages
        {
            get { return _reviewWindowViewModel.SelectedImages; }
        }

        public ReviewWindow(ObservableCollection<BitmapImage> images, string comment)
        {
            _reviewWindowViewModel = new ReviewWindowViewModel(images, comment);
            InitializeComponent();
            DataContext = _reviewWindowViewModel;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
