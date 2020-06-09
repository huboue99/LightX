using LightX.Classes;
using LightX.ViewModel;
using System.Windows;

namespace LightX
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    
    public partial class PatientInfosWindow : Window
    {
        public Exam Exam
        {
            get { return _patientInfosWindowViewModel.Exam; }
            set
            {
                _patientInfosWindowViewModel.Exam = value;
            }
        }

        private readonly PatientInfosWindowViewModel _patientInfosWindowViewModel;
        public PatientInfosWindow()
        {
            _patientInfosWindowViewModel = new PatientInfosWindowViewModel();
            
            InitializeComponent();
            FileNumber.Focus();
            DataContext = _patientInfosWindowViewModel;
            this.Title = "LightX - Nouvel Examen";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }

        private void KeywordButton_Click(object sender, RoutedEventArgs e)
        {
            string keyword = (sender as System.Windows.Controls.Button).Content.ToString();
            _patientInfosWindowViewModel.RemoveKeyword(keyword);
        }
    }
}
