﻿using CameraControl.Devices;
using CameraControl.Devices.Canon;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX.Classes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Timers;

namespace LightX.ViewModel
{
    public class CameraControlWindowViewModel : ViewModelBase
    {
        #region Fields

        // Main data holder & handler
        private CameraDeviceManager _deviceManager;
        private Exam _currentExam;
        private TestResults _currentTestResults;

        // Camera settings and results
        private CameraSettings _currentTestCameraSettings;
        private BitmapImage _currentLiveViewImage;
        //private bool _liveViewEnabled;
        private bool _captureEnabled = false;
        private string _lastZoom;
        private double _subZoomDivider = 1;
        private double _lastSubZoomDivider = 1;
        private volatile bool _zoomHasChanged = true;
        private ObservableCollection<int> _roiXY = null;
        private ObservableCollection<int> ROIXYMAX;
        private ObservableCollection<int> _currentRoiXYLimits;
        private WriteableBitmap _overlayBitmap;
        private string _fileExtension;
        private volatile int _remainingBurst = 0;
        private int _totalBurstNumber = 0;
        private bool _isAutoBurstControl = false;
        public List<string> CapturedImages { get; set; }

        // System and windows vars
        private List<double> _oldGuideWindowPosition = new List<double>(2);
        private System.Timers.Timer _liveViewTimer;
        public System.Timers.Timer _burstTimer;
        public bool _shutterGotReleased = false;
        private object _locker = new object();
        private int _testIndex = 0;
        private List<Task> _tasks = new List<Task>();
        private List<Task> _lowPriorityTasks = new List<Task>();
        private FinishWindow _objFinishWindow;
        private ReviewWindow _reviewWindow;

        // Commands definition
        private RelayCommand _captureCommand;
        private ICommand _imageMouseWheelCommand;
        private ICommand _closingCommand;

        #endregion Fields

        #region Properties

        public CameraDeviceManager DeviceManager
        {
            get { return _deviceManager; }
            set
            {
                if (value != _deviceManager)
                {
                    _deviceManager = value;
                    RaisePropertyChanged(() => DeviceManager);
                }
            }
        }

        public Exam CurrentExam
        {
            get { return _currentExam; }
            set
            {
                if (value != _currentExam)
                {
                    _currentExam = value;
                    RaisePropertyChanged(() => CurrentExam);
                }
            }
        }

        public BitmapImage CurrentLiveViewImage
        {
            get { return _currentLiveViewImage; }
            set
            {
                if (value != _currentLiveViewImage)
                {
                    _currentLiveViewImage = value;
                    RaisePropertyChanged(() => CurrentLiveViewImage);
                }
            }
        }

        public WriteableBitmap OverlayBitmap
        {
            get { return _overlayBitmap; }
            set
            {
                if (value != _overlayBitmap)
                {
                    _overlayBitmap = value;
                    RaisePropertyChanged(() => OverlayBitmap);
                }
            }
        }

        public ObservableCollection<int> RoiXY
        {
            get { return _roiXY; }
            set
            {
                if (value != _roiXY)
                {
                    _roiXY = value;
                    RaisePropertyChanged(() => RoiXY);
                    new Thread(() =>
                    {
                        bool retry;
                        Thread.CurrentThread.Name = "SetRoiXY";
                        Thread.CurrentThread.IsBackground = true;
                        do
                        {
                            retry = false;
                            try
                            {
                                DeviceManager.SelectedCameraDevice.Focus(_roiXY[0], _roiXY[1]);
                            }
                            catch (DeviceException exception)
                            {
                                if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                                {
                                    // this may cause infinite loop
                                    Thread.Sleep(100);
                                    retry = true;
                                }
                                else
                                {
                                    System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                                }
                            }
                            catch (Exception err)
                            {
                                // might access a null var
                                System.Windows.MessageBox.Show("Error occurred :" + err.Message);
                            }
                        } while (retry);
                    }).Start();
                }
            }
        }

        internal void ReconnectCamera()
        {
            _liveViewTimer.Stop();
            Console.WriteLine("Trying to reconnect to the camera...");
            //if (_liveViewTimer.Enabled)
            //DeviceManager.CloseAll();
            //DeviceManager.SelectedCameraDevice.Close();
            //DeviceManager.ConnectToCamera();
            //Thread.Sleep(700);
            //DeviceManager.SelectNextCamera();
            //Thread.Sleep(700);

            if (DeviceManager.SelectedCameraDevice is CanonSDKBase || DeviceManager.SelectedCameraDevice is NikonBase)
            {
                //LiveViewEnabled = true;
                ResetZoom();
                StartLiveViewInThread();
            }
        }

        //public bool LiveViewEnabled
        //{
        //    get { return _liveViewEnabled; }
        //    set
        //    {
        //        if (value != _liveViewEnabled)
        //        {
        //            _liveViewEnabled = value;
        //            RaisePropertyChanged(() => LiveViewEnabled);
        //        }
        //    }
        //}

        public bool CaptureEnabled
        {
            get { return _captureEnabled; }
            set
            {
                if (value != _captureEnabled)
                {
                    _captureEnabled = value;
                    RaisePropertyChanged(() => CaptureEnabled);
                }
            }
        }

        public int BurstNumber
        {
            get
            {
                if (_currentTestCameraSettings != null)
                    return Int32.Parse(_currentTestCameraSettings.BurstNumber);
                else
                    return 0;
            }
            set
            {
                if (value != Int32.Parse(_currentTestCameraSettings.BurstNumber))
                {
                    _currentTestCameraSettings.BurstNumber = value.ToString();
                    if (DeviceManager.SelectedCameraDevice is NikonBase)
                        DeviceManager.SelectedCameraDevice.AdvancedProperties[2].SetValue(_currentTestCameraSettings.BurstNumber);
                    RaisePropertyChanged(() => BurstNumber);
                }
            }
        }

        public bool IsAutoBurstControl
        {
            get { return _isAutoBurstControl; }
            set
            {
                if (value != _isAutoBurstControl)
                {
                    _isAutoBurstControl = value;
                    RaisePropertyChanged(() => IsAutoBurstControl);
                }
            }
        }

        #endregion Properties

        #region RelayCommands

        public RelayCommand CaptureCommand
        {
            get
            {
                if (_captureCommand == null)
                    _captureCommand = new RelayCommand(CaptureInThread, true);
                return _captureCommand;
            }
        }

        public ICommand ImageMouseWheelCommand
        {
            get
            {
                if (_imageMouseWheelCommand == null)
                    _imageMouseWheelCommand = new RelayCommand<MouseWheelEventArgs>(
                        param => ImageMouseWheel(param)
                        );
                return _imageMouseWheelCommand;
            }
        }

        public ICommand ClosingCommand
        {
            get
            {
                if (_closingCommand == null)
                    _closingCommand = new RelayCommand<CancelEventArgs>(
                        param => CloseApplication(param)
                        );
                return _closingCommand;
            }
        }

        #endregion RelayCommands

        #region SystemCommands

        private void SetLiveViewTimer()
        {
            _liveViewTimer = new System.Timers.Timer(1000 / 60);
            _liveViewTimer.Elapsed += _liveViewTimer_Tick;
            _liveViewTimer.AutoReset = true;
        }

        private void SetBurstTimer()
        {
            _burstTimer = new System.Timers.Timer(9 * 135);
            _burstTimer.Elapsed += _bustTimer_Tick;
            _burstTimer.AutoReset = true;
        }

        private void _bustTimer_Tick(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Max ammount of pictures reached.");
            
            StopBurstCapture();
        }

        private void _liveViewTimer_Tick(object sender, EventArgs e)
        {
            LiveViewData liveViewData = null;
            try
            {
                liveViewData = DeviceManager.SelectedCameraDevice.GetLiveViewImage();
            }
            catch (Exception)
            { return; }

            if (liveViewData == null || liveViewData.ImageData == null)
                return;

            /* Set le ROI du live view (and get the dimensions of the bitmapImage) */
            if (RoiXY == null)
            {
                ROIXYMAX = new ObservableCollection<int>() { liveViewData.ImageWidth, liveViewData.ImageHeight };
                SetRoiXY(ROIXYMAX[0]/2, ROIXYMAX[1]/2);
            }

            if (_zoomHasChanged)
            {
                double ratioX = GetZoomRatio(liveViewData.LiveViewImageWidth, liveViewData.ImageWidth);
                double ratioY = GetZoomRatio(liveViewData.LiveViewImageHeight, liveViewData.ImageHeight);
                _currentRoiXYLimits = new ObservableCollection<int>
                {
                    (int)(ratioX*liveViewData.LiveViewImageWidth/2),
                    ROIXYMAX[0] - (int)(ratioX*liveViewData.LiveViewImageWidth/2),
                    (int)(ratioY*liveViewData.LiveViewImageHeight/2),
                    ROIXYMAX[1] - (int)(ratioY*liveViewData.LiveViewImageHeight/2)
                };
                
                _zoomHasChanged = false;
            }

            try
            {
                if (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value == DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0] && _subZoomDivider == 1)
                    OverlayBitmap = DrawFocusPoint(liveViewData);
                else
                    OverlayBitmap = null;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                _liveViewTimer.Stop();
                return;
            }
            
                
            try
            {
                BitmapImage image = new BitmapImage();

                using (MemoryStream stream = new MemoryStream(liveViewData.ImageData,
                                                                liveViewData.ImageDataPosition,
                                                                liveViewData.ImageData.Length -
                                                                liveViewData.ImageDataPosition, false))
                {
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.SourceRect = GetSourceRect(liveViewData); // Crop the image to get smaller zoom steps
                    image.EndInit();
                    image.Freeze();
                }
                CurrentLiveViewImage = image;
                image.StreamSource.Close();
                image.StreamSource.Dispose();
            }
            catch (Exception err)
            {
                err.GetHashCode();
            }
        }

        private static int Clamp(int value, int min, int max)
        {
            return (value < min) ? min : (value > max) ? max : value;
        }

        #endregion SystemCommands

        #region EventHandlers

        private void ImageMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            //new Thread(() =>
            //{
                //Thread.CurrentThread.Name = "ImageMouseWheel";
                //Thread.CurrentThread.IsBackground = true;
                if (e.Delta > 0)
                {
                    if (_subZoomDivider == 1)
                        _subZoomDivider = 1.25;
                    else if ( _subZoomDivider < 2.75 && DeviceManager.SelectedCameraDevice is CanonSDKBase)
                    {
                        _subZoomDivider += 0.75;
                    }
                    else if (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value != DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values.Count - 2])
                    {
                        DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.NextValue();
                        DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.SetValue(DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value, true);
                        _subZoomDivider = 1;
                    }
                }
                else if (e.Delta < 0)
                {
                    if (_subZoomDivider == 1.25)
                        _subZoomDivider = 1;
                    else if (_subZoomDivider >= 1.50 && DeviceManager.SelectedCameraDevice is CanonSDKBase)
                    {
                        _subZoomDivider -= 0.75;
                    }
                    else if (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value != DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0])
                    {
                        DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.PrevValue();
                        _subZoomDivider = DeviceManager.SelectedCameraDevice is CanonSDKBase ? 2.75: 1.25;
                    }
                }
            //}).Start();
            _zoomHasChanged = true;
        }

        public void MouseLeftButtonDown(double x, double y)
        {
            // don't want the click to focus action when already zoomed in?
            //x*(liveViewSize/imageSizeOnScreen)*(fullResolutionSize/liveViewSize) = x * fullResolutionSize / imageSizeOnScreen
            if (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value == DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0] && _subZoomDivider == 1)
                SetRoiXY((int)(x * (double)ROIXYMAX[0]), (int)(y * (double)ROIXYMAX[1]));
        }

        public void ZoomOutEvent(bool canZoom)
        {
            if (canZoom)
            {
                _lastZoom = DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value;
                _lastSubZoomDivider = _subZoomDivider;
                SetZoom(DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0]);
                _subZoomDivider = 1;
            }
        }

        public void SetZoom(string desiredZoom = null)
        {
            if (desiredZoom == null)
            {
                desiredZoom = _lastZoom;
                _subZoomDivider = _lastSubZoomDivider;
            }

            try
            {
                if (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value != desiredZoom)
                    DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.SetValue(desiredZoom, true);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.Message);
            }

            _zoomHasChanged = true;
        }

        void SelectedCamera_CameraInitDone(ICameraDevice cameraDevice)
        {
            Console.WriteLine("Event CameraInitDone : {0}", cameraDevice.DeviceName);
            //ApplyCameraSettings(cameraDevice);

            if (_currentTestCameraSettings != null)
                ApplyCameraSettings(cameraDevice, _currentTestCameraSettings);

            if (CaptureEnabled)
                StartLiveViewInThread(cameraDevice);

            //if (LiveViewEnabled)
            //{
            //    //System.Windows.Application.Current.Dispatcher.Invoke(() =>
            //    //{
            //    //    StartLiveViewInThread();
            //    //});
            //}
        }

        void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            Console.WriteLine("Event CameraSelected : {0}", newcameraDevice.DeviceName);

            if (oldcameraDevice != newcameraDevice)
            {
                //LiveViewEnabled = newcameraDevice.GetCapability(CapabilityEnum.LiveView);
                //if (_liveViewTimer.Enabled && (oldcameraDevice is NikonBase || oldcameraDevice is CanonSDKBase))
                //{
                //    // if oldcameraDevice still exist (not disconnected), need to remove handle of disconnection
                //    //oldcameraDevice.CameraDisconnected -= SelectedCamera_CameraDisconnected;
                //    //oldcameraDevice.CaptureCompleted -= SelectedCamera_CaptureCompleted;
                //    oldcameraDevice.CameraInitDone -= SelectedCamera_CameraInitDone;
                //    StopLiveViewInThread(oldcameraDevice);
                //}

                //if (LiveViewEnabled && (newcameraDevice is CanonSDKBase))
                //{
                //    // Canon EOS R is too quick and already initialized when we get here
                //    SelectedCamera_CameraInitDone(newcameraDevice);
                //}
                //newcameraDevice.CameraInitDone += SelectedCamera_CameraInitDone;

                if (newcameraDevice is CanonSDKBase)
                    SelectedCamera_CameraInitDone(newcameraDevice);
            }
        }

        void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
        {
            Console.WriteLine("Event CameraConnected : {0}", cameraDevice.DeviceName);
            cameraDevice.CameraInitDone += SelectedCamera_CameraInitDone;
        }

        void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            ++_remainingBurst;
            ++_totalBurstNumber;

            //Stream stream = new MemoryStream();
            var fileName = Path.GetTempFileName();
            File.Delete(fileName);
            fileName = Path.ChangeExtension(fileName, null); // Remove the .tmp extension

            //byte[] buffer;

            lock (_locker) // Prevent program to start new transfer while another transfer is active.
            {
                //PhotoCaptured(eventArgs);
                //GetImageStream(eventArgs, stream);
                SaveImageToFile(eventArgs, fileName);
            }


            if (DeviceManager.SelectedCameraDevice is NikonBase)
                StartLiveViewInThread();
            _liveViewTimer.Start();
            

            GC.KeepAlive(eventArgs);
            GC.KeepAlive(sender);

            //byte[] buffer = (stream as MemoryStream).ToArray();
            //stream.Close();
            //_tasks.Add(new Task(() => ProcessImage(stream)));

            _tasks.Add(new Task(() => ProcessImageFromFile(fileName)));

            Console.WriteLine($"_remainingBurst = {_remainingBurst}; _totalBurstNumber = {_totalBurstNumber}; _remainingBurst = {_remainingBurst};");
            //if ( (_remainingBurst == 0 && !_isAutoBurstControl) || (_isAutoBurstControl && _totalBurstNumber == BurstNumber && _remainingBurst == 0) )
            if ( (_remainingBurst == 0 && !_isAutoBurstControl) || (_isAutoBurstControl && _remainingBurst == 0 && DeviceManager.SelectedCameraDevice is CanonSDKBase) || (_isAutoBurstControl && _totalBurstNumber == BurstNumber && _remainingBurst == 0 && DeviceManager.SelectedCameraDevice is NikonBase))
            {
                foreach (Task task in _tasks)
                {
                    if (!task.IsCompleted)
                        task.Start();
                }
                Task.WaitAll(_tasks.ToArray());

                foreach (Task task in _lowPriorityTasks)
                {
                    if (!task.IsCompleted)
                        task.Start();
                }

                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    Thread.Sleep(10);
                    CaptureEnabled = true;

                    SetZoom(_lastZoom);
                    SetDepthOfField(true);

                    if (_reviewWindow == null)
                        ShowReviewWindow();
                    else
                        _reviewWindow.RefreshReviewImages(CapturedImages);
                    //GC.KeepAlive(CapturedImages);
                });
                _tasks.Clear();
            }
        }

        void DeviceManager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            Console.WriteLine("Event CameraDisconnected : {0}", cameraDevice.DeviceName);
            cameraDevice.CameraInitDone -= SelectedCamera_CameraInitDone;
            _liveViewTimer.Stop();
            DeviceManager.SelectedCameraDevice = null;
            //LiveViewEnabled = false;
        }

        #endregion EventHandlers

        #region CameraCommands

        private void ApplyCameraSettings(ICameraDevice cameraDevice, CameraSettings cameraSettings)
        {
            if (cameraDevice is CanonSDKBase)
            {
                foreach (var propertyVal in cameraDevice.AdvancedProperties)
                {
                    switch (propertyVal.Name)
                    {
                        case "Drive Mode":
                            propertyVal.SetValue(propertyVal.Values[1]); //High-Speed Continuous shooting
                            break;
                        case "Flash":
                            propertyVal.SetValue(propertyVal.Values[0]);
                            break;
                        default:
                            break;
                    }
                }
                _fileExtension = ".cr3";
                SetDepthOfField(true);
                cameraDevice.IsoNumber.SetValue(cameraSettings.Iso);
                cameraDevice.CompressionSetting.SetValue(cameraDevice.CompressionSetting.Values[8]); // "JPEG (Smalest)" = 6 / RAW = 8 / RAW + JPEG = 7
            }
            else if (cameraDevice is NikonBase)
            {
                foreach (var propertyVal in cameraDevice.AdvancedProperties)
                {
                    switch (propertyVal.Name)
                    {
                        case "Burst Number":
                            propertyVal.SetValue(cameraSettings.BurstNumber); // Burst de n photos = [n - 1]
                            break;
                        case "Still Capture Mode":
                            propertyVal.SetValue(propertyVal.NumericValues[1]); // "Continuous high-speed shooting (CH)"
                            break;
                        case "Auto Iso":
                            propertyVal.SetValue(propertyVal.NumericValues[0]); // Auto Iso OFF = 0 // ON = 1
                            break;
                        case "Flash":
                            propertyVal.SetValue(propertyVal.NumericValues[0]); // Flash prohibited = 0 / Normal synch = 2 / slow synch = 3
                            break;
                        case "Color space":
                            propertyVal.SetValue(propertyVal.NumericValues[0]); // Adobe RGB = 1
                            break;
                        case "Flash sync speeeeeeeed":
                            propertyVal.SetValue(propertyVal.NumericValues[0]); // 1/320
                            break;
                        case "Active D-Lighting":
                            propertyVal.SetValue(propertyVal.NumericValues[4]); // Not Performed
                            break;
                    }
                }
                _fileExtension = ".NEF";
                cameraDevice.FocusMode.SetValue(cameraDevice.FocusMode.Values[0]); // "AF-S"
                cameraDevice.CompressionSetting.SetValue(cameraDevice.CompressionSetting.Values[3]); // "JPEG (BASIC)" = 0 / RAW = 3 / RAW + JPEG = 4
            }

            if (!(cameraDevice is CanonSDKBase || cameraDevice is NikonBase))
                return; // if cameraDevice is FakeCamera

            cameraDevice.ShutterSpeed.SetValue(cameraSettings.ShutterSpeed);
            cameraDevice.FNumber.SetValue(cameraSettings.FNumber);
            if (_lastZoom != null)
                SetZoom(_lastZoom);
            else
                SetZoom(cameraDevice.LiveViewImageZoomRatio.Values[0]);

            _subZoomDivider = _lastSubZoomDivider;

            _roiXY = null; // will reset the focus point to the center of the image;
            RaisePropertyChanged(() => BurstNumber);

            CaptureEnabled = true;
            Console.WriteLine("Camera settings applied");
        }

        private void CaptureInThread()
        {
            CaptureEnabled = false;

            _lastZoom = DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value;
            _lastSubZoomDivider = _subZoomDivider;

            if (DeviceManager.SelectedCameraDevice is NikonBase)
            {
                if (_liveViewTimer.Enabled)
                {
                    StopLiveViewInThread();
                    Thread.Sleep(100); // wait for the liveView to stop
                }
                new Thread(Capture).Start();
            }
            else if (DeviceManager.SelectedCameraDevice is CanonSDKBase)
            {
                _liveViewTimer.Stop();

                try
                {
                    int burst = BurstNumber;
                    int delay = 0;
                    // no thread, since we control the burst number on timing alone
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).CapturePhotoBurstNoAf();
                    switch (burst)
                    {
                        case 1:
                            break;
                        case 2:
                            delay = 100;
                            break;
                        case 8:
                            delay = 135;
                            break;
                        case 9:
                            delay = 135;
                            break;
                        default:
                            delay = 130;
                            break;
                    }
                    do {
                        Thread.Sleep(delay);
                        Console.WriteLine("Capture #{0}", Int32.Parse(_currentTestCameraSettings.BurstNumber) - burst + 1);
                        burst--;
                    } while (burst != 0);
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                    SetDepthOfField(false);
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                    //Thread.Sleep(150);
                }
                catch (COMException comException)
                {
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                    DeviceManager.SelectedCameraDevice.IsBusy = false;
                    ErrorCodes.GetException(comException);
                }
                catch
                {
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                    DeviceManager.SelectedCameraDevice.IsBusy = false;
                    throw;
                }
            }
        }

        public void StartBurstCapture()
        {

            Console.WriteLine("Shutter pressed");

            if (CaptureEnabled)
            {

                if (DeviceManager.SelectedCameraDevice is NikonBase)
                {
                    CaptureInThread();
                    return;
                }

                _shutterGotReleased = false;
                CaptureEnabled = false;
                if (!IsAutoBurstControl)
                {
                    _lastZoom = DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value;
                    _lastSubZoomDivider = _subZoomDivider;

                    try
                    {
                        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                        {
                            _liveViewTimer.Stop();
                            
                            (DeviceManager.SelectedCameraDevice as CanonSDKBase).CapturePhotoBurstNoAf();
                            
                            //CameraControlWindow currentWindow = null;
                            //foreach (Window window in System.Windows.Application.Current.Windows)
                            //{
                            //    if (window.GetType() == typeof(CameraControlWindow))
                            //    {
                            //        currentWindow = (window as CameraControlWindow);
                            //    }
                            //}
                            //while (!Keyboard.IsKeyUp(Key.C)) { currentWindow.Activate(); } //Thread.Sleep(2); }
                            //Console.WriteLine("Shutter released");
                            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.DepthOfFieldPreview = false;
                            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                        });
                        _burstTimer.Start();
                    }
                    catch (COMException comException)
                    {
                        (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                        (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                        DeviceManager.SelectedCameraDevice.IsBusy = false;
                        ErrorCodes.GetException(comException);
                    }
                    catch (DeviceException exception)
                    {
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                        (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                        (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                        DeviceManager.SelectedCameraDevice.IsBusy = false;
                    }
                    catch
                    {
                        (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                        (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                        DeviceManager.SelectedCameraDevice.IsBusy = false;
                        throw;
                    }
                }
                else
                    CaptureInThread();
            }
        }

        public void StopBurstCapture()
        {
            _burstTimer.Stop();
            Console.WriteLine("Shutter released");

            if (DeviceManager.SelectedCameraDevice is NikonBase)
                return;

            if(!IsAutoBurstControl && !_shutterGotReleased)
            {
                _shutterGotReleased = true;
                try
                {
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                    Thread.Sleep(20);
                    SetDepthOfField(false);
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                }
                catch (Exception exception)
                {
                    System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).ResetShutterButton();
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResumeLiveview();
                    DeviceManager.SelectedCameraDevice.IsBusy = false;
                }
            }
        }

        private void Capture() // FOR NIKON CAMERA ONLY
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
                }
                catch (DeviceException exception)
                {
                    // if device is bussy retry after 100 miliseconds
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy ||
                        exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // !!!!this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        if (exception.ErrorCode == ErrorCodes.MTP_Out_of_Focus) // restart the liveView when can't focus
                            StartLiveViewInThread();
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error occurred :" + ex.Message);
                }
            } while (retry);
        }

        private void SaveImageToFile(object o, string path)
        {
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;

            if (eventArgs == null)
                return;

            try
            {
                eventArgs.CameraDevice.IsBusy = true;

                Console.WriteLine("Download of {0} started...", eventArgs.FileName);
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, path + _fileExtension);
                GC.KeepAlive(eventArgs);
                //memStream.Position = 0;
                Console.WriteLine("Download of {0} finished", eventArgs.FileName);

                --_remainingBurst;
            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
                return;
            }

            eventArgs.CameraDevice.IsBusy = false;
            eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
        }

        private void GetImageStream(object o, Stream memStream)
        {
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;

            if (eventArgs == null)
                return;

            try
            {
                eventArgs.CameraDevice.IsBusy = true;

                Console.WriteLine("Download of {0} started...", eventArgs.FileName);
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);
                GC.KeepAlive(eventArgs);
                memStream.Position = 0;
                Console.WriteLine("Download of {0} finished", eventArgs.FileName);

                --_remainingBurst;
            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
                return;
            }

            eventArgs.CameraDevice.IsBusy = false;
            eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
        }

        private byte[] GetImageBuffer(object o)
        {

            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;

            if (eventArgs == null)
                return null;

            try
            {
                eventArgs.CameraDevice.IsBusy = true;
                byte[] buffer = (eventArgs.CameraDevice as CanonSDKBase).TransferFile(eventArgs.Handle);

                --_remainingBurst;

                eventArgs.CameraDevice.IsBusy = false;
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
                return buffer;
            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
                System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
                return null;
            }
        }

        //private void PhotoCaptured(object o)
        //{

        //    if (CapturedImages == null)
        //        CapturedImages = new ObservableCollection<BitmapImage>();
        //    PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;

        //    if (eventArgs == null)
        //        return;

        //    try
        //    {
        //        eventArgs.CameraDevice.IsBusy = true;

        //        //while (eventArgs.CameraDevice.TransferProgress != 100u && eventArgs.CameraDevice.TransferProgress != 0u) { }
        //        //using (Stream memStream = new MemoryStream())
        //        //{
        //        Stream memStream = new MemoryStream();
        //        Stream mem = new MemoryStream();
        //        //Thread.Sleep(100);
        //        //GC.KeepAlive(eventArgs);
        //        eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);
        //            //GC.KeepAlive(eventArgs);
        //            //while (eventArgs.CameraDevice.TransferProgress != 100u && eventArgs.CameraDevice.TransferProgress != 0u) { }
        //            memStream.Position = 0;

        //            string fileName = Path.GetFileName(eventArgs.FileName);
        //            if (Path.GetExtension(fileName) == ".NEF" || Path.GetExtension(fileName) == ".CR2" || Path.GetExtension(fileName) == ".CR3")
        //            {
        //                byte[] rawData;
        //                rawData = new byte[memStream.Length];
        //                memStream.Read(rawData, 0, (int)memStream.Length);

        //                GCHandle rawDataHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
        //                IntPtr address = rawDataHandle.AddrOfPinnedObject();

        //                System.Windows.Media.PixelFormat pf = PixelFormats.Rgb24;
        //                int width = 6720;
        //                int height = 4480;
        //                int rawStride = (width * pf.BitsPerPixel + 7) / 8;
        //                byte[] result = new byte[rawStride * height];


        //                //byte[] result = new byte[6720 * 4480 * 3 * 1];
        //                //byte[] result = new byte[6720 * 4480 * 3 * 2];
        //                //GCHandle resultHandle = GCHandle.Alloc(result, GCHandleType.Pinned);
        //                //IntPtr resAdd = resultHandle.AddrOfPinnedObject();

        //                int size = LibrawClass.extractThumb(address, rawData.Length);

        //                //Thread.Sleep(100);
        //                rawDataHandle.Free();
        //                //Thread.Sleep(100);
        //                Marshal.Copy(address, result, 0, size);
        //                //Thread.Sleep(100);
        //                //resultHandle.Free();


        //                mem.Write(result, 0, size);
        //                /*
        //                BitmapSource bitmap = BitmapSource.Create(width, height, 300, 300, pf, null, result, rawStride);
        //                //BitmapSource bitmap = BitmapSource .Create()
        //                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //                encoder.Frames.Add(BitmapFrame.Create(bitmap));
        //                encoder.Save(mem);
        //                */
        //                mem.Position = 0;

        //                BitmapImage image = new BitmapImage();
        //                image.BeginInit();
        //                image.StreamSource = mem;
        //                image.CacheOption = BitmapCacheOption.None;
        //                image.EndInit();
        //                image.Freeze();
        //                //mem.Close();
        //                CapturedImages.Add(image);

        //                --_remainingBurst;
        //                //memStream.Close();
        //            }
        //            else // JPEG
        //            {
        //                BitmapImage image = new BitmapImage();
        //                image.BeginInit();
        //                image.StreamSource = memStream;
        //                image.CacheOption = BitmapCacheOption.Default;
        //                image.EndInit();
        //                image.Freeze();
        //                //memStream.Close();
        //                CapturedImages.Add(image);

        //                --_remainingBurst;
        //            }

        //            //GC.KeepAlive(eventArgs);
        //            eventArgs.CameraDevice.IsBusy = false;
        //            eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
        //         //}

        //    }
        //    catch (Exception exception)
        //    {
        //        eventArgs.CameraDevice.IsBusy = false;
        //        System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
        //    }
        //    //GC.Collect();

        //    if (_remainingBurst == 0 && CapturedImages.Count != 0) // (wait for all captureEvents to be process before showing the review window)
        //        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            //_totalBurstNumber /= 2; // if JPEG + RAW
        //            ShowReviewWindow();
        //        });
        //}

        //private void PhotoCaptured_OLD(object o)
        //{
        //    if (CapturedImages == null)
        //        CapturedImages = new ObservableCollection<BitmapImage>();
        //    PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;

        //    if (eventArgs == null)
        //        return;
        //    try
        //    {
        //        //eventArgs.CameraDevice.TransferFile(eventArgs.Handle, fileName);
        //        //// the IsBusy may used internally, if file transfer is done should set to false  
        //        //eventArgs.CameraDevice.IsBusy = false;
        //        //eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);

        //        //if (Path.GetExtension(fileName) == ".NEF" || Path.GetExtension(fileName) == ".CR2" || Path.GetExtension(fileName) == ".CR3")
        //        //{
        //        //    bool OnlyRaw = false; //to restart the liveView when only uploading raw files (gotta do the raw->tiff->bitmap function)
        //        //    if(OnlyRaw)
        //        //        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //        //        {
        //        //            StartLiveViewInThread();
        //        //        });
        //        //    return;
        //        //}

        //        eventArgs.CameraDevice.IsBusy = true;

        //        //BitmapImage image;

        //        //Stream memStream = new MemoryStream();

        //        while (eventArgs.CameraDevice.TransferProgress != 100u && eventArgs.CameraDevice.TransferProgress != 0u) { }
        //        byte[] rawData;
        //        using (Stream memStream = new MemoryStream())
        //        {
        //            eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);

        //            memStream.Position = 0;

        //            rawData = new byte[memStream.Length];
        //            memStream.Read(rawData, 0, (int)memStream.Length);
        //        }

        //        GCHandle rawDataHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
        //        IntPtr address = rawDataHandle.AddrOfPinnedObject();

        //        System.Windows.Media.PixelFormat pf = PixelFormats.Rgb48;
        //        int width = 6742;
        //        int height = 4498;
        //        int rawStride = (width * pf.BitsPerPixel + 7) / 8;
        //        byte[] result = new byte[rawStride * height];


        //        //byte[] result = new byte[6742 * 4498 * 3 * 2];
        //        GCHandle resultHandle = GCHandle.Alloc(result, GCHandleType.Pinned);
        //        IntPtr resAdd = resultHandle.AddrOfPinnedObject();

        //        resAdd = LibrawClass.processRawImage(address, rawData.Length);

        //        Thread.Sleep(100);

        //        rawDataHandle.Free();

        //        //byte[] buff = new byte[6742 * 4498 * 3 * 2];

        //        Thread.Sleep(100);


        //        Marshal.Copy(resAdd, result, 0, result.Length);
        //        Thread.Sleep(100);

        //        resultHandle.Free();

        //        MemoryStream mem = new MemoryStream();
        //        //mem.Write(result, 0, result.Length);



        //        BitmapSource bitmap = BitmapSource.Create(width, height, 96, 96, pf, null, result, rawStride);

        //        //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
        //        BmpBitmapEncoder encoder = new BmpBitmapEncoder();
        //        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        //        encoder.Save(mem);
        //        mem.Position = 0;

        //        BitmapImage image = new BitmapImage();
        //        image.BeginInit();
        //        image.StreamSource = mem;
        //        image.CacheOption = BitmapCacheOption.OnLoad;
        //        image.EndInit();
        //        image.Freeze();
        //        mem.Close();
        //        CapturedImages.Add(image);




        //        // Gestion temporaire des fichier RAW


        //        string fileName = Path.GetFileName(eventArgs.FileName);
        //        /*
        //        if(Path.GetExtension(fileName) == ".NEF" || Path.GetExtension(fileName) == ".CR2" || Path.GetExtension(fileName) == ".CR3")
        //        {
        //            using (MagickImage magickImage = new MagickImage(memStream, MagickFormat.Cr3))
        //            {
        //                memStream.Close();
        //                GC.Collect();
        //                //IPixelCollection pc = magickImage.GetPixels();
        //                //byte[] data = pc.ToByteArray("RGB");
        //                //BitmapSource bmf = FromArray(data, magickImage.Width, magickImage.Height, magickImage.ChannelCount);


        //                System.Drawing.Bitmap bmp = magickImage.ToBitmap(System.Drawing.Imaging.ImageFormat.Tiff);

        //                //System.Drawing.Bitmap uwu = new System.Drawing.Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format48bppRgb);

        //                //using (Graphics graph = Graphics.FromImage(uwu))
        //                //{
        //                //    graph.DrawImage(bmp, new System.Drawing.Point(0, 0));
        //                //}

        //                //bmp.PixelFormat = System.Drawing.Imaging.PixelFormat.Format48bppRgb;

        //                //Bitmap ayy;

        //                //System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format48bppRgb;

        //                //Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

        //                //BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
        //                //try
        //                //{
        //                //    Bitmap convertedBitmap = new Bitmap(bmp.Width, bmp.Height, pixelFormat);
        //                //    BitmapData convertedBitmapData = convertedBitmap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
        //                //    try
        //                //    {
        //                //        NativeMethods.CopyMemory(convertedBitmapData.Scan0, bitmapData.Scan0, (uint)bitmapData.Stride * (uint)bitmapData.Height);
        //                //    }
        //                //    finally
        //                //    {
        //                //        convertedBitmap.UnlockBits(convertedBitmapData);
        //                //    }

        //                //    ayy = convertedBitmap;
        //                //}
        //                //finally
        //                //{
        //                //    bmp.UnlockBits(bitmapData);
        //                //}

        //                magickImage.Dispose();
        //                GC.Collect();

        //                using (var mem = new MemoryStream())
        //                {
        //                    bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Tiff);
        //                    bmp.Dispose();
        //                    mem.Position = 0;

        //                    BitmapImage image = new BitmapImage();
        //                    image.BeginInit();
        //                    image.StreamSource = mem;
        //                    image.CacheOption = BitmapCacheOption.OnLoad;
        //                    image.EndInit();
        //                    image.Freeze();

        //                    CapturedImages.Add(image);
        //                    mem.Close();
        //                }

        //                //    bmp.Save()
        //                //IPixelCollection pc = magickImage.GetPixels();

        //                //byte[] data = pc.ToByteArray(PixelMapping.BGR);

        //                //ushort[] data = pc.ToArray();
        //                //BitmapSource bmf = FromArray(data, magickImage.Width, magickImage.Height, magickImage.ChannelCount);

        //                //BitmapSource bmf = magickImage.ToBitmapSource(BitmapDensity.)

        //                // conversion en bitmap image

        //                //BitmapImage image = Bs2bi(bmf);


        //                //image.Freeze();
        //                //CapturedImages.Add(image);
        //            }
        //        }
        //        else // JPEG 
        //        {
        //            //Stream memStream = new MemoryStream();

        //            // eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);

        //            //memStream.Position = 0;

        //            BitmapImage image = new BitmapImage();
        //            image.BeginInit();
        //            image.StreamSource = memStream;
        //            image.CacheOption = BitmapCacheOption.OnLoad;
        //            image.EndInit();
        //            image.Freeze();
        //            memStream.Close();
        //            CapturedImages.Add(image);
        //        }
        //        */
        //        --_remainingBurst;

        //        eventArgs.CameraDevice.IsBusy = false;
        //        eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
        //    }
        //    catch (Exception exception)
        //    {
        //        eventArgs.CameraDevice.IsBusy = false;
        //        System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
        //    }
        //    GC.Collect();

        //    if (_remainingBurst == 0) // (wait for all captureEvents to be process before showing the review window)
        //        System.Windows.Application.Current.Dispatcher.Invoke(() =>
        //        {
        //            ShowReviewWindow();
        //        });

        //}

        public void SetRoiXY(int x, int y)
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    RoiXY = new ObservableCollection<int>() { Clamp(x, 0, ROIXYMAX[0]), Clamp(y, 0, ROIXYMAX[1]) };
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }
            } while (retry);
        }

        public void MoveRoiXY(Key key, bool canMove)
        {
            if(canMove)
            {
                int stepSize;
                bool retry;
                switch (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value)
                {
                    case "33%":
                        stepSize = 150;
                        break;
                    case "50%":
                        stepSize = 100;
                        break;
                    case "66%":
                        stepSize = 50;
                        break;
                    case "100%":
                        stepSize = 25;
                        break;
                    default: // case "All" + "25%"
                        stepSize = 200;
                        break;
                }

                do // to deal if you press the direction keys too quickly
                {
                    retry = false;
                    try
                    {
                        switch (key)
                        {
                            case Key.Up:
                                SetRoiXY(RoiXY[0], Clamp(RoiXY[1] - stepSize, _currentRoiXYLimits[2], _currentRoiXYLimits[3]));
                                break;
                            case Key.Down:
                                SetRoiXY(RoiXY[0], Clamp(RoiXY[1] + stepSize, _currentRoiXYLimits[2], _currentRoiXYLimits[3]));
                                break;
                            case Key.Left:
                                SetRoiXY(Clamp(RoiXY[0] - stepSize, _currentRoiXYLimits[0], _currentRoiXYLimits[1]), RoiXY[1]);
                                break;
                            case Key.Right:
                                SetRoiXY(Clamp(RoiXY[0] + stepSize, _currentRoiXYLimits[0], _currentRoiXYLimits[1]), RoiXY[1]);
                                break;
                        }
                    }
                    catch (DeviceException exception)
                    {
                        if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                        {
                            // this may cause infinite loop
                            Thread.Sleep(100);
                            retry = true;
                        }
                        else
                        {
                            System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                        }
                    }
                } while (retry);
            }
        }

        public void StartLiveViewInThread(ICameraDevice cameraDevice = null)
        {
            if (cameraDevice == null)
                cameraDevice = DeviceManager.SelectedCameraDevice;

            if (cameraDevice is NikonBase || cameraDevice is CanonSDKBase)
            {
                Thread thread = new Thread(() => StartLiveView(cameraDevice));
                thread.Name = "StartLiveView";
                thread.Start();
            }
            else
                Console.WriteLine("Tried to start LiveView with a fake camera.");
        }

        private void StartLiveView(ICameraDevice cameraDevice)
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    //while (DeviceManager.SelectedCameraDevice.IsBusy) { }
                    cameraDevice.StartLiveView();
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }
            } while (retry);
            _liveViewTimer.Start();
            //cameraDevice.IsBusy = false;
        }

        private void StopLiveViewInThread(ICameraDevice cameraDevice = null)
        {
            if (cameraDevice == null)
                cameraDevice = DeviceManager.SelectedCameraDevice;

            if (cameraDevice is NikonBase || cameraDevice is CanonSDKBase)
            {
                Thread thread = new Thread(() => StopLiveView(cameraDevice));
                thread.Name = "StartLiveView";
                thread.Start();
            }
            else
                Console.WriteLine("Tried to stop LiveView with a fake camera.");
        }

        private void StopLiveView(ICameraDevice cameraDevice)
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    _liveViewTimer.Stop();
                    // wait for last get live view image
                    Thread.Sleep(500);
                     cameraDevice.StopLiveView();
                }
                catch (DeviceException exception)
                {
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy || exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // this may cause infinite loop
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }
            } while (retry);
        }

        private void StartupThread()
        {
            foreach (var cameraDevice in DeviceManager.ConnectedDevices)
            {
                // multiple canon cameras block with this settings
                if (!(cameraDevice is CanonSDKBase))
                    cameraDevice.CaptureInSdRam = true;
                try
                {
                    cameraDevice.DateTime = DateTime.Now;
                }
                catch (Exception exception)
                {
                    Log.Error("Unable to sysnc date time", exception);
                }
            }
        }

        #endregion CameraCommands

        #region WindowsManagement

        private void ShowReviewWindow()
        {
            //List<string> imageArray;
            //CapturedImages.CopyTo(imageArray, 0);
            

            _reviewWindow = new ReviewWindow(CapturedImages, _currentTestResults.Comments);
            _reviewWindow.ReviewWindowClosingEvent += _reviewWindow_ReviewWindowClosingEvent;
            _reviewWindow.Show();

            // Put the focus back on the CameraControlWindow (to get back the keybinds actions)
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(CameraControlWindow))
                    {
                        (window as CameraControlWindow).Activate();
                    }
                }
            });

        }

        private void _reviewWindow_ReviewWindowClosingEvent(bool? isAccepted)
        {
            CameraSettings cameraSettings = new CameraSettings()
            {
                BurstNumber = _totalBurstNumber.ToString(),
                Flash = _currentTestCameraSettings.Flash,
                FNumber = DeviceManager.SelectedCameraDevice.FNumber.Value,
                ShutterSpeed = DeviceManager.SelectedCameraDevice.ShutterSpeed.Value,
                Iso = DeviceManager.SelectedCameraDevice.IsoNumber.Value
            };

            _currentTestResults.Comments = _reviewWindow.Comment;
            switch (isAccepted)
            {
                case true:
                    SaveTestResults(_reviewWindow.SelectedImages, CapturedImages, cameraSettings);
                    if (!NextInstructionGuideWindow() || _testIndex >= CurrentExam.TestList.Count)
                    {
                        CloseCurrentGuideWindow();
                        ++_testIndex;
                        if (_testIndex < CurrentExam.TestList.Count)
                            FetchCurrentTest();
                    }
                    if (_testIndex >= CurrentExam.TestList.Count)
                    {
                        _testIndex = CurrentExam.TestList.Count;
                        break;
                    }
                    CaptureEnabled = true;
                    break;
                default:
                    CaptureEnabled = true;
                    SetDepthOfField(true);
                    
                    // deleting temp files
                    foreach (string temp in CapturedImages)
                    {
                        _lowPriorityTasks.Add(Task.Run(() =>
                        {
                            bool retry;
                            do
                            {
                                retry = false;
                                try
                                {
                                    Console.WriteLine("Deleting {0} + .jpeg + {1}...", Path.GetFileName(temp), _fileExtension);
                                    if (File.Exists(temp + _fileExtension))
                                        File.Delete(temp + _fileExtension);
                                    if (File.Exists(temp + ".jpeg"))
                                        File.Delete(temp + ".jpeg");
                                }
                                catch (IOException)
                                {
                                    Thread.Sleep(1000);
                                    retry = true;
                                }
                            } while (retry);
                        }));
                    }
                    ZoomOutEvent(true);
                    break;
            }
            CapturedImages.Clear();
            CapturedImages = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            _totalBurstNumber = 0; // reset the burstNumber after review

            //_reviewWindow.Close();
            _reviewWindow = null;

            if (_testIndex >= CurrentExam.TestList.Count)
            {
                ShowFinishWindow();
                return;
            }

            // Put the focus back on the CameraControlWindow (to get back the keybinds actions)
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(CameraControlWindow))
                    {
                        (window as CameraControlWindow).Activate();
                    }
                }
            });

            //if (DeviceManager.SelectedCameraDevice is NikonBase)
            //    StartLiveViewInThread();

            //_liveViewTimer.Start();
        }

        private void SetDepthOfField(bool isOn)
        {
            try
            {
                if (DeviceManager.SelectedCameraDevice is CanonSDKBase)
                    (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.DepthOfFieldPreview = isOn;
            }
            catch
            {
                Console.WriteLine("Could not set the DepthOfFieldPreview.");
            }
            
        }

        private void ShowFinishWindow()
        {
            CameraControlWindow currentWindow;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(CameraControlWindow))
                    {
                        currentWindow = window as CameraControlWindow;
                        currentWindow.Hide();
                    }
                }
            });
            if (_objFinishWindow == null)
            {
                _objFinishWindow = new FinishWindow(CurrentExam);
                _objFinishWindow.NewPhotoEvent += ObjFinishWindow_NewPhotoEvent;
                _objFinishWindow.FinishWindowClosingEvent += ObjFinishWindow_FinishWindowClosingEvent;
            }

            _objFinishWindow.Show();
        }

        private void ObjFinishWindow_FinishWindowClosingEvent(CancelEventArgs e)
        {
            foreach (TestResults testResult in _currentExam.Results)
            {
                if (testResult.ResultsImages != null)
                {
                    if (testResult.ResultsImages.Count > 0)
                    {
                        string path = Path.GetFullPath(testResult.ResultsImages[0]);
                        SaveTestResultsToJson(testResult, path);
                    }
                }
            }

            if(CloseApplication(e))
            {
                _objFinishWindow.FinishWindowClosingEvent -= ObjFinishWindow_FinishWindowClosingEvent;
            }
        }

        private void ObjFinishWindow_NewPhotoEvent(TestResults test)
        {
            _objFinishWindow.NewPhotoEvent -= ObjFinishWindow_NewPhotoEvent;
            _objFinishWindow.CloseWithoutEvent();
            _objFinishWindow = null;

            _currentTestResults = test;
            FetchTest(test.Id, new ObservableCollection<Tests>() { test.Id }, 0);
            CaptureEnabled = true;
            ResetZoom();
            StartLiveViewInThread();

            CameraControlWindow currentWindow;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(CameraControlWindow))
                    {
                        currentWindow = window as CameraControlWindow;
                        currentWindow.Show();
                    }
                }
            });

        }

        private void CloseApplication()
        {
            var result = System.Windows.MessageBox.Show("Do you want to close?", "", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                ClosingRoutine();
            }
        }

        private bool CloseApplication(CancelEventArgs e)
        {
            if(_objFinishWindow == null)
            {
                var result = System.Windows.MessageBox.Show("Do you want to close?", "", MessageBoxButton.YesNoCancel);
                e.Cancel = result != MessageBoxResult.Yes;
                if (!e.Cancel)
                {
                    ClosingRoutine();
                    return true;
                }
                else
                    return false;
            }
            else if (_objFinishWindow.IsVisible)
            {
                var result = System.Windows.MessageBox.Show("Do you want to close?", "", MessageBoxButton.YesNoCancel);
                e.Cancel = result != MessageBoxResult.Yes;
                if (!e.Cancel)
                {
                    ClosingRoutine();
                    return true;
                }
                else
                    return false;
            }
            _objFinishWindow = null;
            return false;
        }

        public void ClosingRoutine()
        {
            Console.WriteLine("Closing LightX...");
            if (_liveViewTimer.Enabled)
            {
                StopLiveView(DeviceManager.SelectedCameraDevice);
                SetDepthOfField(false);
                DeviceManager.CloseAll();
            }
            System.Windows.Application.Current.Shutdown();
        }

        private void CloseCurrentGuideWindow()
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(GuideWindow))
                    {
                        _oldGuideWindowPosition[0] = (window as GuideWindow).Left;
                        _oldGuideWindowPosition[1] = (window as GuideWindow).Top;
                        (window as GuideWindow).NextInstructionEvent -= ObjGuideWindow_NextInstructionEvent;
                        (window as GuideWindow).Close();
                    }
                }
            });
        }

        private bool NextInstructionGuideWindow()
        {
            bool nextOK = false;
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (Window window in System.Windows.Application.Current.Windows)
                {
                    if (window.GetType() == typeof(GuideWindow))
                    {
                        nextOK = (window as GuideWindow).NextInstruction();
                    }
                }
            });

            return nextOK;
        }

        private void RefreshDisplay()
        {
            RaisePropertyChanged(() => DeviceManager);
        }

        #endregion WindowsManagement

        #region DataAccess

        private void SaveTestResults(ObservableCollection<bool> selectedImages, List<string> imagesPath, CameraSettings cameraSettings)
        {
            // get Applied camera settings
            _currentTestResults.CamSettings = cameraSettings;

            //_currentTestResults.CamSettings = new CameraSettings()
            //{
            //    BurstNumber = _totalBurstNumber.ToString(),
            //    Flash = _currentTestCameraSettings.Flash,
            //    FNumber = DeviceManager.SelectedCameraDevice.FNumber.Value,
            //    ShutterSpeed = DeviceManager.SelectedCameraDevice.ShutterSpeed.Value,
            //    Iso = DeviceManager.SelectedCameraDevice.IsoNumber.Value
            //};

            // create filenames, folder and save selected images
            if(_currentTestResults.ResultsImages == null)
                _currentTestResults.ResultsImages = new List<string>();
            string fileName = _currentTestResults.Id.ToString();
            //    CurrentExam.TestList[_testIndex].ToString();
            string folderName = string.Format("{0}\\{1}\\{2}_{3}_{4}_{5,2:D2}h{6,2:D2}\\{7}", 
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 
                "LightX", 
                CurrentExam.Patient.LastName, 
                CurrentExam.Patient.FirstName, 
                CurrentExam.ExamDate.ToLongDateString(), 
                CurrentExam.ExamDate.Hour, 
                CurrentExam.ExamDate.Minute,
                _currentTestResults.TestTitle);

            // check the folder of filename, if not found create it
            if (!Directory.Exists(folderName))
                Directory.CreateDirectory(folderName);

            int index = -1;
            for (int i = 0; i < selectedImages.Count; ++i)
                
                if (selectedImages[i])
                {
                    string path;
                    do
                    {
                        path = string.Format("{0}\\{1}_{2,2:D2}{3}", folderName, fileName, ++index, _fileExtension);  // add path to the folder (Nom_Prenom_timestamp)
                    } while (File.Exists(path));

                    string temp = imagesPath[i];
                    _lowPriorityTasks.Add(Task.Run(() =>
                    {
                        bool retry;
                        do
                        {
                            retry = false;
                            try
                            {
                                if (File.Exists(temp + _fileExtension))
                                {
                                    Console.WriteLine("Moving {0} to {1}...", Path.GetFileName(temp + _fileExtension), path);
                                    File.Move(temp + _fileExtension, path);
                                    File.Move(temp + ".jpeg", Path.ChangeExtension(path,".jpeg"));
                                    _currentTestResults.ResultsImages.Add(path);
                                }
                                //Console.WriteLine("Deleting {0} + .jpeg...", Path.GetFileName(temp));
                                //if (File.Exists(temp + ".jpeg"))
                                //    File.Delete(temp + ".jpeg");
                            }
                            catch (IOException)
                            {
                                Thread.Sleep(1000);
                                retry = true;
                            }
                            catch (Exception exception)
                            {
                                System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                            }
                        } while (retry);
                    }));
                }
                else
                {
                    string temp = imagesPath[i];
                    _lowPriorityTasks.Add(Task.Run(() =>
                    {
                        bool retry;
                        do
                        {
                            retry = false;
                            try
                            {
                                Console.WriteLine("Deleting {0} + .jpeg + {1}...", Path.GetFileName(temp), _fileExtension);
                                if (File.Exists(temp + _fileExtension))
                                    File.Delete(temp + _fileExtension);
                                if (File.Exists(temp + ".jpeg"))
                                    File.Delete(temp + ".jpeg");
                            }
                            catch (IOException)
                            {
                                Thread.Sleep(1000);
                                retry = true;
                            }
                        } while (retry);
                    }));
                }

            Task.WaitAll(_lowPriorityTasks.ToArray());


            // Save test informations to JSON file (comments, camera settings, ...)
            _lowPriorityTasks.Add(Task.Run(() =>
            {
                SaveTestResultsToJson(_currentTestResults, $"{folderName}\\{fileName}.json");
            }));
            
            if (CurrentExam.Results.Count < CurrentExam.TestList.Count)
                CurrentExam.Results.Add(_currentTestResults);
            else
            {
                int i = 0;
                foreach(Tests test in CurrentExam.TestList)
                {
                    if (test == _currentTestResults.Id)
                        break;
                    ++i;
                }
                CurrentExam.Results[i] = _currentTestResults;
            }
            imagesPath = null;
            //_currentTestResults = null;
            _lowPriorityTasks.Clear();
        }

        private void SaveTestResultsToJson(TestResults testResults, string path)
        {
            using (StreamWriter file = File.CreateText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, testResults);
            }
        }

        public void FetchCurrentTest()
        {
            
            Tests test = CurrentExam.TestList[_testIndex];

            string title = FetchTest(test, CurrentExam.TestList, _testIndex);

            if (_currentTestResults != null)
            {
                _currentTestResults = null;
                GC.Collect();
            }
            _currentTestResults = new TestResults() { TestTitle = title, Id = test };
        }

        public string FetchTest(Tests test, ObservableCollection<Tests> testList, int testIndex)
        {
            TestInstructions currentTest;
            string path = $@"..\..\Resources\{test}.json";
            if (File.Exists(path))
            {
                using (StreamReader file = File.OpenText(path))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    currentTest = (TestInstructions)serializer.Deserialize(file, typeof(TestInstructions));
                }

                GuideWindow objGuideWindow = new GuideWindow(currentTest, testList, testIndex);
                objGuideWindow.NextInstructionEvent += ObjGuideWindow_NextInstructionEvent;
                if (_oldGuideWindowPosition.Count == 0)
                {
                    // Initialize the new starting position
                    //_oldGuideWindowPosition.Add(Screen.AllScreens[0].WorkingArea.Left + objGuideWindow.Width / 4);
                    //_oldGuideWindowPosition.Add(Screen.AllScreens[0].WorkingArea.Height / 2 - objGuideWindow.Height / 2);
                    _oldGuideWindowPosition.Add(5);
                    _oldGuideWindowPosition.Add(15);
                }

                objGuideWindow.Left = _oldGuideWindowPosition[0];
                objGuideWindow.Top = _oldGuideWindowPosition[1];

                objGuideWindow.Show();

                // apply recomended camera settings to camera
                if (_currentTestCameraSettings == null)
                    _currentTestCameraSettings = new CameraSettings();

                _currentTestCameraSettings = currentTest.Instructions[0].CamSettings;
                //if (_testIndex != 0) // since DeviceManage is not declared yet in the first call of this function -> ok
                ResetZoom();
                ApplyCameraSettings(DeviceManager.SelectedCameraDevice, _currentTestCameraSettings);

                return currentTest.TestTitle;
            }
            else
            {
                return "New test";
            }
        }

        private void ResetZoom()
        {
            _subZoomDivider = 1;
            _lastSubZoomDivider = 1;
            _lastZoom = "All";
            if (DeviceManager.ConnectedDevices.Count > 0)
                SetZoom();
        }

        private void ObjGuideWindow_NextInstructionEvent()
        {
            //if (!NextInstructionGuideWindow())
                SkipCurrentTest();
        }

        private void SkipCurrentTest()
        {
            if (_currentTestResults.CamSettings == null)
                SaveTestResults(new ObservableCollection<bool>(), new List<string>(), new CameraSettings());

            _currentTestResults = null;

            if (DeviceManager.SelectedCameraDevice is NikonBase)
            {
                try
                {
                    if (_liveViewTimer.Enabled)
                        StopLiveView(DeviceManager.SelectedCameraDevice);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Can't stop LiveView, infinite loop : {0}", exception.Message);
                }
                Thread.Sleep(100); // wait for the liveView to stop
            }
            else if (DeviceManager.SelectedCameraDevice is CanonSDKBase)
            {
                _liveViewTimer.Stop();
                (DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.PauseLiveview();
            }
            CloseCurrentGuideWindow();
            ++_testIndex;
            if (_testIndex < CurrentExam.TestList.Count)
            {
                FetchCurrentTest();
                // Put the focus back on the CameraControlWindow (to get back the keybinds actions)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    foreach (Window window in System.Windows.Application.Current.Windows)
                    {
                        if (window.GetType() == typeof(CameraControlWindow))
                        {
                            (window as CameraControlWindow).Activate();
                        }
                    }
                });

                if (DeviceManager.SelectedCameraDevice is NikonBase)
                    StartLiveViewInThread();

                _liveViewTimer.Start();
            }
            else
            {
                _testIndex = CurrentExam.TestList.Count;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                ShowFinishWindow();
            }
        }

        #endregion DataAccess

        #region ImageManipulation

        private void ProcessImage(Stream stream)
        {
            if (CapturedImages == null)
                CapturedImages = new List<string>();

            byte[] rawData = new byte[stream.Length];
            stream.Read(rawData, 0, (int)stream.Length);

            GCHandle rawDataHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            IntPtr address = rawDataHandle.AddrOfPinnedObject();

            //System.Windows.Media.PixelFormat pf = PixelFormats.Rgb24;
            //int width = 6720;
            //int height = 4480;
            //int rawStride = (width * pf.BitsPerPixel + 7) / 8;

            Console.WriteLine("Extracting thumbnail to {0}", address);
            int size = LibrawClass.extractThumb(address, rawData.Length);
            Console.WriteLine("Thumbnail ({0}) extracted to {1}", size, address);

            byte[] result = new byte[size];
            rawDataHandle.Free();
            Marshal.Copy(address, result, 0, size);

            var fileName = Path.GetTempFileName();
            File.Delete(fileName);
            fileName = Path.ChangeExtension(fileName, null); // remove the .tmp extension
            _lowPriorityTasks.Add(new Task(() =>
            {
                File.WriteAllBytes(fileName + _fileExtension, (stream as MemoryStream).ToArray());
                stream.Close();
                stream.Dispose();
            }));
            File.WriteAllBytes(fileName + ".jpeg", result);

            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(fileName + ".jpeg");
            image.CacheOption = BitmapCacheOption.None;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.EndInit();
            image.Freeze();
            
            CapturedImages.Add(fileName);
            Console.WriteLine("{0}.jpeg added to the list of thumbnails", Path.GetFileName(fileName));
        }

        private void ProcessImageFromFile(string path)
        {
            if (CapturedImages == null)
                CapturedImages = new List<string>();

            byte[] rawData = new byte[5000000];
            //stream.Read(rawData, 0, (int)stream.Length);

            GCHandle rawDataHandle = GCHandle.Alloc(rawData, GCHandleType.Pinned);
            IntPtr address = rawDataHandle.AddrOfPinnedObject();

            //System.Windows.Media.PixelFormat pf = PixelFormats.Rgb24;
            //int width = 6720;
            //int height = 4480;
            //int rawStride = (width * pf.BitsPerPixel + 7) / 8;

            Console.WriteLine("Extracting thumbnail to {0}", address);
            int size = LibrawClass.extractThumbFromFile(address, path + _fileExtension);
            Console.WriteLine("Thumbnail ({0}) extracted to {1}", size, address);

            byte[] result = new byte[size];
            rawDataHandle.Free();
            Marshal.Copy(address, result, 0, size);

            File.WriteAllBytes(path + ".jpeg", result);

            //BitmapImage image = new BitmapImage();
            //image.BeginInit();
            //image.UriSource = new Uri(path + ".jpeg");
            //image.CacheOption = BitmapCacheOption.None;
            //image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            //image.EndInit();
            //image.Freeze();


            CapturedImages.Add(path);
            Console.WriteLine("{0}.jpeg added to the list of thumbnails", Path.GetFileName(path));
        }

        private static BitmapImage Bs2bi(BitmapSource bs)
        {
            TiffBitmapEncoder encoder = new TiffBitmapEncoder();
            //JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bs));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = memoryStream;
            bImg.CacheOption = BitmapCacheOption.OnLoad;
            bImg.EndInit();

            memoryStream.Close();
            memoryStream.Dispose();

            return bImg;
        }

        private double GetZoomRatio(int displayLenght, int fullLenght)
        {
            double ratio;
            switch(DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value)
            {
                case "25%":
                    ratio = 4;
                    break;
                case "33%":
                    ratio = 3;
                    break;
                case "50%":
                    ratio = 2;
                    break;
                case "66%":
                    ratio = 1.5;
                    break;
                case "100%":
                    ratio = 1;
                    break;
                default:
                    goto case "100%";
            }
            return ratio;
        }

        private WriteableBitmap DrawFocusPoint(LiveViewData liveViewData)
        {
            var bitmap = new WriteableBitmap(liveViewData.LiveViewImageWidth, liveViewData.LiveViewImageHeight, 96, 96, System.Windows.Media.PixelFormats.Bgra32, null);
            try
            {
                if (liveViewData == null)
                    return null;
                double xt = bitmap.Width / liveViewData.ImageWidth;
                double yt = bitmap.Height / liveViewData.ImageHeight;

                System.Windows.Media.Color col = Colors.LightGreen;
                col.A = 240;

                bitmap.DrawRectangle((int)(liveViewData.FocusX * xt - (liveViewData.FocusFrameXSize * xt / 2)),
                    (int)(liveViewData.FocusY * yt - (liveViewData.FocusFrameYSize * yt / 2)),
                    (int)(liveViewData.FocusX * xt + (liveViewData.FocusFrameXSize * xt / 2)),
                    (int)(liveViewData.FocusY * yt + (liveViewData.FocusFrameYSize * yt / 2)),
                    col);
                bitmap.Freeze();
                return bitmap;
            }
            catch (Exception exception)
            {
                Log.Error("Error draw helper lines", exception);
                return null;
            }
        }

        private Int32Rect GetSourceRect(LiveViewData liveViewData)
        {
            if (_subZoomDivider == 1)
            {
                Int32Rect ret = new Int32Rect();
                return ret;
            }
            else if(DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value == DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0])
            {
                double xt = (double)liveViewData.LiveViewImageWidth / (double)liveViewData.ImageWidth;
                double yt = (double)liveViewData.LiveViewImageHeight / (double)liveViewData.ImageHeight;
                return new Int32Rect(
                    Clamp((int)((liveViewData.FocusX * xt) - (liveViewData.LiveViewImageWidth / _subZoomDivider / 2)), 0, (int)(liveViewData.LiveViewImageWidth - (liveViewData.LiveViewImageWidth / _subZoomDivider))),
                    Clamp((int)((liveViewData.FocusY * yt) - (liveViewData.LiveViewImageHeight / _subZoomDivider / 2)), 0, (int)(liveViewData.LiveViewImageHeight - (liveViewData.LiveViewImageHeight / _subZoomDivider))),
                    (int)(liveViewData.LiveViewImageWidth / _subZoomDivider),
                    (int)(liveViewData.LiveViewImageHeight / _subZoomDivider));
            }
            else // pas idéal pour les extémité de l'image
            {
                return new Int32Rect(
                    Clamp((int)((liveViewData.LiveViewImageWidth / 2) - (liveViewData.LiveViewImageWidth / _subZoomDivider / 2)), 0, (int)(liveViewData.LiveViewImageWidth - (liveViewData.LiveViewImageWidth / _subZoomDivider))),
                    Clamp((int)((liveViewData.LiveViewImageHeight / 2) - (liveViewData.LiveViewImageHeight / _subZoomDivider / 2)), 0, (int)(liveViewData.LiveViewImageHeight - (liveViewData.LiveViewImageHeight / _subZoomDivider))),
                    (int)(liveViewData.LiveViewImageWidth / _subZoomDivider),
                    (int)(liveViewData.LiveViewImageHeight / _subZoomDivider));
            }
        }

        #endregion ImageManipulation


        public CameraControlWindowViewModel()
        {
            Thread.CurrentThread.Name = "MainThread";
            SetLiveViewTimer();
            SetBurstTimer();

            CurrentExam = new Exam();
            //FetchCurrentTest();

            //Task task = new Task(() => 
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    DeviceManager = new CameraDeviceManager();
                    DeviceManager.LoadWiaDevices = false;
                    DeviceManager.DetectWebcams = false;
                    DeviceManager.CameraSelected += DeviceManager_CameraSelected;
                    DeviceManager.CameraConnected += DeviceManager_CameraConnected;
                    DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
                    DeviceManager.CameraDisconnected += DeviceManager_CameraDisconnected;
                    // For experimental Canon driver support- to use canon driver the canon sdk files should be copied in application folder
                    DeviceManager.UseExperimentalDrivers = true;
                    DeviceManager.DisableNativeDrivers = false;
                    DeviceManager.ConnectToCamera();
                }
                catch (Exception exception)
                {
                    Log.Error("Unable to initialize device manager", exception);
                }
            });
            
            //task.Start();
            //Task.WaitAll(task);

            Thread thread = new Thread(StartupThread);
            thread.Name = "Startup";
            thread.Start();
        }
    }
}
