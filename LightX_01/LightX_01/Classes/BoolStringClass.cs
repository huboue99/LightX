using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightX_01.Classes
{
    public class BoolStringClass : BaseClass
    {
        #region Fields

        private string _text;
        private string _value;
        private bool _isSelected;

        #endregion Fields

        #region Properties

        public string Text
        {
            get { return _text; }
        }

        public string Value
        {
            get { return _value; }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if(value != _isSelected)
                {
                    _isSelected = value;
                    OnPropertyChanged("IsSelected");
                }
            }
        }

        #endregion Properties



        // Constructor

        public BoolStringClass(string text, string value, bool isSelected = true)
        {
            _text = text;
            _value = value;
            _isSelected = isSelected;
        }
    }
}
