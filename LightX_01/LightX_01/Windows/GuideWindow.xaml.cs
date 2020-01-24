using LightX_01.Classes;
using LightX_01.ViewModel;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace LightX_01
{
    /// <summary>
    /// Interaction logic for GuideWindow.xaml
    /// </summary>
    /// 
    
    public partial class GuideWindow : Window
    {
        private readonly GuideWindowViewModel _guideWindowViewModel;

        [DllImport("user32.dll")]
        static extern IntPtr GetSystemMenu(IntPtr hWnd, bool bRevert);
        [DllImport("user32.dll")]
        static extern bool EnableMenuItem(IntPtr hMenu, uint uIDEnableItem, uint uEnable);



        const uint MF_BYCOMMAND = 0x00000000;
        const uint MF_GRAYED = 0x00000001;
        const uint MF_ENABLED = 0x00000000;

        const uint SC_CLOSE = 0xF060;

        const int WM_SHOWWINDOW = 0x00000018;
        const int WM_CLOSE = 0x10;


        public GuideWindow(GuideData test, int i)
        {
            _guideWindowViewModel = new GuideWindowViewModel(test, i);
            InitializeComponent();
            DataContext = _guideWindowViewModel;

            this.Title = $"LightX - {test.TestTitle}";
            //this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            //this.Left = Screen.AllScreens[0].WorkingArea.Left + this.Width / 4;
            //this.Top = Screen.AllScreens[0].WorkingArea.Height / 2 - this.Height / 2;
            //this.Left = SystemParameters.PrimaryScreenWidth;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            HwndSource hwndSource = PresentationSource.FromVisual(this) as HwndSource;

            if (hwndSource != null)
            {
                hwndSource.AddHook(new HwndSourceHook(this.hwndSourceHook));
            }
        }


        IntPtr hwndSourceHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_SHOWWINDOW)
            {
                IntPtr hMenu = GetSystemMenu(hwnd, false);
                if (hMenu != IntPtr.Zero)
                {
                    //EnableMenuItem(hMenu, SC_CLOSE, MF_GRAYED);
                    EnableMenuItem(hMenu, SC_CLOSE, MF_BYCOMMAND | MF_GRAYED);
                }
            }
            return IntPtr.Zero;
        }
    }
}
