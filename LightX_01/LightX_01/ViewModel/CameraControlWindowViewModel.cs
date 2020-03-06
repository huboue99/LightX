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
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using CameraControl.Devices.Canon;
using System.Timers;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Threading;
using CameraControl.Devices.Nikon;
using System.Windows.Interop;

namespace LightX_01.ViewModel
{

    public class CameraControlWindowViewModel : ViewModelBase
    {
        #region Fields

        private CameraDeviceManager _deviceManager;
        private Exam _currentExam;
        private CameraSettings _currentTestCameraSettings;
        private TestResults _currentTestResults;
        private int _testIndex = 0;
        private List<double> _oldGuideWindowPosition = new List<double>(2);
        private BitmapImage _currentLiveViewImage;
        private bool _liveViewEnabled;
        private System.Timers.Timer _liveViewTimer;
        private string _folderForPhotos;
        private object _locker = new object();
        private RelayCommand _captureCommand;
        private ICommand _imageMouseWheelCommand;
        public BitmapImage LastPhoto { get; set; }
        public ObservableCollection<BitmapImage> CapturedImages { get; set; }
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
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }

            } while (retry);
            _liveViewTimer.Start();
            //DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.SetValue(3);
        }


        private void ImageMouseWheel(MouseWheelEventArgs e)
        {
            e.Handled = true;
            if (e.Delta > 0)
            {
                DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.NextValue();
            }
            else if (e.Delta < 0)
            {
                DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.PrevValue();
            }
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
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
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

        public ICommand ImageMouseWheelCommand
        {
            get
            {
                if (_imageMouseWheelCommand == null)
                {
                    _imageMouseWheelCommand = new RelayCommand<MouseWheelEventArgs>(
                        param => ImageMouseWheel(param)
                        );
                }
                return _imageMouseWheelCommand;
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

        private void ApplyCameraSettings(ICameraDevice cameraDevice)
        {
            if (cameraDevice is CanonSDKBase)
            {
                foreach (var propertyVal in cameraDevice.AdvancedProperties)
                {
                    if (propertyVal.Name == "Drive Mode")
                    {
                        //propertyVal.NumericValue = propertyVal.NumericValues[1]; //Continuous shooting
                        //propertyVal.SetValue(propertyVal.NumericValues[1]);//Continuous shooting
                        propertyVal.SetValue(propertyVal.NumericValues[4]); //High-Speed Continuous shooting
                        //propertyVal.NumericValue = propertyVal.NumericValues[12]; //14fps super high shooting
                        //propertyVal.NumericValue = propertyVal.NumericValues[14]; //Silent Continuous shooting
                        //propertyVal.NumericValue = propertyVal.NumericValues[15]; //Silent high-speed Continuous shooting
                    }
                }
            }
            else if (cameraDevice is NikonBase)
            {
                foreach (var propertyVal in cameraDevice.AdvancedProperties)
                {
                    switch (propertyVal.Name)
                    {
                        case "Burst Number":
                            propertyVal.SetValue(_currentTestCameraSettings.BurstNumber); // Burst de n photos = [n - 1]
                            break;
                        case "Still Capture Mode":
                            propertyVal.SetValue(propertyVal.NumericValues[1]); // "Continuous high-speed shooting (CH)"
                            break;
                        case "Auto Iso":
                            propertyVal.SetValue(propertyVal.NumericValues[1]); // Auto Iso OFF = 0 // ON = 1
                            break;
                        case "Flash":
                            propertyVal.SetValue(propertyVal.NumericValues[0]); // Flash prohibited = 0 / Normal synch = 2 / slow synch = 3
                            break;
                        case "Color space":
                            propertyVal.SetValue(propertyVal.NumericValues[1]); // Adobe RGB
                            break;
                        case "Flash sync speeeeeeeed":
                            propertyVal.SetValue(propertyVal.NumericValues[0]); // 1/320
                            break;
                        case "Active D-Lighting":
                            propertyVal.SetValue(propertyVal.NumericValues[4]); // Not Performed
                            break;

                    }
                }
                cameraDevice.FocusMode.SetValue(cameraDevice.FocusMode.NumericValues[0]); // "AF-S"
                //cameraDevice.LiveViewFocusMode.SetValue(cameraDevice.LiveViewFocusMode.NumericValues[0]); // "AF-S"
                cameraDevice.CompressionSetting.SetValue(cameraDevice.CompressionSetting.NumericValues[0]); // "JPEG (BASIC)" = 0 / RAW = 3 / RAW + JPEG = 4

                cameraDevice.ShutterSpeed.SetValue(_currentTestCameraSettings.ShutterSpeed);
                cameraDevice.FNumber.SetValue(_currentTestCameraSettings.FNumber);

            }
        }

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
                        System.Windows.MessageBox.Show("Error occurred :" + exception.Message);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show("Error occurred :" + ex.Message);
                }
            } while (retry);
        }

        private void RefreshDisplay()
        {
            RaisePropertyChanged(() => DeviceManager);
        }

        
        private void PhotoCaptured(object o)
        {
            if (CapturedImages == null)
                CapturedImages = new ObservableCollection<BitmapImage>();
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
            if (eventArgs == null)
                return;
            try
            {
                //eventArgs.CameraDevice.IsBusy = true;
                //string fileName = Path.Combine(FolderForPhotos, Path.GetFileName(eventArgs.FileName));
                //// if file exist try to generate a new filename to prevent file lost. 
                //// This useful when camera is set to record in ram the the all file names are same.
                //if (File.Exists(fileName))
                //    fileName =
                //      StaticHelper.GetUniqueFilename(
                //        Path.GetDirectoryName(fileName) + "\\" + Path.GetFileNameWithoutExtension(fileName) + "_", 0,
                //        Path.GetExtension(fileName));

                //// check the folder of filename, if not found create it
                //if (!Directory.Exists(Path.GetDirectoryName(fileName)))
                //{
                //    Directory.CreateDirectory(Path.GetDirectoryName(fileName));
                //}
                //eventArgs.CameraDevice.TransferFile(eventArgs.Handle, fileName);
                //// the IsBusy may used internally, if file transfer is done should set to false  
                //eventArgs.CameraDevice.IsBusy = false;
                //eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);

                //if (Path.GetExtension(fileName) == ".NEF" || Path.GetExtension(fileName) == ".CR2" || Path.GetExtension(fileName) == ".CR3")
                //{
                //    bool OnlyRaw = false; //to restart the liveView when only uploading raw files (gotta do the raw->tiff->bitmap function)
                //    if(OnlyRaw)
                //        System.Windows.Application.Current.Dispatcher.Invoke(() =>
                //        {
                //            StartLiveViewInThread();
                //        });
                //    return;
                //}

                //BitmapImage image = new BitmapImage();
                //image.BeginInit();
                //image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                //image.UriSource = new Uri(fileName, UriKind.Absolute);
                //image.CacheOption = BitmapCacheOption.OnLoad;
                //image.EndInit();
                //image.Freeze();
                //LastPhoto = image;

                eventArgs.CameraDevice.IsBusy = true;

                // Gestion temporaire des fichier RAW
                string fileName = Path.Combine(FolderForPhotos, Path.GetFileName(eventArgs.FileName));
                if(Path.GetExtension(fileName) == ".NEF" || Path.GetExtension(fileName) == ".CR2" || Path.GetExtension(fileName) == ".CR3")
                {
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
                    return;
                }


                Stream memStream = new MemoryStream();

               eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);

                memStream.Position = 0;
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.StreamSource = memStream;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();

                //LastPhoto = image;
                CapturedImages.Add(image);

                eventArgs.CameraDevice.IsBusy = false;
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
            }
            
            //GC.Collect();
            if(_currentTestCameraSettings.BurstNumber == CapturedImages.Count.ToString())
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowReviewWindow();
                    CapturedImages.Clear();
                });
        }


        private void ShowReviewWindow()
        {
            ReviewWindow objReviewWindow = new ReviewWindow(CapturedImages[0], _currentTestResults.Comments);
            bool? isAccepted = objReviewWindow.ShowDialog();
            _currentTestResults.Comments = objReviewWindow.Comment;
            switch (isAccepted)
            {
                case true:
                    CloseCurrentGuideWindow();
                    SaveTestResults();
                    _testIndex++;
                    FetchCurrentTest();
                    break;
                default:
                    break;
            }

            StartLiveViewInThread();
        }

        private void SaveTestResults()
        {
            // get Applied camera settings
            _currentTestResults.CamSettings = new CameraSettings()
            {
                FNumber = DeviceManager.SelectedCameraDevice.FNumber.Value,
                ShutterSpeed = DeviceManager.SelectedCameraDevice.ShutterSpeed.Value,
                Iso = DeviceManager.SelectedCameraDevice.IsoNumber.Value
            };

            // get paths of selected images

            // rename and copy/move selected images

            // delete remaining images

            // add _currentTestResults to _currentExam
            _currentExam.Results.Add(_currentTestResults);
        }

        void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            //StartupThread();
            if (oldcameraDevice != newcameraDevice)
            {
                LiveViewEnabled = newcameraDevice.GetCapability(CapabilityEnum.LiveView);
                if (_liveViewTimer.Enabled)
                {
                    // if oldcameraDevice still exist (not disconnected), need to remove handle of disconnection
                    //oldcameraDevice.CameraDisconnected -= SelectedCamera_CameraDisconnected;
                    //oldcameraDevice.CaptureCompleted -= SelectedCamera_CaptureCompleted;
                    oldcameraDevice.CameraInitDone -= SelectedCamera_CameraInitDone;
                    StopLiveViewInThread();
                }
                
                if (LiveViewEnabled)
                {
                    //newcameraDevice.CaptureCompleted += SelectedCamera_CaptureCompleted;
                    //newcameraDevice.CompressionSetting.Value = newcameraDevice.CompressionSetting.Values[4];
                    //newcameraDevice.CompressionSetting.SetValue(newcameraDevice.CompressionSetting.Values[4]);
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        StartLiveViewInThread();
                    });
                }
                newcameraDevice.CameraInitDone += SelectedCamera_CameraInitDone;
            }
        }

        void SelectedCamera_CameraInitDone(ICameraDevice cameraDevice)
        {
            ApplyCameraSettings(cameraDevice);
        }

        void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
        {
            //StartupThread();
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

        private void CloseApplication(CancelEventArgs e)
        {
            
            var result = System.Windows.MessageBox.Show("Do you want to close?", "", MessageBoxButton.YesNoCancel);
            e.Cancel = result != MessageBoxResult.Yes;
            if (!e.Cancel)
            {
                if (_liveViewTimer.Enabled)
                {
                    StopLiveViewInThread();
                }
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void FetchCurrentTest()
        {
            GuideData currentTest;
            //int i = _testIndex;
            string path = $@"..\..\Resources\{CurrentExam.TestList[_testIndex]}.json";
            using (StreamReader file = File.OpenText(path))
            {
                JsonSerializer serializer = new JsonSerializer();
                currentTest = (GuideData)serializer.Deserialize(file, typeof(GuideData));
            }

            GuideWindow objGuideWindow = new GuideWindow(currentTest, CurrentExam.TestList, _testIndex);
            if (_oldGuideWindowPosition.Count == 0)
            {
                // Initialize the new starting position
                _oldGuideWindowPosition.Add(Screen.AllScreens[0].WorkingArea.Left + objGuideWindow.Width / 4);
                _oldGuideWindowPosition.Add(Screen.AllScreens[0].WorkingArea.Height / 2 - objGuideWindow.Height / 2);
            }

            objGuideWindow.Left = _oldGuideWindowPosition[0];
            objGuideWindow.Top = _oldGuideWindowPosition[1];

            objGuideWindow.Show();

            // apply recomended camera settings to camera
            if (_currentTestCameraSettings == null)
                _currentTestCameraSettings = new CameraSettings();

            _currentTestCameraSettings = currentTest.CamSettings;
            if (_testIndex != 0)
                ApplyCameraSettings(DeviceManager.SelectedCameraDevice);

            if (_currentTestResults != null)
                _currentTestResults = null;
            _currentTestResults = new TestResults() { TestTitle = currentTest.TestTitle };
            
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
                        (window as GuideWindow).Close();
                    }
                }
            });
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

        public CameraControlWindowViewModel(Exam exam)
        {
            SetLiveViewTimer();


            
            lock (_locker) // Prevent program to start new transfer while another transfer is active.
            {

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
            }
            FolderForPhotos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Test");

            //while (DeviceManager.SelectedCameraDevice.AdvancedProperties.Count == 0) { }

            Thread sthread = new Thread(StartupThread);
            
            
            //StartupThread();
            //RefreshDisplay();


            CurrentExam = exam;
            //while(DeviceManager.SelecetedCameraDevice.IsBusy) { }

            string whew = DeviceManager.SelectedCameraDevice.GetProhibitionCondition(OperationEnum.LiveView);
            FetchCurrentTest();
        }
    }
}
