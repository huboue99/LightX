using LightX_01.Classes;
using LightX_01.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace LightX_01
{
    /// <summary>
    /// Interaction logic for GuideWindow.xaml
    /// </summary>
    /// 
    
    public partial class GuideWindow : Window
    {
        private readonly GuideWindowViewModel _guideWindowViewModel;

        public GuideWindow(PatientData patientData)
        {
            _guideWindowViewModel = new GuideWindowViewModel(patientData);

            InitializeComponent();
            // var mainWindow = this.DataContext;
            this.Title = $"LightX - {patientData.FirstName} {patientData.LastName}";
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
        }
    }
}
