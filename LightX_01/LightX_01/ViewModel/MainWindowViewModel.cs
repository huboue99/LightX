using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX_01.Classes;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace LightX_01.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private Patient _currentPatient;

        // Commands definition
        private ICommand _closeWindowCommand;
        private ICommand _createNewExamCommand;

        public ObservableCollection<string> Genders { get; set; }

        #endregion Fields

        #region Properties

        public Patient CurrentPatient
        {
            get { return _currentPatient; }
            set
            {
                if (value != _currentPatient)
                {
                    _currentPatient = value;
                    RaisePropertyChanged(() => CurrentPatient);
                }
            }
        }

        #endregion Properties

        #region RelayCommands

        public ICommand CloseWindowCommand
        {
            get
            {
                if (_closeWindowCommand == null)
                    _closeWindowCommand = new RelayCommand<Window>(
                        param => CloseWindow(param)
                        );
                return _closeWindowCommand;
            }
        }


        public ICommand CreateNewExamCommand
        {
            get
            {
                if (_createNewExamCommand == null)
                    _createNewExamCommand = new RelayCommand<Window>(
                        param => CreateNewExam(param)
                        );
                return _createNewExamCommand;
            }
        }

        #endregion RelayCommands

        #region Actions

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        private void CreateNewExam(Window window)
        {
            ///////////// TESTINGS /////////////////
            bool TESTING = true;
            ////////////////////////////////////////
            
            if ((string.IsNullOrEmpty(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.LastName) || string.IsNullOrEmpty(CurrentPatient.LastName)) && !TESTING)
            {
                MessageBox.Show("Veuillez préciser le prénom et le nom.");
            }
            else
            {
                ///////////////////////////////////
                if(TESTING)
                {
                    CurrentPatient.FirstName = "John";
                    CurrentPatient.LastName = "Smith";
                }
                //////////////////////////////////

                // Sanitize input string
                CurrentPatient.FirstName.Trim();
                CurrentPatient.LastName = CurrentPatient.LastName.Trim();

                // Create the Exam (patient, currentTime, testList)
                Exam exam = new Exam() { Patient = CurrentPatient };

                // Open control and guide windows; close the patien info windows
                CameraControlWindow objCamControlWindow = new CameraControlWindow(exam);
                this.CloseWindow(window);
                objCamControlWindow.Show();
            }
        }

        #endregion Actions

        public MainWindowViewModel()
        {
            Genders = new ObservableCollection<string>() { "Homme", "Femme" };
            _currentPatient = new Patient() { Gender = Genders[0] };
        }
    }
}
