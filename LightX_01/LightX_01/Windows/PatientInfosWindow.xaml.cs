using LightX_01.Classes;
using LightX_01.ViewModel;
using System.Windows;

namespace LightX_01
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
    }
}
