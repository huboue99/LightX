using LightX.Classes;
using LightX.ViewModel;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace LightX
{
    /// <summary>
    /// Interaction logic for FinishWindow.xaml
    /// </summary>
    public partial class FinishWindow : Window
    {
        private bool _sendClosingEvent = true;

        private readonly FinishWindowViewModel _finishWindowViewModel;

        private ZoomBorder loadedZoomBorder;

        private bool TestHasChanged = false;

        public delegate void NewPhotoEventHandler(TestResults test);
        public event NewPhotoEventHandler NewPhotoEvent;

        public delegate void FinishWindowClosingEventHandler(CancelEventArgs e);
        public event FinishWindowClosingEventHandler FinishWindowClosingEvent;

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //_finishWindowViewModel.ActiveImageEvent(sender as Image);

            bool imageHasChanged = _finishWindowViewModel.ActiveImageEvent(sender as Image);
            if (TestHasChanged)
            {
                loadedZoomBorder.Reset();
                TestHasChanged = false;
            }
                //(TabControl01.Template.FindName("border", TabControl01) as ZoomBorder).Reset();
                //(TabControl01.SelectedContentTemplate.FindName("border", TabControl01) as ZoomBorder).Reset();
                
                //((TabControl01.SelectedContent as TabControl).ContentTemplate.FindName("border", TabControl01.SelectedContent as TabControl) as ZoomBorder);
        }

        private void TabControl01_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            TestHasChanged = true;
            GC.Collect();
        }

        private void NewPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            NewPhotoEvent(this.TabControl01.SelectedContent as TestResults);
        }

        public FinishWindow(Exam exam)
        {
            _finishWindowViewModel = new FinishWindowViewModel(exam);
            InitializeComponent();

            DataContext = _finishWindowViewModel;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public void CloseWithoutEvent()
        {
            _sendClosingEvent = false;
            this.Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_sendClosingEvent)
                FinishWindowClosingEvent(e);
            _sendClosingEvent = true;
        }

        private void Border_Loaded(object sender, RoutedEventArgs e)
        {
            loadedZoomBorder = sender as ZoomBorder;
        }
    }
}
