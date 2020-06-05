using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX.Classes;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace LightX.ViewModel
{
    public class PatientInfosWindowViewModel : ViewModelBase
    {
        #region Fields

        private Patient _currentPatient;
        private Exam _exam;
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

        public Exam Exam
        {
            get { return _exam; }
            set
            {
                if (value != _exam)
                {
                    _exam = value;
                    RaisePropertyChanged(() => Exam);
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
                        param => Cancel(param)
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
                        param => Confirm(param)
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

        private void Confirm(Window window)
        {
            CreateNewExam();
            window.DialogResult = true;
            CloseWindow(window);
        }

        private void Cancel(Window window)
        {
            window.DialogResult = false;
            CloseWindow(window);
        }

        private void CloseWindow(Window window)
        {
            if (window != null)
            {
                window.Close();
            }
        }

        private ObservableCollection<Tests> CreateTestList()
        {
            ObservableCollection<Tests> testList = new ObservableCollection<Tests>();
            foreach(BoolStringClass test in CurrentTestListChoices)
            {
                if (test.IsSelected)
                    testList.Add(test.Value);
            }

            return testList;
        }

        private void CreateNewExam()
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
                    if (string.IsNullOrEmpty(CurrentPatient.FirstName) && string.IsNullOrEmpty(CurrentPatient.LastName))
                        CurrentPatient.FirstName = "John";

                    if (string.IsNullOrEmpty(CurrentPatient.LastName))
                        CurrentPatient.LastName = "Smith";
                }
                //////////////////////////////////

                // Sanitize input string
                CurrentPatient.FirstName.Trim();
                CurrentPatient.LastName = CurrentPatient.LastName.Trim();

                ObservableCollection<Tests> testList = CreateTestList();

                // Create the Exam (patient, currentTime, testList)
                if (Exam == null)
                    Exam = new Exam();

                Exam.Patient = CurrentPatient;
                Exam.TestList = testList;

                // Open control and guide windows; close the patient info windows
                //CameraControlWindow objCamControlWindow = new CameraControlWindow(exam);
                //this.CloseWindow(window);
                //objCamControlWindow.Show();
            }
        }

        private void CreateCheckBoxList()
        {
            _currentTestListChoices = new ObservableCollection<BoolStringClass>();
            _currentTestListChoices.Add(new BoolStringClass("Conjonctive", Tests.Conjonctive));
            _currentTestListChoices.Add(new BoolStringClass("Filtre Cobalt", Tests.CobaltFilter));
            _currentTestListChoices.Add(new BoolStringClass("Van Herick", Tests.VanHerick));
            _currentTestListChoices.Add(new BoolStringClass("Cornée", Tests.Cornea));
            _currentTestListChoices.Add(new BoolStringClass("Chambre Antérieure", Tests.AnteriorChamber));
            _currentTestListChoices.Add(new BoolStringClass("Cristallin", Tests.Lens));
            _currentTestListChoices.Add(new BoolStringClass("Marge Pupillaire", Tests.PupillaryMargin));
            _currentTestListChoices.Add(new BoolStringClass("Transillumination de l'iris", Tests.IrisTransillumination));
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

        public PatientInfosWindowViewModel()
        {
            CreateCheckBoxList();
            Genders = new ObservableCollection<string>() { "Homme", "Femme" };
            _currentPatient = new Patient() { Gender = Genders[0] };
        }
    }
}
