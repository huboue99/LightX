using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace LightX_01.Classes
{
    public class RunList : ObservableCollection<Run>
    {
        public RunList(List<string> listString) : base()
        {
            ObservableCollection<Run> list = new ObservableCollection<Run>();
            foreach (string element in listString)
            {
                Run t = new Run(element);
                Add(t);
            }
        }
    }
}
