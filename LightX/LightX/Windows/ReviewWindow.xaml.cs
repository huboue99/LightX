using LightX.Classes;
using LightX.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace LightX
{
    public partial class ReviewWindow : Window
    {
        private readonly ReviewWindowViewModel _reviewWindowViewModel;
        private ZoomBorder loadedZoomBorder;

        public string Comment
        {
            get { return _reviewWindowViewModel.CurrentComment; }
        }

        public ObservableCollection<bool> SelectedImages
        {
            get
            {
                ObservableCollection<bool> selectedImages = new ObservableCollection<bool>();
                foreach (ReviewImage reviewImage in _reviewWindowViewModel.ReviewImages)
                    selectedImages.Add(reviewImage.IsSelected);
                return selectedImages;
            }
        }

        public void RefreshReviewImages(List<string> images)
        {
            _reviewWindowViewModel.RefreshReviewImages(images);
        }

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                _reviewWindowViewModel.SelectImageEvent(sender as Image);
            else
            {
                bool imageHasChanged = _reviewWindowViewModel.ActiveImageEvent(sender as Image);
                if (imageHasChanged)
                    loadedZoomBorder.Reset();
            }
        }

        public ReviewWindow(List<string> images, string comment)
        {
            _reviewWindowViewModel = new ReviewWindowViewModel(images, comment);
            InitializeComponent();
            DataContext = _reviewWindowViewModel;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            loadedZoomBorder = sender as ZoomBorder;
        }
    }
}
