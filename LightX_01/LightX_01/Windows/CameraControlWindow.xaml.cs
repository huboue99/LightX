using LightX_01.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
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
    /// Interaction logic for CameraControlWindow.xaml
    /// </summary>
    public partial class CameraControlWindow : Window
    {
        private readonly CameraControlWindowViewModel _cameraControlWindowViewModel;

        public CameraControlWindow()
        {
            _cameraControlWindowViewModel = new CameraControlWindowViewModel();
            InitializeComponent();
            DataContext = _cameraControlWindowViewModel;

            //this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Left = Screen.AllScreens[0].WorkingArea.Right - this.Width - this.Width / 4;
            this.Top = Screen.AllScreens[0].WorkingArea.Height / 2 - this.Height / 2;
        }
    }
}
