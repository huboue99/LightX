using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX_01.Classes;
using Newtonsoft.Json;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Media.Imaging;
using System;
using System.Drawing;
using System.Windows.Controls;
using CameraControl.Devices;
using CameraControl.Devices.Classes;
using System.Threading;
using System.Collections.ObjectModel;
using CameraControl.Devices.Canon;
using System.Timers;
using System.Windows.Input;
using System.ComponentModel;

namespace LightX_01.ViewModel
{

    public class CameraControlWindowViewModel : ViewModelBase
    {
        #region Fields

        private CameraDeviceManager _deviceManager;
        private BitmapImage _currentLiveViewImage;
        private bool _liveViewEnabled;
        private System.Timers.Timer _liveViewTimer;
        private string _folderForPhotos;
        private object _locker = new object();
        private RelayCommand _captureCommand;
        public BitmapImage LastPhoto { get; set; }
        private ICommand _closingCommand;

        public ICameraDevice CameraDevice { get; set; }

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

        public string FolderForPhotos
        {
            get { return _folderForPhotos; }
            set { _folderForPhotos = value; }
        }

        #endregion Properties

        #region Commands

        public bool LiveViewEnabled
        {
            get { return _liveViewEnabled; }
            set
            {
                if (value != _liveViewEnabled)
                {
                    _liveViewEnabled = value;
                    RaisePropertyChanged(() => LiveViewEnabled);
                }
            }
        }

        void _liveViewTimer_Tick(object sender, EventArgs e)
        {
            LiveViewData liveViewData = null;
            try
            {
                liveViewData = DeviceManager.SelectedCameraDevice.GetLiveViewImage();
            }
            catch (Exception)
            {
                return;
            }

            if (liveViewData == null || liveViewData.ImageData == null)
            {
                return;
            }
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                //image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                //image.UriSource = new Uri(CurrentTest.ImagesPath[0], UriKind.Relative);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = new MemoryStream(liveViewData.ImageData,
                                                                liveViewData.ImageDataPosition,
                                                                liveViewData.ImageData.Length -
                                                                liveViewData.ImageDataPosition);
                image.EndInit();
                image.Freeze();
                CurrentLiveViewImage = image;
            }
            catch (Exception)
            {

            }
        }

        private void StartLiveView()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    while (DeviceManager.SelectedCameraDevice.IsBusy) { }
                    DeviceManager.SelectedCameraDevice.StartLiveView();
                    
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
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);
            _liveViewTimer.Start();
            //CurrentFNumber = DeviceManager.SelectedCameraDevice.FNumber;
        }


        private void StopLiveView()
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
                    //while (DeviceManager.SelectedCameraDevice.IsBusy) { }
                    DeviceManager.SelectedCameraDevice.StopLiveView();
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
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);
        }

        private void SetLiveViewTimer()
        {
            _liveViewTimer = new System.Timers.Timer(1000 / 60);
            _liveViewTimer.Elapsed += _liveViewTimer_Tick;
            _liveViewTimer.AutoReset = true;
        }

        public RelayCommand CaptureCommand
        {
            get
            {
                if (_captureCommand == null)
                {
                    _captureCommand = new RelayCommand(CaptureInThread, true);
                }
                return _captureCommand;
            }
        }

        public ICommand ClosingCommand
        {
            get
            {
                if (_closingCommand == null)
                {
                    _closingCommand = new RelayCommand<CancelEventArgs>(
                        param => CloseApplication(param)
                        );
                }
                return _closingCommand;
            }
        }

        private void CaptureInThread()
        {
            if (_liveViewEnabled)
                StopLiveViewInThread();
            //thread.ThreadState = ThreadState.
            Thread.Sleep(100);
            new Thread(Capture).Start();
        }

        private void StartLiveViewInThread()
        {
            new Thread(StartLiveView).Start();
        }

        private void StopLiveViewInThread()
        {
            Thread thread = new Thread(StopLiveView);
            thread.Start();
            thread.Join();
        }

        #endregion Commands

        private void Capture()
        {
            bool retry;
            do
            {
                retry = false;
                try
                {
                    DeviceManager.SelectedCameraDevice.CapturePhoto();
                }
                catch (DeviceException exception)
                {
                    // if device is bussy retry after 100 miliseconds
                    if (exception.ErrorCode == ErrorCodes.MTP_Device_Busy ||
                        exception.ErrorCode == ErrorCodes.ERROR_BUSY)
                    {
                        // !!!!this may cause infinite loop
                        //Thread.Yield();
                        Thread.Sleep(100);
                        retry = true;
                    }
                    else
                    {
                        MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error occurred :" + ex.Message);
                }

            } while (retry);
        }

        private void RefreshDisplay()
        {
            RaisePropertyChanged(() => DeviceManager);
        }

        [STAThread]
        private void PhotoCaptured(object o)
        {
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
            if (eventArgs == null)
                return;
            try
            {
                eventArgs.CameraDevice.IsBusy = true;
                string fileName = Path.Combine(FolderForPhotos, Path.GetFileName(eventArgs.FileName));
                // if file exist try to generate a new filename to prevent file lost. 
                // This useful when camera is set to record in ram the the all file names are same.
                if (File.Exists(fileName))
                    fileName =
                      StaticHelper.GetUniqueFilename(
                        Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                        Path.GetExtension(fileName));

                // check the folder of filename, if not found create it
                if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                }
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, fileName);
                // the IsBusy may used internally, if file transfer is done should set to false  
                eventArgs.CameraDevice.IsBusy = false;
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);



                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                image.UriSource = new Uri(fileName, UriKind.Absolute);
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                LastPhoto = image;

            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                MessageBox.Show("Error download photo from camera :\n" + exception.Message);
            }
            //GC.Collect();

            // Restart the live view after capture is finished
            //StartLiveViewInThread();


            //StartLiveViewInThread();
            //Thread thread = new Thread(ShowReviewWindow);
            //thread.SetApartmentState(ApartmentState.STA);
            //thread.Start();
            ////ShowReviewWindow();
            ////thread.Join();
            //StartLiveViewInThread();
        }

        private void ShowReviewWindow()
        {
            ReviewWindow objReviewWindow = new ReviewWindow(LastPhoto);
            objReviewWindow.ShowDialog();
        }

        void SelectedCamera_CaptureCompleted(object sender, EventArgs eventArgs)
        {
            Thread thread = new Thread(ShowReviewWindow);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            StartLiveViewInThread();
        }

        void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            if (oldcameraDevice != newcameraDevice)
            {
                LiveViewEnabled = newcameraDevice.GetCapability(CapabilityEnum.LiveView);
                if (_liveViewTimer.Enabled)
                {
                    // if oldcameraDevice still exist (not disconnected), need to remove handle of disconnection
                    //oldcameraDevice.CameraDisconnected -= SelectedCamera_CameraDisconnected;
                    oldcameraDevice.CaptureCompleted -= SelectedCamera_CaptureCompleted;
                    _liveViewTimer.Stop();
                }
                //if (oldcameraDevice.GetStatus(OperationEnum.LiveView))
                //{
                //    oldcameraDevice.StopLiveView();
                //}

                
                if (LiveViewEnabled)
                {
                    StartLiveViewInThread();
                    //newcameraDevice.CameraDisconnected += SelectedCamera_CameraDisconnected;
                    newcameraDevice.CaptureCompleted += SelectedCamera_CaptureCompleted;
                    //CurrentFNumber.ValueChanged += SelectedCamera_FNumberChanged;


                }
            }
        }

        void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
        {
            RefreshDisplay();
        }

        void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            lock (_locker) // Prevent program to start new transfer while another transfer is active.
            {
                // to prevent UI freeze start the transfer process in a new thread
                //Thread thread = new Thread(PhotoCaptured);
                //thread.Start(eventArgs);
                PhotoCaptured(eventArgs);
            }
        }

        void DeviceManager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            RefreshDisplay();
        }

        void SelectedCamera_CameraDisconnected(object sender, DisconnectCameraEventArgs eventArgs)
        {
            _liveViewTimer.Stop();
            Thread.Sleep(100);
        }

        //void SelectedCamera_FNumberChanged(object sender, string key, long val)
        //{
        //    if (LiveViewEnabled)
        //    {
        //        StopLiveView();
        //        StartLiveViewInThread();
        //    }
        //}

        private void CloseApplication(CancelEventArgs e)
        {
            
            var result = MessageBox.Show("Do you want to close?", "", MessageBoxButton.YesNoCancel);
            e.Cancel = result != MessageBoxResult.Yes;
            if (!e.Cancel)
            {
                if (_liveViewTimer.Enabled)
                {
                    StopLiveViewInThread();
                }
                Application.Current.Shutdown();
            }
        }

        private void ApplicationClosing()
        {

        }

        private void StartupThread()
        {
            foreach (var cameraDevice in DeviceManager.ConnectedDevices)
            {
                //var property = cameraDevice.
                //CameraPreset preset = ServiceProvider.Settings.GetPreset(property.DefaultPresetName);
                // multiple canon cameras block with this settings

                if (!(cameraDevice is CanonSDKBase))
                    cameraDevice.CaptureInSdRam = true;

                //Log.Debug("cameraDevice_CameraInitDone");
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

        public CameraControlWindowViewModel()
        {
            SetLiveViewTimer();
            DeviceManager = new CameraDeviceManager();
            DeviceManager.CameraSelected += DeviceManager_CameraSelected;
            DeviceManager.CameraConnected += DeviceManager_CameraConnected;
            DeviceManager.PhotoCaptured += DeviceManager_PhotoCaptured;
            DeviceManager.CameraDisconnected += DeviceManager_CameraDisconnected;
            // For experimental Canon driver support- to use canon driver the canon sdk files should be copied in application folder
            DeviceManager.UseExperimentalDrivers = true;
            DeviceManager.DisableNativeDrivers = false;
            FolderForPhotos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Test");
            //DeviceManager.AddFakeCamera();
            DeviceManager.ConnectToCamera();
            
            new Thread(StartupThread).Start();
            RefreshDisplay();


            Patient CurrentPatient = new Patient();

            CurrentPatient.FirstName = "John";
            CurrentPatient.LastName = "Smith";

            Exam exam = new Exam() { Patient = CurrentPatient };


            GuideWindow objGuideWindow = new GuideWindow(exam);
            objGuideWindow.Show();
        }
    }
}
