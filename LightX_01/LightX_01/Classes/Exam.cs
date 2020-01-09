using System;
using System.Collections.ObjectModel;

namespace LightX_01.Classes
{
    public class Exam : BaseClass
    {
        #region Fields

        private Patient _patient;
        private DateTime _examDate;
        private ObservableCollection<string> _testList;
        private string _comments;

        #endregion Fields

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

        public ObservableCollection<string> TestList
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

        public string Comments
        {
            get { return _comments; }
            set
            {
                if (value != _comments)
                {
                    _comments = value;
                    OnPropertyChanged("Comments");
                }
            }
        }

        public Exam()
        {
            //get dateCurrent
            _testList = new ObservableCollection<string>() { "Conjonctive", "VanHerick", "Cornea", "AnteriorChamber", "Lens", "PupillaryMargin", "IrisTransillumination", "CobaltFilter" };
            _examDate = DateTime.Now;
        }
    }
}
