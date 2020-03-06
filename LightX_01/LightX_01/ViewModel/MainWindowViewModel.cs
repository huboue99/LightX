using GalaSoft.MvvmLight;
using LightX_01.Classes;
using System.ComponentModel;
using System.Windows;
using System;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;
using System.Collections.ObjectModel;

namespace LightX_01.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private Patient _currentPatient;
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

        #region Commands

        public ICommand CloseWindowCommand
        {
            get
            {
                if (_closeWindowCommand == null)
                {
                    _closeWindowCommand = new RelayCommand<Window>(
                        param => CloseWindow(param)
                        );
                }
                return _closeWindowCommand;
            }
        }


        public ICommand CreateNewExamCommand
        {
            get
            {
                if (_createNewExamCommand == null)
                {
                    _createNewExamCommand = new RelayCommand<Window>(
                        param => CreateNewExam(param)
                        );
                }
                return _createNewExamCommand;
            }
        }

        #endregion Commands

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        private void CreateNewExam(Window window)
        {
            bool TESTING = true;
            if ((string.IsNullOrEmpty(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.LastName) || string.IsNullOrEmpty(CurrentPatient.LastName)) && !TESTING)
            {
                MessageBox.Show("Veuillez préciser le prénom et le nom.");
            }
            else
            {
                if(TESTING)
                {
                    CurrentPatient.FirstName = "John";
                    CurrentPatient.LastName = "Smith";
                }

                CurrentPatient.FirstName.Trim();
                CurrentPatient.LastName = CurrentPatient.LastName.Trim();

                Exam exam = new Exam() { Patient = CurrentPatient };

                //GuideWindow objGuideWindow = new GuideWindow(exam);
                CameraControlWindow objCamControlWindow = new CameraControlWindow(exam);
                //objGuideWindow.DataContext = this;
                //this.Visibility = Visibility.Hidden; // Hidding the current window
                this.CloseWindow(window);
                //objGuideWindow.Show();
                objCamControlWindow.Show();
            }
        }

        public MainWindowViewModel()
        {
            
            Genders = new ObservableCollection<string>() { "Homme", "Femme" };
            _currentPatient = new Patient() { Gender = Genders[0] };
        }
    }
}
