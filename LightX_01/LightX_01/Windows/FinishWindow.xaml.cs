using LightX_01.Classes;
using LightX_01.ViewModel;
using System;
using System.Windows;
using System.Windows.Controls;

namespace LightX_01
{
    /// <summary>
    /// Interaction logic for FinishWindow.xaml
    /// </summary>
    public partial class FinishWindow : Window
    {
        private readonly FinishWindowViewModel _finishWindowViewModel;

        public delegate void NewPhotoEventHandler(TestResults test);

        public event NewPhotoEventHandler NewPhotoEvent;

        public FinishWindow(Exam exam)
        {
            _finishWindowViewModel = new FinishWindowViewModel(exam);
            InitializeComponent();

            DataContext = _finishWindowViewModel;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

            //this.TabControl01.Template.Template
        }

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
    }
}
