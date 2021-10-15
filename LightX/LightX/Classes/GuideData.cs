using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace LightX.Classes
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
        public ParametersList(Instruction data) : base()
        {
            ObservableCollection<Parameters> list = new ObservableCollection<Parameters>();
            Add(new Parameters() { Name = "Intensité slit", Value = data.SlitIntensity });
            Add(new Parameters() { Name = "Angle d'illumination", Value = data.IllumAngle });
            Add(new Parameters() { Name = "Intensité diffuse", Value = data.DiffuseIntensity });
            Add(new Parameters() { Name = "Flash", Value = data.CamSettings.Flash});
            Add(new Parameters() { Name = "Shutter speed", Value = data.CamSettings.ShutterSpeed });
            Add(new Parameters() { Name = "f#", Value = data.CamSettings.FNumber });
            Add(new Parameters() { Name = "ISO", Value = data.CamSettings.Iso });
        }
    }

    public class Instruction : BaseClass
    {
        #region Fields

        private string _slitIntensity;
        private string _illumAngle;
        private string _diffuseIntensity;
        private CameraSettings _camSettings;
        private string _instructionsNotes;
        private List<string> _imagesPath;

        #endregion Fields

        #region Properties

        public string SlitIntensity
        {
            get { return _slitIntensity; }
            set
            {
                if (value != _slitIntensity)
                {
                    _slitIntensity = value;
                    OnPropertyChanged("SlitIntensity");
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

        public string DiffuseIntensity
        {
            get { return _diffuseIntensity; }
            set
            {
                if (value != _diffuseIntensity)
                {
                    _diffuseIntensity = value;
                    OnPropertyChanged("DiffuseIntensity");
                }
            }
        }

        public CameraSettings CamSettings
        {
            get { return _camSettings; }
            set
            {
                if (value != _camSettings)
                {
                    _camSettings = value;
                    OnPropertyChanged("CamSettings");
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

    public class TestInstructions : BaseClass
    {
        #region Fields

        //private string _fileName;
        private Tests _id;
        private string _testTitle;
        private List<Instruction> _instructions = new List<Instruction>();

        #endregion Fields

        #region Properties

        public Tests Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged("Id");
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



        public List<Instruction> Instructions
        {
            get { return _instructions; }
            set
            {
                if (value != _instructions)
                {
                    _instructions = value;
                    OnPropertyChanged("Instructions");
                }
            }
        }

        #endregion Properties
    }

    public class GuideData : BaseClass
    {
        #region Fields

        //private string _fileName;
        private ParametersList _paramList;
        private string _instructionsNotes;
        private BitmapImage _image;
        private int _id;

        #endregion Fields

        #region Properties

        public ParametersList ParamList
        {
            get { return _paramList; }
            set
            {
                if (value != _paramList)
                {
                    _paramList = value;
                    OnPropertyChanged("ParamList");
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

        public BitmapImage Image
        {
            get { return _image; }
            set
            {
                if (value != _image)
                {
                    _image = value;
                    OnPropertyChanged("Image");
                }
            }
        }

        public int Id
        {
            get { return _id; }
            set
            {
                if (value != _id)
                {
                    _id = value;
                    OnPropertyChanged("Id");
                }
            }
        }

        #endregion Properties
    }
}

