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
using LightX_01.ViewModel;

namespace LightX_01
{
    /// <summary>
    /// Interaction logic for ReviewWindow.xaml
    /// </summary>
    public partial class ReviewWindow : Window
    {
        private readonly ReviewWindowViewModel _reviewWindowViewModel;

        public string Comment
        {
            get { return _reviewWindowViewModel.CurrentComment; }
        }

        public ReviewWindow(BitmapImage image, string comment)
        {
            _reviewWindowViewModel = new ReviewWindowViewModel(image, comment);
            InitializeComponent();
            DataContext = _reviewWindowViewModel;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
