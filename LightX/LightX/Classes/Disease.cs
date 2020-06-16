using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LightX.Classes
{
    public class Disease
    {
        #region Fields

        private string _displayName;
        private List<string> _keywords;

        #endregion Fields

        #region Properties

        public string DisplayName
        {
            get { return _displayName; }
            set { _displayName = value;  }
        }

        public List<string> Keywords
        {
            get { return _keywords; }
            set { _keywords = value; }
        }

        #endregion Properties
    }
}
