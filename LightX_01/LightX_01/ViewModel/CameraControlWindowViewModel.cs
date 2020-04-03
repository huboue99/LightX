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
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Canon.Eos.Framework.Eventing;
using Canon.Eos.Framework;

namespace LightX_01.ViewModel
{
    public class CameraControlWindowViewModel : ViewModelBase
    {
        #region Fields

        //[DllImport(@"G:\\LibRawTester\\x64\\Release\\LibRawTester.dll")]
        //public static extern int processRawImage(int a);

        // Main data holder & handler
        private CameraDeviceManager _deviceManager;
        private Exam _currentExam;
        private TestResults _currentTestResults;

        // Camera settings and results
        private CameraSettings _currentTestCameraSettings;
        private BitmapImage _currentLiveViewImage;
        private bool _liveViewEnabled;
        private string _lastZoom;
        private double _subZoomDivider = 1;
        private double _lastSubZoomDivider = 1;
        private volatile bool _zoomHasChanged = true;
        private ObservableCollection<int> _roiXY = null;
        private ObservableCollection<int> ROIXYMAX;
        private ObservableCollection<int> _currentRoiXYLimits;
        private WriteableBitmap _overlayBitmap;
        //private Int32Rect sourceRect = new Int32Rect();
        private volatile int _remainingBurst = 0;
        private int _totalBurstNumber = 0;
        public ObservableCollection<BitmapImage> CapturedImages { get; set; }

        // System and windows vars
        private List<double> _oldGuideWindowPosition = new List<double>(2);
        private System.Timers.Timer _liveViewTimer;
        private object _locker = new object();
        private int _testIndex = 0;

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
            /**/
            //sourceRect = GetSourceRect(0, 0, liveViewData);

            if (true)
            {
                if (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value == DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0] && _subZoomDivider == 1)
                    OverlayBitmap = DrawFocusPoint(liveViewData);
                else
                    OverlayBitmap = null;
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
                image.SourceRect = GetSourceRect(liveViewData); // Crop the image to get smaller zoom steps
                image.EndInit();
                image.Freeze();
                CurrentLiveViewImage = image;
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
            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
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
            }).Start();
            _zoomHasChanged = true;
        }

        public void MouseLeftButtonDown(double x, double y)
        {
            // don't want the click to focus action when already zoomed in?
            //x*(liveViewSize/imageSizeOnScreen)*(fullResolutionSize/liveViewSize) = x * fullResolutionSize / imageSizeOnScreen
            if (DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value == DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0] && _subZoomDivider == 1)
                SetRoiXY((int)(x * (double)ROIXYMAX[0]), (int)(y * (double)ROIXYMAX[1]));
        }

        public void ZoomOutEvent()
        {
            _lastZoom = DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value;
            _lastSubZoomDivider = _subZoomDivider;
            SetZoom(DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0]);
            _subZoomDivider = 1;
        }

        public void SetZoom(string desiredZoom = null)
        {
            if (desiredZoom == null)
            {
                desiredZoom = _lastZoom;
                _subZoomDivider = _lastSubZoomDivider;
            }
            if(DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Value != desiredZoom)
                DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.SetValue(desiredZoom);
            _zoomHasChanged = true;
        }

        void SelectedCamera_CameraInitDone(ICameraDevice cameraDevice)
        {
            ApplyCameraSettings(cameraDevice);
            if (LiveViewEnabled)
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    StartLiveViewInThread();
                });
            }
        }

        void DeviceManager_CameraSelected(ICameraDevice oldcameraDevice, ICameraDevice newcameraDevice)
        {
            if (oldcameraDevice != newcameraDevice)
            {
                LiveViewEnabled = newcameraDevice.GetCapability(CapabilityEnum.LiveView);
                if (_liveViewTimer.Enabled && (oldcameraDevice is NikonBase || oldcameraDevice is CanonSDKBase))
                {
                    // if oldcameraDevice still exist (not disconnected), need to remove handle of disconnection
                    //oldcameraDevice.CameraDisconnected -= SelectedCamera_CameraDisconnected;
                    //oldcameraDevice.CaptureCompleted -= SelectedCamera_CaptureCompleted;
                    oldcameraDevice.CameraInitDone -= SelectedCamera_CameraInitDone;
                    StopLiveViewInThread();
                }

                if (LiveViewEnabled && (newcameraDevice is CanonSDKBase))
                {
                    //newcameraDevice.CaptureCompleted += SelectedCamera_CaptureCompleted;
                    //newcameraDevice.CompressionSetting.Value = newcameraDevice.CompressionSetting.Values[4];
                    //newcameraDevice.CompressionSetting.SetValue(newcameraDevice.CompressionSetting.Values[4]);

                    // Canon EOS R is too quick and already initialized when we get here
                    SelectedCamera_CameraInitDone(newcameraDevice);
                }
                newcameraDevice.CameraInitDone += SelectedCamera_CameraInitDone;
            }
        }

        void DeviceManager_CameraConnected(ICameraDevice cameraDevice)
        {
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
            ++_remainingBurst; // if the burst is controlled via the camera's trigger button itself (counts the number of capture to process before showing the review window)
            ++_totalBurstNumber;
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
                    switch(propertyVal.Name)
                    {
                        case "Drive Mode":
                            propertyVal.SetValue(propertyVal.Values[1]); //High-Speed Continuous shooting
                            break;
                        default:
                            break;
                    }
                }

                (cameraDevice as CanonSDKBase).Camera.DepthOfFieldPreview = true;
                //cameraDevice.FocusMode.SetValue(cameraDevice.FocusMode.NumericValues[0]); // "AF-S"
                //cameraDevice.LiveViewFocusMode.SetValue(cameraDevice.LiveViewFocusMode.NumericValues[0]); // "AF-S"
                cameraDevice.CompressionSetting.SetValue(cameraDevice.CompressionSetting.Values[8]); // "JPEG (Smalest)" = 6 / RAW = 8 / RAW + JPEG = 7
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
                cameraDevice.FocusMode.SetValue(cameraDevice.FocusMode.Values[0]); // "AF-S"
                //cameraDevice.LiveViewFocusMode.SetValue(cameraDevice.LiveViewFocusMode.NumericValues[0]); // "AF-S"
                cameraDevice.CompressionSetting.SetValue(cameraDevice.CompressionSetting.Values[3]); // "JPEG (BASIC)" = 0 / RAW = 3 / RAW + JPEG = 4
            }
            //bool retry = true;
            //do
            //{
                //if (_currentTestCameraSettings != null)
                //{
                    cameraDevice.ShutterSpeed.SetValue(_currentTestCameraSettings.ShutterSpeed);
                    cameraDevice.FNumber.SetValue(_currentTestCameraSettings.FNumber);
                    SetZoom(DeviceManager.SelectedCameraDevice.LiveViewImageZoomRatio.Values[0]);
                    _roiXY = null; // will reset the focus point to the center of the image;
                                   //retry = false;
                                   //}
                                   //} while (retry);
            //cameraDevice.UnLockCamera();
        }

        private void CaptureInThread()
        {
            if (_liveViewEnabled)
                StopLiveViewInThread();
            //thread.ThreadState = ThreadState.
            //Thread.Sleep(100);
            //new Thread(Capture).Start();

            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.PauseLiveview();
            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).PressButton();

            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.ResetShutterButton();
            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).Camera.SendCommand(Canon.Eos.Framework.Internal.SDK.Edsdk.CameraCommand_TakePicture);

            //while (_totalBurstNumber < Int32.Parse(_currentTestCameraSettings.BurstNumber)) { };
            //(DeviceManager.SelectedCameraDevice as CanonSDKBase).ReleaseButton();

            try
            {
                (DeviceManager.SelectedCameraDevice as CanonSDKBase).CapturePhotoBurstNoAf(1);
            }
            catch (COMException comException)
            {
                DeviceManager.SelectedCameraDevice.IsBusy = false;
                ErrorCodes.GetException(comException);
            }
            catch
            {
                DeviceManager.SelectedCameraDevice.IsBusy = false;
                throw;
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
                    DeviceManager.SelectedCameraDevice.CapturePhotoNoAf();
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

        private void PhotoCaptured(object o)
        {
            if (CapturedImages == null)
                CapturedImages = new ObservableCollection<BitmapImage>();
            PhotoCapturedEventArgs eventArgs = o as PhotoCapturedEventArgs;
            //PhotoCapturedEventArgs eventAr = new PhotoCapturedEventArgs
            //{
            //    WiaImageItem = null,
            //    CameraDevice = this,
            //    FileName = "DSC_0000.CR3",
            //    Handle = (uint)BitConverter.ToUInt32(eventArgs.Handle as , 0)
            //}
            //EosMemoryImageEventArgs file = o as EosMemoryImageEventArgs;
            if (eventArgs == null)
                return;
            try
            {
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

                //GC.TryStartNoGCRegion(244);
                while (eventArgs.CameraDevice.TransferProgress != 100u && eventArgs.CameraDevice.TransferProgress != 0u) { }
                eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);
                //eventArgs.CameraDevice.TransferFile(eventArgs.Handle, memStream);
                //eventArgs.CameraDevice.TransferFile(eventArgs.Handle, eventArgs.FileName);
                //EosMemoryImageEventArgs memory = eventArgs.Handle as EosMemoryImageEventArgs;
                //EosImageEventArgs arg = o as EosImageEventArgs;
                //memStream.Write(memory.ImageData, 0, memory.ImageData.Length);

                //GC.EndNoGCRegion();

                memStream.Position = 0;

                // Gestion temporaire des fichier RAW
                string fileName = Path.GetFileName(eventArgs.FileName);
                if(Path.GetExtension(fileName) == ".NEF" || Path.GetExtension(fileName) == ".CR2" || Path.GetExtension(fileName) == ".CR3")
                {
                    using (MagickImage magickImage = new MagickImage(memStream, MagickFormat.Cr3))
                    {
                        memStream.Close();
                        GC.Collect();
                        //IPixelCollection pc = magickImage.GetPixels();
                        //byte[] data = pc.ToByteArray("RGB");
                        //BitmapSource bmf = FromArray(data, magickImage.Width, magickImage.Height, magickImage.ChannelCount);


                        System.Drawing.Bitmap bmp = magickImage.ToBitmap(System.Drawing.Imaging.ImageFormat.Tiff);

                        //System.Drawing.Bitmap uwu = new System.Drawing.Bitmap(bmp.Width, bmp.Height, System.Drawing.Imaging.PixelFormat.Format48bppRgb);

                        //using (Graphics graph = Graphics.FromImage(uwu))
                        //{
                        //    graph.DrawImage(bmp, new System.Drawing.Point(0, 0));
                        //}

                        //bmp.PixelFormat = System.Drawing.Imaging.PixelFormat.Format48bppRgb;

                        //Bitmap ayy;

                        //System.Drawing.Imaging.PixelFormat pixelFormat = System.Drawing.Imaging.PixelFormat.Format48bppRgb;

                        //Rectangle rect = new Rectangle(0, 0, bmp.Width, bmp.Height);

                        //BitmapData bitmapData = bmp.LockBits(rect, ImageLockMode.ReadOnly, pixelFormat);
                        //try
                        //{
                        //    Bitmap convertedBitmap = new Bitmap(bmp.Width, bmp.Height, pixelFormat);
                        //    BitmapData convertedBitmapData = convertedBitmap.LockBits(rect, ImageLockMode.WriteOnly, pixelFormat);
                        //    try
                        //    {
                        //        NativeMethods.CopyMemory(convertedBitmapData.Scan0, bitmapData.Scan0, (uint)bitmapData.Stride * (uint)bitmapData.Height);
                        //    }
                        //    finally
                        //    {
                        //        convertedBitmap.UnlockBits(convertedBitmapData);
                        //    }

                        //    ayy = convertedBitmap;
                        //}
                        //finally
                        //{
                        //    bmp.UnlockBits(bitmapData);
                        //}

                        magickImage.Dispose();
                        GC.Collect();

                        using (var mem = new MemoryStream())
                        {
                            bmp.Save(mem, System.Drawing.Imaging.ImageFormat.Tiff);
                            bmp.Dispose();
                            mem.Position = 0;

                            BitmapImage image = new BitmapImage();
                            image.BeginInit();
                            image.StreamSource = mem;
                            image.CacheOption = BitmapCacheOption.OnLoad;
                            image.EndInit();
                            image.Freeze();

                            CapturedImages.Add(image);
                            mem.Close();
                        }

                        //    bmp.Save()
                        //IPixelCollection pc = magickImage.GetPixels();

                        //byte[] data = pc.ToByteArray(PixelMapping.BGR);

                        //ushort[] data = pc.ToArray();
                        //BitmapSource bmf = FromArray(data, magickImage.Width, magickImage.Height, magickImage.ChannelCount);

                        //BitmapSource bmf = magickImage.ToBitmapSource(BitmapDensity.)

                        // conversion en bitmap image

                        //BitmapImage image = Bs2bi(bmf);


                        //image.Freeze();
                        //CapturedImages.Add(image);
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

                --_remainingBurst;

                eventArgs.CameraDevice.IsBusy = false;
                eventArgs.CameraDevice.ReleaseResurce(eventArgs.Handle);
            }
            catch (Exception exception)
            {
                eventArgs.CameraDevice.IsBusy = false;
                System.Windows.MessageBox.Show("Error download photo from camera :\n" + exception.Message);
            }
            GC.Collect();

            if (_remainingBurst == 0) // (wait for all captureEvents to be process before showing the review window)
                System.Windows.Application.Current.Dispatcher.Invoke(() =>
                {
                    ShowReviewWindow();
                });
        }

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

        public void MoveRoiXY(System.Windows.Input.KeyEventArgs e)
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
                    switch (e.Key)
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
                    //while (DeviceManager.SelectedCameraDevice.IsBusy) { }
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
            //DeviceManager.SelectedCameraDevice.IsBusy = false;
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
                    break;
                default:
                    ZoomOutEvent();
                    break;
            }
            CapturedImages.Clear();
            CapturedImages = null;
            GC.Collect();
            _totalBurstNumber = 0; // reset the burstNumber after review
                                   
            objReviewWindow.Close();


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
                BurstNumber = _totalBurstNumber.ToString(),
                Flash = _currentTestCameraSettings.Flash,
                FNumber = DeviceManager.SelectedCameraDevice.FNumber.Value,
                ShutterSpeed = DeviceManager.SelectedCameraDevice.ShutterSpeed.Value,
                Iso = DeviceManager.SelectedCameraDevice.IsoNumber.Value
            };

            // create filenames, folder and save selected images
            _currentTestResults.ResultsImages = new List<string>();
            string fileName01 = CurrentExam.TestList[_testIndex];
            string folderName = string.Format("{0}\\{1}\\{2}_{3}_{4}_{5}h{6}", 
                Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), 
                "LightX", 
                CurrentExam.Patient.LastName, 
                CurrentExam.Patient.FirstName, 
                CurrentExam.ExamDate.ToLongDateString(), 
                CurrentExam.ExamDate.Hour.ToString(), 
                CurrentExam.ExamDate.Minute.ToString());

            for (int i = 0; i < selectedImages.Count; ++i)
                if(selectedImages[i])
                {
                    
                    string path = string.Format("{0}\\{1}_{2,2:D2}.tiff", folderName, fileName01, i);  // add path to the folder (Nom_Prenom_timestamp)
                    // check the folder of filename, if not found create it
                    if (!Directory.Exists(Path.GetDirectoryName(path)))
                        Directory.CreateDirectory(Path.GetDirectoryName(path));
                    _currentTestResults.ResultsImages.Add(path);
                    //SaveToTiff(CapturedImages[i], path);
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
            if (_testIndex != 0) // since DeviceManage is not declared yet in the first call of this function -> ok
                ApplyCameraSettings(DeviceManager.SelectedCameraDevice);

            if (_currentTestResults != null)
            {
                _currentTestResults = null;
                GC.Collect();
            }
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

        public CameraControlWindowViewModel(Exam exam)
        {
            SetLiveViewTimer();

            CurrentExam = exam;
            FetchCurrentTest();

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

            //int i = LibrawClass.processRawImage(15);
            
            Thread thread = new Thread(StartupThread);
            thread.Start();
            //StartupThread(); // initial setup of camera
        }
    }
}
