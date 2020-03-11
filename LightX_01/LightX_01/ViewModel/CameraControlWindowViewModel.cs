using CameraControl.Devices;
using CameraControl.Devices.Canon;
using CameraControl.Devices.Classes;
using CameraControl.Devices.Nikon;
using ImageMagick;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using LightX_01.Classes;
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

namespace LightX_01.ViewModel
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
        private bool _liveViewEnabled;
        private volatile int _burstIsFinished;
        public ObservableCollection<BitmapImage> CapturedImages { get; set; }

        // System and windows vars
        private List<double> _oldGuideWindowPosition = new List<double>(2);
        private System.Timers.Timer _liveViewTimer;
        private object _locker = new object();
        private int _testIndex = 0;
        private string _folderForPhotos;

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

        public string FolderForPhotos
        {
            get { return _folderForPhotos; }
            set { _folderForPhotos = value; }
        }

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
            { }
        }

        #endregion SystemCommands

        #region EventHandlers

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

        void SelectedCamera_CameraInitDone(ICameraDevice cameraDevice)
        {
            ApplyCameraSettings(cameraDevice);
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

        void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
        {
            //StartupThread();
            RefreshDisplay();
        }

        void DeviceManager_PhotoCaptured(object sender, PhotoCapturedEventArgs eventArgs)
        {
            //lock (_locker) // Prevent program to start new transfer while another transfer is active.
            //{
            //    // to prevent UI freeze start the transfer process in a new thread
            //    //Thread thread = new Thread(PhotoCaptured);
            //    //thread.Start(eventArgs);
            //    PhotoCaptured(eventArgs);
            //}
            ++_burstIsFinished; // if the burst is controlled via the camera's trigger button itself (counts the number of capture to process before showing the review window)
            Thread thread = new Thread(PhotoCaptured);
            thread.Start(eventArgs);
        }

        void DeviceManager_CameraDisconnected(ICameraDevice cameraDevice)
        {
            _liveViewTimer.Stop();
            RefreshDisplay();
        }

        #endregion EventHandlers

        #region CameraCommands

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
                cameraDevice.CompressionSetting.SetValue(cameraDevice.CompressionSetting.NumericValues[3]); // "JPEG (BASIC)" = 0 / RAW = 3 / RAW + JPEG = 4

                cameraDevice.ShutterSpeed.SetValue(_currentTestCameraSettings.ShutterSpeed);
                cameraDevice.FNumber.SetValue(_currentTestCameraSettings.FNumber);

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

        private void PhotoCaptured(object o)
        {
            //_burstIsFinished = true;
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

                eventArgs.CameraDevice.IsBusy = true;

                //BitmapImage image;

                Stream memStream = new MemoryStream();
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);

                memStream.Position = 0;

                

                // Gestion temporaire des fichier RAW
                string fileName = Path.Combine(FolderForPhotos, Path.GetFileName(eventArgs.FileName));
                if(Path.GetExtension(fileName) == ".NEF" || Path.GetExtension(fileName) == ".CR2" || Path.GetExtension(fileName) == ".CR3")
                {
                    using (MagickImage magickImage = new MagickImage(memStream, MagickFormat.Nef))
                    {
                        memStream.Close();
                        //IPixelCollection pc = magickImage.GetPixels();
                        //byte[] data = pc.ToByteArray("RGB");
                        //BitmapSource bmf = FromArray(data, magickImage.Width, magickImage.Height, magickImage.ChannelCount);


                        //IPixelCollection pc = magickImage.GetPixels();
                        //byte[] data = pc.ToByteArray(PixelMapping.BGR);
                        //BitmapSource bmf = FromArray(data, magickImage.Width, magickImage.Height, magickImage.ChannelCount);
                        BitmapSource bmf = magickImage.ToBitmapSource();

                        // conversion en bitmap image

                        BitmapImage image = Bs2bi(bmf);
                        image.Freeze();
                        CapturedImages.Add(image);
                    }
                }
                else // JPEG 
                {
                    //Stream memStream = new MemoryStream();

                    // eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);

                    //memStream.Position = 0;

                    BitmapImage image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = memStream;
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.EndInit();
                    image.Freeze();
                    memStream.Close();
                    CapturedImages.Add(image);
                }

                --_burstIsFinished;

                //LastPhoto = image;
                eventArgs.CameraDevice.IsBusy = false;
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
            }
            
            GC.Collect();
            //if (_currentTestCameraSettings.BurstNumber == CapturedImages.Count.ToString())
            if (_burstIsFinished == 0) // (wait for all captureEvents to be process before showing the review window)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowReviewWindow();
                    CapturedImages.Clear();
                    CapturedImages = null;
                });
            
        }

        private void StartLiveViewInThread()
        {
            new Thread(StartLiveView).Start();
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

        private void StopLiveViewInThread()
        {
            Thread thread = new Thread(StopLiveView);
            thread.Start();
            thread.Join();
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

        #endregion CameraCommands

        #region WindowsManagement

        private void ShowReviewWindow()
        {
            ReviewWindow objReviewWindow = new ReviewWindow(CapturedImages, _currentTestResults.Comments);
            bool? isAccepted = objReviewWindow.ShowDialog();
            _currentTestResults.Comments = objReviewWindow.Comment;
            switch (isAccepted)
            {
                case true:
                    CloseCurrentGuideWindow();
                    SaveTestResults(objReviewWindow.SelectedImages);
                    _testIndex++;
                    FetchCurrentTest();
                    goto default;
                default:
                    objReviewWindow.Close();
                    break;
            }

            StartLiveViewInThread();
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

        private void RefreshDisplay()
        {
            RaisePropertyChanged(() => DeviceManager);
        }

        #endregion WindowsManagement

        #region DataAccess

        private void SaveTestResults(ObservableCollection<bool> selectedImages)
        {
            // get Applied camera settings
            _currentTestResults.CamSettings = new CameraSettings()
            {
                BurstNumber = _currentTestCameraSettings.BurstNumber,
                Flash = _currentTestCameraSettings.Flash,
                FNumber = DeviceManager.SelectedCameraDevice.FNumber.Value,
                ShutterSpeed = DeviceManager.SelectedCameraDevice.ShutterSpeed.Value,
                Iso = DeviceManager.SelectedCameraDevice.IsoNumber.Value
            };

            // get paths of selected images

            // rename and copy/move selected images
            for(int i = 0; i < CapturedImages.Count; i++)
            {
                if (selectedImages[i]) { }
                    //SaveToTiff(CapturedImages[i]);
            }

            // delete remaining images


            // add _currentTestResults to _currentExam
            _currentExam.Results.Add(_currentTestResults);
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

        #endregion DataAccess

        #region ImageManipulation

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

            return bImg;
        }

        private static BitmapSource FromArray(byte[] data, int w, int h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Rgb48; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha


            WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);

            wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

            return wbm;
        }

        #endregion ImageManipulation

        public CameraControlWindowViewModel(Exam exam)
        {
            SetLiveViewTimer();
            
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

            FolderForPhotos = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Test");

            StartupThread(); // initial setup of camera
            CurrentExam = exam;
            FetchCurrentTest();
        }
    }
}
