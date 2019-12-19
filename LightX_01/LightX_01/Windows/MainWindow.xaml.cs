using LightX_01.ViewModel;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LightX_01
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 

    //public partial class KeyCommands : MainWindow
    //{

    //}
    
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _mainWindowViewModel;
        public MainWindow()
        {
            _mainWindowViewModel = new MainWindowViewModel();
            
            InitializeComponent();
            FirstName.Focus();
            DataContext = _mainWindowViewModel;
            this.Title = "LightX - Nouvel Examen";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        //private void Button1Close_Click(object sender, RoutedEventArgs e)
        //{
        //    MessageBox.Show("The App is Closing");
        //}

        //private void ButtonOpenFile_Click(object sender, RoutedEventArgs e)
        //{
        //    OpenFileDialog openDlg = new OpenFileDialog();
        //    openDlg.ShowDialog();
        //}

        //private void ButtonSaveFile_Click(object sender, RoutedEventArgs e)
        //{
        //    SaveFileDialog saveDlg = new SaveFileDialog();
        //    saveDlg.ShowDialog();
        //}

        //private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        //{
        //    CreateNewExam();
        //}

        //private void CreateNewExam()
        //{
        //    firstName = FirstName.Text.Trim();
        //    lastName = LastName.Text.Trim();
        //    if(string.IsNullOrEmpty(firstName) || string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName) || string.IsNullOrEmpty(lastName))
        //    {
        //        MessageBox.Show("Veuillez préciser le prénom et le nom.");
        //    }
        //    else
        //    {
        //        GuideWindow objGuideWindow = new GuideWindow(firstName, lastName);
        //        //objGuideWindow.DataContext = this;
        //        //this.Visibility = Visibility.Hidden; // Hidding the current window
        //        this.Close();
        //        objGuideWindow.Show();
        //    }
        //}

        //private void ButtonClose_Click(object sender, RoutedEventArgs e)
        //{
        //    this.Close();
        //}
    }
}
