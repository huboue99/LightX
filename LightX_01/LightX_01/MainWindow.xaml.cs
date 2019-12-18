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
        string prenom = "";
        string nom = "";

        private ICommand exitMainWindow;
        private ICommand createNewExam_key;

        public MainWindow()
        {
            InitializeComponent();
            Prenom.Focus();
            DataContext = this;
            this.Title = "LightX - Nouvel Examen";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        public ICommand ExitMainWindow
        {
            get
            {
                return exitMainWindow
                    ?? (exitMainWindow = new ActionCommand(() => { this.Close(); }));
            }
        }

        public ICommand CreateNewExam_key
        {
            get
            {
                return createNewExam_key
                    ?? (createNewExam_key = new ActionCommand(() => { CreateNewExam(); }));
            }
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

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            CreateNewExam();
        }

        private void CreateNewExam()
        {
            prenom = Prenom.Text.Trim();
            nom = Nom.Text.Trim();
            if(string.IsNullOrEmpty(prenom) || string.IsNullOrWhiteSpace(prenom) || string.IsNullOrWhiteSpace(nom) || string.IsNullOrEmpty(nom))
            {
                MessageBox.Show("Veuillez préciser le prénom et le nom.");
            }
            else
            {
                MessageBox.Show($"Hi, {prenom} {nom}");
            }
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
