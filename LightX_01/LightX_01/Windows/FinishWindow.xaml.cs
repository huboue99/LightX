using LightX_01.Classes;
using LightX_01.ViewModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        public FinishWindow(Exam exam)
        {
            _finishWindowViewModel = new FinishWindowViewModel(exam);
            InitializeComponent();

            DataContext = _finishWindowViewModel;

            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
