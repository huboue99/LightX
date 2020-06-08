using LightX.Classes;
using LightX.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;

namespace LightX
{
    public partial class ReviewWindow : Window
    {
        private readonly ReviewWindowViewModel _reviewWindowViewModel;
        private ZoomBorder loadedZoomBorder;
        private bool? _isAccepted = null;

        public delegate void ReviewWindowClosingEventHandler(bool ? accepted);
        public event ReviewWindowClosingEventHandler ReviewWindowClosingEvent;

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
                //if (imageHasChanged)
                //    loadedZoomBorder.Reset();
            }
        }

        public ReviewWindow(List<string> images, string comment)
        {
            _reviewWindowViewModel = new ReviewWindowViewModel(images, comment);
            InitializeComponent();
            DataContext = _reviewWindowViewModel;

            //this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Left = 5;
            this.Top = 15;
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            loadedZoomBorder = sender as ZoomBorder;
        }

        private void ConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            _isAccepted = true;
            this.Close();
            //ReviewWindowClosingEvent(_isAccepted);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _isAccepted = false;
            this.Close();
            //ReviewWindowClosingEvent(_isAccepted);
        }

        private void PhotoReviewWindow_Closing(object sender, CancelEventArgs e)
        {
            ReviewWindowClosingEvent(_isAccepted);
        }

        private void PhotoReviewWindow_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key is Key.F1)
            {
                _reviewWindowViewModel.SelectAllImages();
            }
        }
    }
}
