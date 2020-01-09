using System;
using System.Collections.ObjectModel;

namespace LightX_01.Classes
{
    public class Patient : BaseClass
    {
        #region Fields

        private string _firstName;
        private string _lastName;
        private int _age;
        //private DateTime _examDate;
        //private ObservableCollection<string> _testList;

        #endregion Fields

        #region Properties

        public string FirstName
        {
            get { return _firstName; }
            set
            {
                if (value != _firstName)
                {
                    _firstName = value;
                    OnPropertyChanged("FirstName");
                }
            }
        }

        public string LastName
        {
            get { return _lastName; }
            set
            {
                if (value != _lastName)
                {
                    _lastName = value;
                    OnPropertyChanged("LastName");
                }
            }
        }

        public int Age
        {
            get { return _age; }
            set
            {
                if (value != _age)
                {
                    _age = value;
                    OnPropertyChanged("Age");
                }
            }
        }

        #endregion Properties
    }




}
