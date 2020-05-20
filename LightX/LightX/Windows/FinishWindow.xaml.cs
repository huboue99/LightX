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

        public delegate void NewPhotoEventHandler(TestResults test);
        public event NewPhotoEventHandler NewPhotoEvent;

        public delegate void FinishWindowClosingEventHandler(CancelEventArgs e);
        public event FinishWindowClosingEventHandler FinishWindowClosingEvent;

        private void Image_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _finishWindowViewModel.ActiveImageEvent(sender as Image);
        }

        private void TabControl01_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
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
    }
}
