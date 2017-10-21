using System;
using System.Threading.Tasks;
using ObjectTrackingDemo.Common;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using System.Drawing;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;
using AForge;
using System.Collections.Generic;
using Windows.Phone.Devices.Notification;

namespace ObjectTrackingDemo
{
    public sealed partial class GameTest : Page
    {
        VibrationDevice testVibrationDevice = VibrationDevice.GetDefault();
        int NbrOfBullets = 10;
        Boolean canShoot = true;
        double hurtOpa = 0;
        int min = 0;
        int sec = 0;
        private WriteableBitmap Wbitmap;
        int counter = 0;

        public static Bitmap miniImagePurple;
        public static Bitmap miniImageGreen;
        public static Bitmap miniImageRed;
        public static Bitmap miniImageBlue;

        private const int ColorPickFrameRequestId = 42;

        public Visibility ControlsVisibility
        {
            get
            {
                return (Visibility)GetValue(ControlsVisibilityProperty);
            }
            private set
            {
                SetValue(ControlsVisibilityProperty, value);
            }
        }
        public static readonly DependencyProperty ControlsVisibilityProperty =
            DependencyProperty.Register("ControlsVisibility", typeof(Visibility), typeof(CameraPage),
                new PropertyMetadata(Visibility.Visible));

        private VideoEngine _videoEngine;
        private ActionQueue _actionQueue;
        private Settings _settings;
        private NavigationHelper _navigationHelper;

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this._navigationHelper; }
        }

        public GameTest()
        {
            InitializeComponent();            
            timing();

            _navigationHelper = new NavigationHelper(this);

            _videoEngine = VideoEngine.Instance;
            
            _settings = App.Settings;

            NavigationCacheMode = NavigationCacheMode.Required;

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);


        //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => buttonShoot.Focus(FocusState.Pointer));
        buttonShoot.Focus(FocusState.Pointer);
            viseur.Visibility = Visibility.Visible;
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            _actionQueue = new ActionQueue();
            _actionQueue.ExecuteIntervalInMilliseconds = 500;

            await InitializeAndStartVideoEngineAsync();
            SetFlash(_settings.Flash, false);
            _videoEngine.ShowMessageRequest += OnVideoEngineShowMessageRequestAsync;
            _videoEngine.Messenger.FrameCaptured += OnFrameCapturedAsync;
            _videoEngine.Messenger.PostProcessComplete += OnPostProcessCompleteAsync;

            Window.Current.VisibilityChanged += OnVisibilityChangedAsync;

            DataContext = this;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _videoEngine.Torch = false;
            _actionQueue.Dispose();
            _settings.Save();
            _videoEngine.ShowMessageRequest -= OnVideoEngineShowMessageRequestAsync;
            _videoEngine.Messenger.FrameCaptured -= OnFrameCapturedAsync;
            _videoEngine.Messenger.PostProcessComplete -= OnPostProcessCompleteAsync;

            Window.Current.VisibilityChanged -= OnVisibilityChangedAsync;

            captureElement.Source = null;
            _videoEngine.DisposeAsync();

            base.OnNavigatedFrom(e);
        }

        /// <summary>
        /// Initializes and starts the video engine.
        /// </summary>
        /// <returns>True, if successful.</returns>
        private async Task<bool> InitializeAndStartVideoEngineAsync()
        {
            bool success = await _videoEngine.InitializeAsync();

            if (success)
            {
                captureElement.Source = _videoEngine.MediaCapture;

                _settings.SupportedIsoSpeedPresets = _videoEngine.SupportedIsoSpeedPresets;
                await _videoEngine.SetIsoSpeedAsync(_settings.IsoSpeedPreset);
                await _videoEngine.SetExposureAsync(_settings.Exposure);

                success = await _videoEngine.StartAsync();
            }
            return success;
        }

        /// <summary>
        /// Sets the flash on/off.
        /// </summary>
        /// <param name="enabled">If true, will try to set the flash on.</param>
        /// <param name="saveSettings">If true, will save the color to the local storage.</param>
        private void SetFlash(bool enabled, bool saveSettings = true)
        {
            _videoEngine.Flash = _videoEngine.Torch = enabled;
            _settings.Flash = _settings.Torch = _videoEngine.Torch;            
            if (saveSettings)
            {
                _settings.Save();
            }
        }

        #region Event handlers

        private async void OnVisibilityChangedAsync(object sender, VisibilityChangedEventArgs e)
        {
            if (e.Visible == true)
            {
                _actionQueue = new ActionQueue();
                _actionQueue.ExecuteIntervalInMilliseconds = 500;

                await InitializeAndStartVideoEngineAsync();
                _videoEngine.Messenger.FrameCaptured += OnFrameCapturedAsync;
                _videoEngine.Messenger.PostProcessComplete += OnPostProcessCompleteAsync;
            }
            else
            {
                _actionQueue.Dispose();
                _actionQueue = null;

                _settings.Save();
                _videoEngine.Messenger.FrameCaptured -= OnFrameCapturedAsync;
                _videoEngine.Messenger.PostProcessComplete -= OnPostProcessCompleteAsync;

                captureElement.Source = null;
                _videoEngine.DisposeAsync();
            }
        }

        private async void OnFrameCapturedAsync(byte[] pixelArray, int frameWidth, int frameHeight, int frameId)
        {
            _videoEngine.Messenger.FrameCaptured -= OnFrameCapturedAsync;
            System.Diagnostics.Debug.WriteLine("OnFrameCapturedAsync");

            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                Wbitmap = await ImageProcessingUtils.PixelArrayToWriteableBitmapAsync(pixelArray, frameWidth, frameHeight);

                if (Wbitmap != null)
                {
                    CapturePhoto(Wbitmap);
                }

                _videoEngine.Messenger.FrameCaptured += OnFrameCapturedAsync;
            });
        }

        private async void OnPostProcessCompleteAsync(
            byte[] pixelArray, int imageWidth, int imageHeight,
            ObjectDetails fromObjectDetails, ObjectDetails toObjectDetails)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
            {
                WriteableBitmap bitmap = await ImageProcessingUtils.PixelArrayToWriteableBitmapAsync(pixelArray, imageWidth, imageHeight);

                if (bitmap != null)
                {
                    
                }
            });

        }

        private async void OnVideoEngineShowMessageRequestAsync(object sender, string message)
        {
            await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
            });
        }

        #endregion
        public Task CapturePhoto(WriteableBitmap writeableBitmap)
        {
            //capturedPhotoBlue.Source = null;
            //capturedPhotoPurple.Source = null;
            //capturedPhotoRed.Source = null;
            //capturedPhotoGreen.Source = null;
            //text_couleur.Text = "loading...";
            text_exeption.Text = "";
            NbrOfBullets--;
            if (NbrOfBullets >= 0)
            {
                textBlock.Text = NbrOfBullets + " / 10";
                Bitmap bitmap = (Bitmap)(writeableBitmap);

                IntPoint screenCenter = new IntPoint((int)writeableBitmap.PixelWidth / 2, (int)writeableBitmap.PixelHeight / 2);
                List<IntPoint> corners = new List<IntPoint>();
                IntPoint c1 = new IntPoint((int)(screenCenter.X - 100), (int)(screenCenter.Y - 100));
                IntPoint c2 = new IntPoint((int)(screenCenter.X + 100), (int)(screenCenter.Y - 100));
                IntPoint c3 = new IntPoint((int)(screenCenter.X + 100), (int)(screenCenter.Y + 100));
                IntPoint c4 = new IntPoint((int)(screenCenter.X - 100), (int)(screenCenter.Y + 100));
                corners.Add(c1);
                corners.Add(c2);
                corners.Add(c3);
                corners.Add(c4);
                QuadrilateralTransformation quadrilateralTransformation =
                    new QuadrilateralTransformation(corners, 400, 400);
                
                try
                {
                    miniImagePurple = quadrilateralTransformation.Apply(bitmap);
                    miniImageRed = quadrilateralTransformation.Apply(bitmap);
                    miniImageGreen = quadrilateralTransformation.Apply(bitmap);
                    miniImageBlue = quadrilateralTransformation.Apply(bitmap);
                    //processingResultImage.Source = (WriteableBitmap)miniImagePurple;
                }
                catch
                {
                    text_exeption.Text = "Erreur convert to Bitmap1";
                    return null;
                }

                text_exeption.Text = ContainPurple(miniImagePurple);
                text_exeption.Text = ContainRed(miniImageRed);
                text_exeption.Text = ContainGreen(miniImageGreen);
                text_exeption.Text = ContainBlue(miniImageBlue);
                
            }
            return null;
        }

        public String ContainPurple(Bitmap bitmap)
        {
            HSLFiltering filter = new HSLFiltering();
            // set color ranges to keep
            filter.Hue = new IntRange(220, 290);
            filter.Saturation = new Range(0.4f, 0.8f);
            filter.Luminance = new Range(0.35f, 0.78f);
            filter.ApplyInPlace(bitmap);

            AForge.Imaging.BlobCounter blobCounterPutple = new AForge.Imaging.BlobCounter();
            blobCounterPutple.FilterBlobs = true;
            blobCounterPutple.ObjectsOrder = ObjectsOrder.Area;
            blobCounterPutple.MinHeight = 10;
            blobCounterPutple.MinWidth = 10;
            blobCounterPutple.ProcessImage(bitmap);
            AForge.Imaging.Blob[] blobsPurple = blobCounterPutple.GetObjectsInformation();
            SimpleShapeChecker shapeCheckerPurple = new SimpleShapeChecker();
            //capturedPhotoBlue.Source = (WriteableBitmap)bitmap;
            for (int iBlob = 0; iBlob < blobsPurple.Length; iBlob++)
            {
                for (int jBlob = iBlob + 1; jBlob < blobsPurple.Length; jBlob++)
                {
                    if ((Math.Abs(blobsPurple[iBlob].CenterOfGravity.X - blobsPurple[jBlob].CenterOfGravity.X) < blobsPurple[iBlob].Rectangle.Width / 2)
                        && (Math.Abs(blobsPurple[iBlob].CenterOfGravity.Y - blobsPurple[jBlob].CenterOfGravity.Y) < 3 * blobsPurple[iBlob].Rectangle.Height)
                     && (Math.Abs(blobsPurple[iBlob].Area - blobsPurple[jBlob].Area) < blobsPurple[iBlob].Area / 2))
                    {
                        //try { text_log.Text += "i" + iBlob + "j" + jBlob + "diff" + Math.Abs(blobsPurple[iBlob].CenterOfGravity.X - blobsPurple[jBlob].CenterOfGravity.X) + "w" + blobsPurple[iBlob].Rectangle.Width / 2; }
                        //catch (Exception ex) { text_log.Text = ex + ""; }                        
                        return "Purple";
                    }
                }
            }
            return "";
        }
        private String ContainBlue(Bitmap bitmap)
        {
            HSLFiltering blueFilter = new HSLFiltering();
            blueFilter.Hue = new IntRange(150, 210);
            blueFilter.Saturation = new Range(0.3f, 1);
            blueFilter.Luminance = new Range(0.20f, 0.8f);
            blueFilter.ApplyInPlace(bitmap);
            

            AForge.Imaging.BlobCounter blobCounterBlue = new AForge.Imaging.BlobCounter();
            blobCounterBlue.FilterBlobs = true;
            blobCounterBlue.ObjectsOrder = ObjectsOrder.Area;
            blobCounterBlue.MinHeight = 10;
            blobCounterBlue.MinWidth = 10;
            blobCounterBlue.ProcessImage(bitmap);
            //capturedPhotoBlue.Source = (WriteableBitmap)bitmap;
            AForge.Imaging.Blob[] blobsBlue = blobCounterBlue.GetObjectsInformation();
            for (int iBlob = 0; iBlob < blobsBlue.Length; iBlob++)
            {
                for (int jBlob = iBlob + 1; jBlob < blobsBlue.Length; jBlob++)
                {
                    if ((Math.Abs(blobsBlue[iBlob].CenterOfGravity.X - blobsBlue[jBlob].CenterOfGravity.X) < blobsBlue[iBlob].Rectangle.Width / 2)
                        && (Math.Abs(blobsBlue[iBlob].CenterOfGravity.Y - blobsBlue[jBlob].CenterOfGravity.Y) < 3 * blobsBlue[iBlob].Rectangle.Height)
                     && (Math.Abs(blobsBlue[iBlob].Area - blobsBlue[jBlob].Area) < blobsBlue[iBlob].Area / 2))
                    {
                        //try { text_log.Text += "i" + iBlob + "j" + jBlob + "diff" + Math.Abs(blobsBlue[iBlob].CenterOfGravity.X - blobsBlue[jBlob].CenterOfGravity.X) + "w" + blobsBlue[iBlob].Rectangle.Width / 2; }
                        //catch (Exception ex) { text_log.Text = ex + ""; }                        
                        return "Blue";
                    }
                }
            }
            
            return "";
        }
        private String ContainGreen(Bitmap bitmap)
        {
            HSLFiltering filter = new HSLFiltering();
            // set color ranges to keep
            filter.Hue = new IntRange(80, 150);
            filter.Saturation = new Range(0.4f, 1);
            filter.Luminance = new Range(0.2f, 0.7f);
            filter.ApplyInPlace(bitmap);
            AForge.Imaging.BlobCounter blobCounterGreen = new AForge.Imaging.BlobCounter();
            blobCounterGreen.FilterBlobs = true;
            blobCounterGreen.ObjectsOrder = ObjectsOrder.Area;
            blobCounterGreen.MinHeight = 10;
            blobCounterGreen.MinWidth = 10;
            blobCounterGreen.ProcessImage(bitmap);
            //capturedPhotoGreen.Source = (WriteableBitmap)bitmap;
            AForge.Imaging.Blob[] blobsGreen = blobCounterGreen.GetObjectsInformation();
            for (int iBlob = 0; iBlob < blobsGreen.Length; iBlob++)
            {
                for (int jBlob = iBlob + 1; jBlob < blobsGreen.Length; jBlob++)
                {
                    if ((Math.Abs(blobsGreen[iBlob].CenterOfGravity.X - blobsGreen[jBlob].CenterOfGravity.X) < blobsGreen[iBlob].Rectangle.Width / 2)
                        && (Math.Abs(blobsGreen[iBlob].CenterOfGravity.Y - blobsGreen[jBlob].CenterOfGravity.Y) < 3 * blobsGreen[iBlob].Rectangle.Height)
                     && (Math.Abs(blobsGreen[iBlob].Area - blobsGreen[jBlob].Area) < blobsGreen[iBlob].Area / 2))
                    {
                        //try { text_log.Text += "i" + iBlob + "j" + jBlob + "diff" + Math.Abs(blobsGreen[iBlob].CenterOfGravity.X - blobsGreen[jBlob].CenterOfGravity.X) + "w" + blobsGreen[iBlob].Rectangle.Width / 2; }
                        //catch (Exception ex) { text_log.Text = ex + ""; }                        
                        return "Green";
                    }
                }
            }
            //text_log.Text += "nG";
            return "";
        }

        public String ContainRed(Bitmap bitmap)
        {
            HSLFiltering filterRed = new HSLFiltering();
            filterRed.Hue = new IntRange(301, 9);//red at night (334, 40)
            filterRed.Saturation = new Range(0.4f, 1);//0.55
            filterRed.Luminance = new Range(0.1f, 0.68f);//75
            filterRed.ApplyInPlace(bitmap);

            AForge.Imaging.BlobCounter blobCounterRed = new AForge.Imaging.BlobCounter();
            blobCounterRed.FilterBlobs = true;
            blobCounterRed.ObjectsOrder = ObjectsOrder.Area;
            blobCounterRed.MinHeight = 10;
            blobCounterRed.MinWidth = 10;
            blobCounterRed.ProcessImage(bitmap);

            AForge.Imaging.Blob[] blobsRed = blobCounterRed.GetObjectsInformation();
            //capturedPhotoRed.Source = (WriteableBitmap)bitmap;
            //Mean filter = new Mean();
            //// apply the filter
            //filter.ApplyInPlace(bitmap);
            for (int iBlob=0; iBlob < blobsRed.Length; iBlob++)
            {
                for (int jBlob = iBlob+1; jBlob < blobsRed.Length; jBlob++)
                { 
                    if ((Math.Abs(blobsRed[iBlob].CenterOfGravity.X - blobsRed[jBlob].CenterOfGravity.X) < blobsRed[iBlob].Rectangle.Width / 2)
                        && (Math.Abs(blobsRed[iBlob].CenterOfGravity.Y - blobsRed[jBlob].CenterOfGravity.Y) < 3 * blobsRed[iBlob].Rectangle.Height)
                     && (Math.Abs(blobsRed[iBlob].Area - blobsRed[jBlob].Area) < blobsRed[iBlob].Area/2))
                    {
                        //try { text_log.Text += "i" + iBlob + "j" + jBlob + "diff" + Math.Abs(blobsRed[iBlob].CenterOfGravity.X - blobsRed[jBlob].CenterOfGravity.X) + "w" + blobsRed[iBlob].Rectangle.Width / 2; }
                        //catch (Exception ex) { text_log.Text = ex + ""; }                        
                        return "red";
                    }
                }
            }
            return "";

        }

        public Boolean ContainGrey(Bitmap bitmap)
        {
            ColorFiltering GrayFilter = new ColorFiltering();
            GrayFilter.Red = new IntRange(100, 150);
            GrayFilter.Green = new IntRange(100, 150);
            GrayFilter.Blue = new IntRange(100, 150);
            GrayFilter.ApplyInPlace(bitmap);
            AForge.Imaging.BlobCounter blobCounterGreen = new AForge.Imaging.BlobCounter();

            blobCounterGreen.FilterBlobs = true;
            blobCounterGreen.MinHeight = 5;
            blobCounterGreen.MinWidth = 5;
            blobCounterGreen.ProcessImage(bitmap);
            AForge.Imaging.Blob[] blobsGreen = blobCounterGreen.GetObjectsInformation();
            SimpleShapeChecker shapeCheckerGreen = new SimpleShapeChecker();

            if (blobsGreen.Length > 0)
            {
                return true;
            }
            return false;
        }

        public Boolean ContainOrange(Bitmap bitmap)
        {
            ColorFiltering RedFilter = new ColorFiltering();
            RedFilter.Red = new IntRange(100, 255);
            RedFilter.Green = new IntRange(0, 100);
            RedFilter.Blue = new IntRange(0, 100);
            RedFilter.ApplyInPlace(bitmap);
            AForge.Imaging.BlobCounter blobCounterGreen = new AForge.Imaging.BlobCounter();

            blobCounterGreen.FilterBlobs = true;
            blobCounterGreen.MinHeight = 5;
            blobCounterGreen.MinWidth = 5;
            blobCounterGreen.ProcessImage(bitmap);
            AForge.Imaging.Blob[] blobsGreen = blobCounterGreen.GetObjectsInformation();
            SimpleShapeChecker shapeCheckerGreen = new SimpleShapeChecker();

            if (blobsGreen.Length > 0)
            {
                return true;
            }
            return false;
        }

        public Boolean ContainCyan(Bitmap bitmap)
        {
            ColorFiltering filter = new ColorFiltering();
            filter.Red = new IntRange(0, 100);
            filter.Green = new IntRange(100, 255);
            filter.Blue = new IntRange(100, 255);
            filter.ApplyInPlace(bitmap);

            AForge.Imaging.BlobCounter blobCounterGreen = new AForge.Imaging.BlobCounter();
            blobCounterGreen.FilterBlobs = true;
            blobCounterGreen.MinHeight = 5;
            blobCounterGreen.MinWidth = 5;
            blobCounterGreen.ProcessImage(bitmap);

            AForge.Imaging.Blob[] blobsGreen = blobCounterGreen.GetObjectsInformation();
            SimpleShapeChecker shapeCheckerGreen = new SimpleShapeChecker();
            if (blobsGreen.Length > 0)
            {
                return true;
            }
            return false;
        }


        private async void button_Reload_Click(object sender, RoutedEventArgs e)
        {
            buttonReload.Visibility = Visibility.Collapsed;
            canShoot = false;
            await Task.Delay(TimeSpan.FromSeconds(0.45));
            textBlock.Text = "";
            await Task.Delay(TimeSpan.FromSeconds(0.45));
            textBlock.Text = ".";
            await Task.Delay(TimeSpan.FromSeconds(0.45));
            textBlock.Text = "..";
            await Task.Delay(TimeSpan.FromSeconds(0.45));
            textBlock.Text = "....";
            await Task.Delay(TimeSpan.FromSeconds(0.45));
            textBlock.Text = "......";
            await Task.Delay(TimeSpan.FromSeconds(0.45));
            textBlock.Text = "........";
            await Task.Delay(TimeSpan.FromSeconds(0.45));
            textBlock.Text = "..........";
            NbrOfBullets = 10;
            textBlock.Text = "10 / 10";
            buttonReload.Visibility = Visibility.Visible;
            canShoot = true;
        }

        private async void timing()
        {
            do
            {
                String zeroMin;
                String zeroSec;
                await Task.Delay(TimeSpan.FromSeconds(1));
                sec++;
                if (sec == 60) { min++; sec = 0; };
                if (min > 9) zeroMin = ""; else zeroMin = "0";
                if (sec > 9) zeroSec = ""; else zeroSec = "0";
                time_text.DataContext = zeroMin + min + ":" + zeroSec + sec;
            } while (min < 60);
        }

        private void button_Shoot_Click(object sender, RoutedEventArgs e)
        {
            if (canShoot)
            {
                try
                {
                    testVibrationDevice.Vibrate(TimeSpan.FromSeconds(0.5));
//                    testVibrationDevice.Cancel();
                    canShoot = false;
                    _videoEngine.Messenger.FrameRequestId = ColorPickFrameRequestId;                   
                    canShoot = true;
                    
                }
                catch (Exception ex)
                {
                    text_exeption.Text = "Erreur capture:"+ex;
                }
            }
        }
    }

}
