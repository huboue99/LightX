using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX.Classes;
using System;
using System.Windows;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.IO;
using Newtonsoft.Json;

namespace LightX.ViewModel
{
    public class PatientInfosWindowViewModel : ViewModelBase
    {
        #region Fields

        private Patient _currentPatient;
        private Exam _exam;
        private ObservableCollection<Disease> _keywords;
        private ObservableCollection<BoolStringClass> _currentTestListChoices;
        private bool _allSelectedChecked = true;

        public ObservableCollection<Disease> KeywordsList { get; set; }
        public string SelectedItem { get; set; } = "";

        // Commands definition
        private ICommand _closeWindowCommand;
        private ICommand _createNewExamCommand;
        private ICommand _selectAllClickCommand;

        //public ObservableCollection<string> Genders { get; set; }

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

        internal void RemoveKeyword(Disease keyword)
        {
            Keywords.Remove(keyword);
            RaisePropertyChanged(() => Keywords);
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

        private ObservableCollection<Disease> ReadKeywordsList()
        {
            ObservableCollection<Disease> list;
            string path = $@"..\..\Resources\Keywords.json";
            if (File.Exists(path))
            {
                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    list = (ObservableCollection<Disease>)serializer.Deserialize(file, typeof(ObservableCollection<Disease>));
                }
            }
            else
                list = new ObservableCollection<Disease>();
            return list;
        }

        internal void SaveKeywordList(ObservableCollection<Disease> list)
        {
            string path = @"..\..\Resources\";
            SaveKeywordList(list, path);
        }

        internal void SaveKeywordList(ObservableCollection<Disease> list, string path)
        {
            if (!path.EndsWith(@"\"))
                path += @"\";
            using (StreamWriter file = File.CreateText($"{path}Keywords.json"))
            {
                Console.WriteLine("Writing Keywords.json to disk...");
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, list);
            }
        }

        internal void SaveKeywordAsList(List<string> list, string path)
        {
            if (!path.EndsWith(@"\"))
                path += @"\";
            using (StreamWriter file = File.CreateText($"{path}Keywords.json"))
            {
                Console.WriteLine("Writing Keywords.json to disk...");
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, list);
            }
        }

        public ObservableCollection<Disease> Keywords
        {
            get { return _keywords; }
            set
            {
                if (value != _keywords)
                {
                    _keywords = value;
                    RaisePropertyChanged(() => Keywords);
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
            window.DialogResult = CreateNewExam();
            if (window.DialogResult == true)
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
                window.Close();
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

        private bool ? CreateNewExam()
        {
            ///////////// TESTINGS /////////////////
            bool TESTING = true;
            ////////////////////////////////////////

            // Vérifications
            if ((string.IsNullOrEmpty(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.FirstName) || string.IsNullOrWhiteSpace(CurrentPatient.LastName) || string.IsNullOrEmpty(CurrentPatient.LastName)) && !TESTING)
            {
                MessageBox.Show("Veuillez préciser le prénom et le nom.");
                return null;
            }
            else if (AreAllTrueOrFalse() == false)
            {
                MessageBox.Show("Aucun test n'a été selectionné. Veuillez en sélectionner au moins un.");
                return null;
            }
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
                Exam.Keywords = new List<string>();
                foreach (Disease keywrd in Keywords)
                {
                    Exam.Keywords.Add(keywrd.DisplayName);
                }

                Exam.ResultsPath = string.Format("{0}\\{1}\\{2}_{3}_{4}_{5,2:D2}_{6,2:D2}_{7,2:D2}h{8,2:D2}",
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures),
                "LightX",
                Exam.Patient.LastName,
                Exam.Patient.FirstName,
                Exam.ExamDate.Year,
                Exam.ExamDate.Month,
                Exam.ExamDate.Day,
                Exam.ExamDate.Hour,
                Exam.ExamDate.Minute);

                if (Exam.Keywords.Count > 0)
                {
                    if (!Directory.Exists(Exam.ResultsPath))
                        Directory.CreateDirectory(Exam.ResultsPath);
                    SaveKeywordAsList(Exam.Keywords, Exam.ResultsPath);
                }
                return true;
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
            _currentTestListChoices.Add(new BoolStringClass("Transillumination de l'iris", Tests.IrisTransillumination));
            _currentTestListChoices.Add(new BoolStringClass("Marge Pupillaire", Tests.PupillaryMargin));
        }

        private void SelectAllClick()
        {
            bool ? a = AreAllTrueOrFalse();
            foreach (BoolStringClass test in _currentTestListChoices)
            {
                switch(a)
                {
                    case true:
                        test.IsSelected = false;
                        break;
                    default:
                        test.IsSelected = true;
                        break;
                }
            }
        }

        #endregion Actions

        private bool ? AreAllTrueOrFalse()
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
                return false; // All false
            else if (i != _currentTestListChoices.Count)
                return null; // Some true, some false
            else
                return true; // All true
        }

        public PatientInfosWindowViewModel()
        {
            KeywordsList = ReadKeywordsList();
            CreateCheckBoxList();
            Keywords = new ObservableCollection<Disease>();
            _currentPatient = new Patient() { BirthDate = DateTime.Today };
        }
    }
}
