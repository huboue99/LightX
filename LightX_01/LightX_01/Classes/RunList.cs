using System.Collections.ObjectModel;
using System.Windows.Documents;

namespace LightX_01.Classes
{
    public class RunList : ObservableCollection<Run>
    {
        public RunList(ObservableCollection<Tests> listString) : base()
        {
            ObservableCollection<Run> list = new ObservableCollection<Run>();
            foreach (Tests element in listString)
            {
                Run t = new Run();
                switch (element)
                {
                    case Tests.Conjonctive :
                        t = new Run("Conjonctive");
                        break;
                    case Tests.VanHerick :
                        t = new Run("Van Herick");
                        break;
                    case Tests.Cornea :
                        t = new Run("Cornée");
                        break;
                    case Tests.AnteriorChamber :
                        t = new Run("Chambre antérieur");
                        break;
                    case Tests.PupillaryMargin :
                        t = new Run("Marge pupillaire");
                        break;
                    case Tests.Lens :
                        t = new Run("Cristallin");
                        break;
                    case Tests.IrisTransillumination :
                        t = new Run("Transillumination de l'iris");
                        break;
                    case Tests.CobaltFilter :
                        t = new Run("Filtre Cobalt");
                        break;
                    case Tests.NewTest :
                        t = new Run("Nouveau test");
                        break;
                }
                Add(t);
            }
        }
    }
}
