using LightX.Classes;
using LightX.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;

namespace LightX
{
    /// <summary>
    /// Interaction logic for GuideWindow.xaml
    /// </summary>
    /// 
    public partial class GuideWindow : Window
    {
        private readonly GuideWindowViewModel _guideWindowViewModel;

        public delegate void NextInstructionEventHandler();
        public event NextInstructionEventHandler NextInstructionEvent;

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

        internal bool NextInstruction()
        {
            bool b = false;
            if(_guideWindowViewModel.NextInstruction())
                b = true;
            TabControlGuide.SelectedIndex = _guideWindowViewModel.InstructionIndex;
            return b;
        }

        internal bool PreviousInstruction()
        {
            bool b = false;
            if (_guideWindowViewModel.PreviousInstruction())
                b = true;
            TabControlGuide.SelectedIndex = _guideWindowViewModel.InstructionIndex;
            return b;
        }

        internal GuideWindow(TestInstructions test, ObservableCollection<Tests> testList, int i)
        {
            _guideWindowViewModel = new GuideWindowViewModel(test, testList, i);
            InitializeComponent();
            DataContext = _guideWindowViewModel;

            this.Title = $"LightX - {test.TestTitle}";
            //this.Left = Screen.AllScreens[0].WorkingArea.Left + this.Width / 4;
            //this.Top = Screen.AllScreens[0].WorkingArea.Height / 2 - this.Height / 2;
            //this.Left = SystemParameters.PrimaryScreenWidth;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NextInstructionEvent();
        }
    }
}
