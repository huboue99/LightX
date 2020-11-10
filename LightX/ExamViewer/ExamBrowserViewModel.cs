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
using System.Linq;
using System.Windows.Controls;
using System.ComponentModel;

namespace ExamViewer
{
    public class ExamBrowserViewModel : ViewModelBase
    {
        #region Fields

        //private Patient _currentPatient;
        private List<Exam> _examList;
        public ObservableCollection<Exam> ExamsFiltered { get; set; }
        private string _searchBox;

        private char[] charSeparators = new char[] { ',', ' ', ';', '+', '-'};

        private ReviewWindow _reviewWindow = null;

        //private ObservableCollection<Disease> _keywords;
        //private ObservableCollection<BoolStringClass> _currentTestListChoices;
        //private bool _allSelectedChecked = true;

        //public ObservableCollection<Disease> KeywordsList { get; set; }
        //public string SelectedItem { get; set; } = "";

        //// Commands definition
        //private ICommand _closeWindowCommand;
        //private ICommand _createNewExamCommand;
        //private ICommand _selectAllClickCommand;

        //public ObservableCollection<string> Genders { get; set; }

        #endregion Fields

        #region Properties

        public List<Exam> ExamList
        {
            get { return _examList; }
            set
            {
                if (value != _examList)
                {
                    _examList = value;
                    RaisePropertyChanged(() => ExamList);
                }
            }
        }

        public string SearchBox
        {
            get { return _searchBox; }
            set
            {
                if (value != _searchBox)
                {
                    _searchBox = value;
                    RaisePropertyChanged(() => SearchBox);
                }
            }
        }

        #endregion Properties

        #region DataAccess

        public T ReadJsonObj<T>(string path)
        {
            if (!File.Exists(path))
                return default(T);
            T obj;
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                try
                {
                    obj = (T)serializer.Deserialize(file, typeof(T));

                }
                catch
                {
                    Console.WriteLine("The .json file does not contain the right class type.");
                    return default(T);
                }
            }
            return obj;
        }

        #endregion DataAccess


        #region Functions

        List<Exam> FetchExamList(string pathToExams)
        {
            List<Exam> exams = new List<Exam>();
            // for each folder in the path get the json file and read the exam and exams.Add the found exam;

            try
            {
                string[] dirs = Directory.GetDirectories(pathToExams, "*", SearchOption.TopDirectoryOnly);
                Console.WriteLine("The number of directories : {0}.", dirs.Length);
                foreach (string dir in dirs)
                {
                    Exam e = new Exam();
                    e = ReadJsonObj<Exam>($"{dir}\\exam.json");
                    if(e != default(Exam))
                        exams.Add(e);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The process failed: {0}", e.ToString());
            }

            return exams;
        }

        public void GetFilteredList()
        {
            /* Perform a Linq query to find all Person objects (from the original People collection)
            that fit the criteria of the filter, save them in a new List called TempFiltered. */
            List<Exam> TempFiltered = new List<Exam>(ExamList);

            /* Make sure all text is case-insensitive when comparing, and make sure 
            the filtered items are in a List object */
            string[] filters = SearchBox.ToLowerInvariant().Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
            
            foreach(string filter in filters)
            {
                TempFiltered = TempFiltered.Where(x => (x.Patient.LastName.ToLowerInvariant().Contains(filter))
                                        || (x.Patient.FirstName.ToLowerInvariant().Contains(filter))
                                        || (x.Patient.BirthDate.ToString().ToLowerInvariant().Contains(filter))
                                        || (x.Patient.RAMQ.ToLowerInvariant().Contains(filter))
                                        || (x.Patient.FileNumber.ToLowerInvariant().Contains(filter))
                                        || (x.Keywords.Contains(filter))
                                        || (x.ExamDate.ToString().ToLowerInvariant().Contains(filter))).ToList();
            }

            /* Go through TempFiltered and compare it with the current PeopleFiltered collection,
            adding and subtracting items as necessary: */

            // First, remove any Person objects in PeopleFiltered that are not in TempFiltered
            for (int i = ExamsFiltered.Count - 1; i >= 0; i--)
            {
                var item = ExamsFiltered[i];
                if (!TempFiltered.Contains(item))
                {
                    ExamsFiltered.Remove(item);
                }
            }

            /* Next, add back any Person objects that are included in TempFiltered and may 
            not currently be in PeopleFiltered (in case of a backspace) */

            foreach (var item in TempFiltered)
            {
                if (!ExamsFiltered.Contains(item))
                {
                    ExamsFiltered.Add(item);
                }
            }

            RaisePropertyChanged(() => ExamsFiltered);
        }

        public void ExamSelected(Exam selectedExam)
        {
            // INIT new window + hide current examBrowserWindow + reset search result
            ShowReviewWindow(selectedExam);
            SearchBox = "";            
        }

        private void ShowReviewWindow(Exam exam)
        {
            HideWindow<ExamBrowser>();

            if (_reviewWindow == null)
            {
                _reviewWindow = new ReviewWindow(exam);
                _reviewWindow.ClosingEvent += ReviewWindowClosingEventHandler;
            }

            _reviewWindow.Show();
        }

        private void HideWindow<T>() where T : Window
        {
            T window = GetCurrentWindow<T>();
            window.Hide();
        }

        private void ShowWindow<T>() where T : Window
        {
            T window = GetCurrentWindow<T>();
            window.Show();
        }

        private T GetCurrentWindow<T>() where T : Window
        {
            T currentWindow = default(T);
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(T))
                    {
                        currentWindow = (window as T);
                    }
                }
            });

            return currentWindow;
        }

        private void ReviewWindowClosingEventHandler(CancelEventArgs e)
        {

            _reviewWindow.ClosingEvent -= ReviewWindowClosingEventHandler;
        }

            #endregion Functions

            internal ExamBrowserViewModel()
        {
            ExamList = FetchExamList(@"D:\Mathieu\Images\LightX");
            ExamsFiltered = new ObservableCollection<Exam>(ExamList);
        }
    }
}
