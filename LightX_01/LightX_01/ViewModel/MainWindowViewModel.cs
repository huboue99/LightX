using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX_01.Classes;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LightX_01.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields

        private Patient _currentPatient;
        private ObservableCollection<BoolStringClass> _currentTestListChoices;
        private bool _allSelectedChecked = true;

        // Commands definition
        private ICommand _closeWindowCommand;
        private ICommand _createNewExamCommand;
        private ICommand _selectAllClickCommand;

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

        public ObservableCollection<BoolStringClass> CurrentTestListChoices
        {
            get { return _currentTestListChoices; }
            set
            {
                if (value != _currentTestListChoices)
                {
                    _currentTestListChoices = value;
                    RaisePropertyChanged(() => CurrentTestListChoices);
                }
            }
        }

        public bool AllSelectedChecked
        {
            get { return _allSelectedChecked; }
            set
            {
                if (value != _allSelectedChecked)
                {
                    _allSelectedChecked = value;
                    RaisePropertyChanged(() => AllSelectedChecked);
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

        public ICommand SelectAllClickCommand
        {
            get
            {
                if(_selectAllClickCommand == null)
                    _selectAllClickCommand = new RelayCommand(SelectAllClick, true);
                return _selectAllClickCommand;
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

        private ObservableCollection<string> CreateTestList()
        {
            ObservableCollection<string> testList = new ObservableCollection<string>();
            foreach(BoolStringClass test in CurrentTestListChoices)
            {
                if (test.IsSelected)
                    testList.Add(test.Value);
            }



            return testList;
        }

        private void CreateNewExam(Window window)
        {
            ///////////// TESTINGS /////////////////
            bool TESTING = true;
            ////////////////////////////////////////

            // Vérifications
            if ((string.IsNullOrEmpty(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.LastName) || string.IsNullOrEmpty(CurrentPatient.LastName)) && !TESTING)
            {
                MessageBox.Show("Veuillez préciser le prénom et le nom.");
            }
            else if (AreAllTrueOrFalse() == 0)
                MessageBox.Show("Aucun test n'a été selectionné. Veuillez en sélectionner au moins un.");
            else
            {
                ///////////////////////////////////
                if (TESTING)
                {
                    CurrentPatient.FirstName = "John";
                    CurrentPatient.LastName = "Smith";
                }
                //////////////////////////////////

                // Sanitize input string
                CurrentPatient.FirstName.Trim();
                CurrentPatient.LastName = CurrentPatient.LastName.Trim();

                ObservableCollection<string> testList = CreateTestList();

                // Create the Exam (patient, currentTime, testList)
                Exam exam = new Exam() { Patient = CurrentPatient, TestList = testList };

                // Open control and guide windows; close the patien info windows
                CameraControlWindow objCamControlWindow = new CameraControlWindow(exam);
                this.CloseWindow(window);
                objCamControlWindow.Show();
            }
        }

        private void CreateCheckBoxList()
        {
            _currentTestListChoices = new ObservableCollection<BoolStringClass>();
            _currentTestListChoices.Add(new BoolStringClass("Conjonctive", "Conjonctive"));
            _currentTestListChoices.Add(new BoolStringClass("Van Herick", "VanHerick"));
            _currentTestListChoices.Add(new BoolStringClass("Cornée", "Cornea"));
            _currentTestListChoices.Add(new BoolStringClass("Chambre Antérieure", "AnteriorChamber"));
            _currentTestListChoices.Add(new BoolStringClass("Cristallin", "Lens"));
            _currentTestListChoices.Add(new BoolStringClass("Marge Pupillaire", "PupillaryMargin"));
            _currentTestListChoices.Add(new BoolStringClass("Transillumination de l'iris", "IrisTransillumination"));
            _currentTestListChoices.Add(new BoolStringClass("Filtre Cobalt", "CobaltFilter"));
        }

        private void SelectAllClick()
        {
            int a = AreAllTrueOrFalse();
            foreach (BoolStringClass test in _currentTestListChoices)
            {
                switch(a)
                {
                    case 1:
                        test.IsSelected = false;
                        break;
                    default:
                        test.IsSelected = true;
                        break;
                }
            }
        }

        #endregion Actions

        private int AreAllTrueOrFalse()
        {
            bool a = false;
            int i = 0;
            foreach(BoolStringClass test in _currentTestListChoices)
            {
                a = a || test.IsSelected;
                if (test.IsSelected)
                    i++;
            }
            if (!a)
                return 0; // All false
            else if (i != _currentTestListChoices.Count)
                return 2; // Some true, some false
            else
                return 1; // All true
        }

        public MainWindowViewModel()
        {
            CreateCheckBoxList();
            Genders = new ObservableCollection<string>() { "Homme", "Femme" };
            _currentPatient = new Patient() { Gender = Genders[0] };
        }
    }
}
