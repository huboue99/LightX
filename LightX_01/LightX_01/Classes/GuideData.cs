using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media.Imaging;

namespace LightX_01.Classes
{
    public class Parameters : BaseClass
    {
        #region Fields

        private string _name;
        private string _value;

        #endregion Fields

        #region Properties

        public string Name
        {
            get { return _name; }
            set
            {
                if (value != _name)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public string Value
        {
            get { return _value; }
            set
            {
                if (value != _value)
                {
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        #endregion Properties
    }

    public class ParametersList : ObservableCollection<Parameters>
    {
        public ParametersList(GuideData data) : base()
        {
            ObservableCollection<Parameters> list = new ObservableCollection<Parameters>();
            Add(new Parameters() { Name = "Grossissement", Value = data.Zoom });
            Add(new Parameters() { Name = "Type d'illumination", Value = data.IllumType });
            Add(new Parameters() { Name = "Intensité d'illumination", Value = data.IllumIntensity });
            Add(new Parameters() { Name = "Angle d'illumination", Value = data.IllumAngle });
            
        }
    }

    public class GuideData : BaseClass
    {
        #region Fields

        private string _fileName;
        private string _testTitle;
        private string _zoom;
        private string _illumType;
        private string _illumIntensity;
        private string _illumAngle;
        private string _instructionsNotes;
        private List<string> _imagesPath;

        #endregion Fields

        #region Properties

        public string FileName
        {
            get { return _fileName; }
            set
            {
                if (value != _fileName)
                {
                    _fileName = value;
                    OnPropertyChanged("FileName");
                }
            }
        }

        public string TestTitle
        {
            get { return _testTitle; }
            set
            {
                if (value != _testTitle)
                {
                    _testTitle = value;
                    OnPropertyChanged("TestTitle");
                }
            }
        }

        public string Zoom
        {
            get { return _zoom; }
            set
            {
                if (value != _zoom)
                {
                    _zoom = value;
                    OnPropertyChanged("Zoom");
                }
            }
        }

        public string IllumType
        {
            get { return _illumType; }
            set
            {
                if (value != _illumType)
                {
                    _illumType = value;
                    OnPropertyChanged("IllumType");
                }
            }
        }

        public string IllumIntensity
        {
            get { return _illumIntensity; }
            set
            {
                if (value != _illumIntensity)
                {
                    _illumIntensity = value;
                    OnPropertyChanged("IllumIntensity");
                }
            }
        }

        public string IllumAngle
        {
            get { return _illumAngle; }
            set
            {
                if (value != _illumAngle)
                {
                    _illumAngle = value;
                    OnPropertyChanged("IllumAngle");
                }
            }
        }

        public string InstructionsNotes
        {
            get { return _instructionsNotes; }
            set
            {
                if (value != _instructionsNotes)
                {
                    _instructionsNotes = value;
                    OnPropertyChanged("InstructionsNotes");
                }
            }
        }

        public List<string> ImagesPath
        {
            get { return _imagesPath; }
            set
            {
                if (value != _imagesPath)
                {
                    _imagesPath = value;
                    OnPropertyChanged("ImagesPath");
                }
            }
        }

        #endregion Properties
    }


}
