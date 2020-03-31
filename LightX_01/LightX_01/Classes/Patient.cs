using System;
using System.Collections.ObjectModel;

namespace LightX_01.Classes
{
    public class Patient : BaseClass
    {
        #region Fields

        private string _firstName;
        private string _lastName;
        private DateTime _birthdate;
        private string _gender;
        private string _fileNumber;
        private string _ramq;

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

        public DateTime BirthDate
        {
            get { return _birthdate; }
            set
            {
                if (value != _birthdate)
                {
                    _birthdate = value;
                    OnPropertyChanged("BirthDate");
                }
            }
        }

        public string Gender
        {
            get { return _gender; }
            set
            {
                if (value != _gender)
                {
                    _gender = value;
                    OnPropertyChanged("Gender");
                }
            }
        }

        public string FileNumber
        {
            get { return _fileNumber; }
            set
            {
                if (value != _fileNumber)
                {
                    _fileNumber = value;
                    OnPropertyChanged("FileNumber");
                }
            }
        }

        public string RAMQ
        {
            get { return _ramq; }
            set
            {
                if (value != _ramq)
                {
                    _ramq = value;
                    OnPropertyChanged("RAMQ");
                }
            }
        }

        #endregion Properties
    }
}
