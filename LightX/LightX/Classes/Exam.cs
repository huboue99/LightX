using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace LightX.Classes
{
    public class Exam : BaseClass
    {
        #region Fields

        private Patient _patient;
        private DateTime _examDate;
        private ObservableCollection<Tests> _testList;
        private ObservableCollection<TestResults> _results;
        private string _resultsPath;
        private string _generalComments;
        private List<string> _keywords;

        #endregion Fields

        #region Properties

        public Patient Patient
        {
            get { return _patient; }
            set
            {
                if (value != _patient)
                {
                    _patient = value;
                    OnPropertyChanged("Patient");
                }
            }
        }

        public DateTime ExamDate
        {
            get { return _examDate; }
            set
            {
                if (value != _examDate)
                {
                    _examDate = value;
                    OnPropertyChanged("ExamDate");
                }
            }
        }

        public ObservableCollection<Tests> TestList
        {
            get { return _testList; }
            set
            {
                if (value != _testList)
                {
                    _testList = value;
                    OnPropertyChanged("TestList");
                }
            }
        }

        public ObservableCollection<TestResults> Results
        {
            get { return _results; }
            set
            {
                if (value != _results)
                {
                    _results = value;
                    OnPropertyChanged("Results");
                }
            }
        }

        public string ResultsPath
        {
            get { return _resultsPath; }
            set
            {
                if (value != _resultsPath)
                {
                    _resultsPath = value;
                    OnPropertyChanged("ResultsPath");
                }
            }
        }

        public string GeneralComments
        {
            get { return _generalComments; }
            set
            {
                if (value != _generalComments)
                {
                    _generalComments = value;
                    OnPropertyChanged("GeneralComments");
                }
            }
        }

        public List<string> Keywords
        {
            get { return _keywords; }
            set
            {
                if (value != _keywords)
                {
                    _keywords = value;
                    OnPropertyChanged("Keywords");
                }
            }
        }

        #endregion Properties

        public Exam()
        {
            // Default test list = ALL OF THEM
            if(_testList == null)
                _testList = new ObservableCollection<Tests>() { Tests.Conjonctive, Tests.VanHerick, Tests.Cornea, Tests.AnteriorChamber, Tests.Lens, Tests.PupillaryMargin, Tests.IrisTransillumination, Tests.CobaltFilter };
            _results = new ObservableCollection<TestResults>();
            _examDate = DateTime.Now;
        }
    }
}
