using LightX_01.ViewModel;
using LightX_01.Classes;
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
        private LowLevelKeyboardListener _listener;
        private bool _shutterIsPressed = false;
        private bool _zoomIsPressed = false;

        //public void KeyDownEventHandler(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    switch (e.Key)
        //    {
        //    }
        //    e.Handled = true;
        //}

        //public void KeyUpEventHandler(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    switch(e.Key)
        //    {
        //    }
        //    e.Handled = true;
        //}

        public void MouseLeftButtonDownEventHandler(object sender, MouseButtonEventArgs e)
        {
            try
            {
                Point cursorPosition = e.GetPosition(e.OriginalSource as IInputElement);
                // divide by the dimension of the image shown on screen
                _cameraControlWindowViewModel.MouseLeftButtonDown(cursorPosition.X/(sender as System.Windows.Controls.Grid).DesiredSize.Width, cursorPosition.Y/(sender as System.Windows.Controls.Grid).DesiredSize.Height);
            }
            catch (Exception exception)
            {
                string ayylmao = exception.ToString();
            }
        }

        

        private void LiveViewWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _listener = new LowLevelKeyboardListener();
            _listener.OnKeyPressed += _listener_OnKeyPressed;
            _listener.OnKeyReleased += _listener_OnKeyReleased;

            _listener.HookKeyboard();
        }

        private void _listener_OnKeyReleased(object sender, KeyPressedArgs e)
        {
            switch (e.KeyPressed)
            {
                case Key.C:
                    if (_shutterIsPressed)
                    {
                        _shutterIsPressed = false;
                        _cameraControlWindowViewModel.StopBurstCapture();
                    }
                    break;
                case Key.Space:
                    if (_zoomIsPressed)
                    {
                        _zoomIsPressed = false;
                        _cameraControlWindowViewModel.SetZoom();
                    }
                    break;
            }
        }

        private void _listener_OnKeyPressed(object sender, KeyPressedArgs e)
        {
            switch (e.KeyPressed)
            {
                case Key.C:
                    if (!_shutterIsPressed)
                    {
                        _shutterIsPressed = true;
                        _cameraControlWindowViewModel.StartBurstCapture();
                    }
                    break;
                case Key.Space:
                    if (!_zoomIsPressed)
                    {
                        _zoomIsPressed = true;
                        _cameraControlWindowViewModel.ZoomOutEvent();
                    }
                    break;
                case Key.Up:
                    goto case Key.Right;
                case Key.Down:
                    goto case Key.Right;
                case Key.Left:
                    goto case Key.Right;
                case Key.Right:
                    _cameraControlWindowViewModel.MoveRoiXY(e.KeyPressed);
                    break;
            }
        }

        private void LiveViewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _listener.UnHookKeyboard();
        }

        public CameraControlWindow(Exam exam)
        {
            _cameraControlWindowViewModel = new CameraControlWindowViewModel(exam);
            InitializeComponent();
            DataContext = _cameraControlWindowViewModel;
            CheckBoxCustomBurst.IsChecked = true;
            BurstUpDownControl.IsEnabled = true;

            this.Title = $"LightX - {exam.Patient.FirstName} {exam.Patient.LastName} - {exam.ExamDate.Day:D2}/{exam.ExamDate.Month:D2}/{exam.ExamDate.Year} - {exam.ExamDate.Hour:D2}:{exam.ExamDate.Minute:D2}:{exam.ExamDate.Second:D2}";
            //this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            this.Left = Screen.AllScreens[0].WorkingArea.Right - this.Width - this.Width / 4;
            this.Top = Screen.AllScreens[0].WorkingArea.Height / 2 - this.Height / 2;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            BurstUpDownControl.IsEnabled = !BurstUpDownControl.IsEnabled;
        }
    }
}
