using AForge;
using AForge.Imaging;
using AForge.Imaging.Filters;
using AForge.Math.Geometry;
using CamShooter2;
using ObjectTrackingDemo.Common;
using RealtimeFramework.Messaging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Phone.UI.Input;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace ObjectTrackingDemo.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class cameraSolo : Page
    {
        WriteableBitmap wbt;
        MediaCapture mediaCapture;
        int NbrOfBullets = 10;
        Boolean canShoot = true;
        int score = 0;
        int min = 0;
        int sec = 0;
      
        private const int ColorPickFrameRequestId = 42;

        bool bAfterLoaded = false;

        private void BasicPage_LayoutUpdated(object sender, object e)
        {
            if (bAfterLoaded)
            {
                buttonShoot.Focus(FocusState.Programmatic);
                bAfterLoaded = !bAfterLoaded;
            }
        }

        private void BasicPage_Loaded(object sender, RoutedEventArgs e)
        {
            bAfterLoaded = !bAfterLoaded;
        }


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
            DependencyProperty.Register("ControlsVisibility", typeof(Visibility), typeof(cameraSolo),
                new PropertyMetadata(Visibility.Visible));

        private VideoEngine _videoEngine;
        private ActionQueue _actionQueue;
        private Settings _settings;


        private Windows.Foundation.Point _viewFinderCanvasTappedPoint;
        private NavigationHelper _navigationHelper;

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this._navigationHelper; }
        }
        public cameraSolo()
        {
            this.InitializeComponent();


            timing();
            _navigationHelper = new NavigationHelper(this);
            controlBar.HideButtonClicked += OnHideButtonClicked;
            controlBar.ToggleFlashButtonClicked += OnToggleFlashButtonClicked;
            controlBar.SettingsButtonClicked += OnSettingsButtonClicked;
            this.Loaded += BasicPage_Loaded;
            this.LayoutUpdated += BasicPage_LayoutUpdated;
            _videoEngine = VideoEngine.Instance;
            _settings = App.Settings;
            NavigationCacheMode = NavigationCacheMode.Required;
            try
            {
                IsolatedStorageHelper.DeleteObject("1");

            }
            catch (Exception e)
            {

            }

        }






        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {




            base.OnNavigatedTo(e);

          
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => buttonShoot.Focus(FocusState.Pointer));
            buttonShoot.Focus(FocusState.Pointer);
            viseur.Visibility = Visibility.Visible;
           
            //buttonHp.Content = health + "";
         
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            _actionQueue = new ActionQueue();
            _actionQueue.ExecuteIntervalInMilliseconds = 500;

            await InitializeAndStartVideoEngineAsync();
            SetFlash(_settings.Flash, false);
            settingsPanelControl.ModeChanged += _videoEngine.OnModeChanged;
            settingsPanelControl.RemoveNoiseChanged += _videoEngine.OnRemoveNoiseChanged;
            settingsPanelControl.ApplyEffectOnlyChanged += _videoEngine.OnApplyEffectOnlyChanged;
            settingsPanelControl.IsoChanged += _videoEngine.OnIsoSettingsChangedAsync;
            settingsPanelControl.ExposureChanged += _videoEngine.OnExposureSettingsChangedAsync;
            _videoEngine.ShowMessageRequest += OnVideoEngineShowMessageRequestAsync;
            _videoEngine.Messenger.FrameCaptured += OnFrameCapturedAsync;
            _videoEngine.Messenger.PostProcessComplete += OnPostProcessCompleteAsync;

            Window.Current.VisibilityChanged += OnVisibilityChangedAsync;

            DataContext = this;
           

            timing();
            _navigationHelper = new NavigationHelper(this);
            controlBar.HideButtonClicked += OnHideButtonClicked;
            controlBar.ToggleFlashButtonClicked += OnToggleFlashButtonClicked;
            controlBar.SettingsButtonClicked += OnSettingsButtonClicked;
            this.Loaded += BasicPage_Loaded;
            this.LayoutUpdated += BasicPage_LayoutUpdated;
            _videoEngine = VideoEngine.Instance;
            _settings = App.Settings;
            NavigationCacheMode = NavigationCacheMode.Required;
            try
            {
                IsolatedStorageHelper.DeleteObject("1");

            }
            catch (Exception ettt)
            {

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            _videoEngine.Torch = false;
            _actionQueue.Dispose();
            _settings.Save();

            settingsPanelControl.ModeChanged -= _videoEngine.OnModeChanged;
            settingsPanelControl.RemoveNoiseChanged -= _videoEngine.OnRemoveNoiseChanged;
            settingsPanelControl.ApplyEffectOnlyChanged -= _videoEngine.OnApplyEffectOnlyChanged;
            settingsPanelControl.IsoChanged -= _videoEngine.OnIsoSettingsChangedAsync;
            settingsPanelControl.ExposureChanged -= _videoEngine.OnExposureSettingsChangedAsync;
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

            controlBar.FlashButtonVisibility = _videoEngine.IsTorchSupported
                ? Visibility.Visible : Visibility.Collapsed;

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

            controlBar.IsFlashOn = _videoEngine.Torch;

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
                WriteableBitmap bitmap = await ImageProcessingUtils.PixelArrayToWriteableBitmapAsync(pixelArray, frameWidth, frameHeight);

                if (bitmap != null)
                {
                    CapturePhoto(bitmap);
                    //capturedPhotoImage.Source = bitmap;
                    //bitmap.Invalidate();
                    //capturedPhotoImage.Visibility = Visibility.Visible;
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

        private void OnHideButtonClicked(object sender, RoutedEventArgs e)
        {
            ControlsVisibility = Visibility.Collapsed;
        }

        private void OnToggleFlashButtonClicked(object sender, RoutedEventArgs e)
        {
            SetFlash(!_videoEngine.Torch);
        }

        private void OnSettingsButtonClicked(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (settingsPanelControl.Visibility == Visibility.Visible)
            {
                settingsPanelControl.Hide();
            }
            else
            {
                settingsPanelControl.Show();
            }
        }

        private void OnSwitchToPhotosButtonClicked(object sender, RoutedEventArgs e)
        {
            if (Frame.BackStack.Count == 0)
            {
                _settings.AppMode = AppMode.Photo;
                _settings.Save();
            }
            else
            {
                NavigationHelper.GoBack();
            }
        }

        /// <summary>
        /// Picks and sets the color from the point, which was tapped.
        /// However, if controls were hidden, their visibility is restored but no color is picked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnViewfinderCanvasTapped(object sender, TappedRoutedEventArgs e)
        {
            if (ControlsVisibility == Visibility.Collapsed)
            {
                ControlsVisibility = Visibility.Visible;
                return;
            }

            if (settingsPanelControl.Visibility == Visibility.Visible)
            {
                settingsPanelControl.Hide();
                return;
            }
            _videoEngine.Messenger.FrameRequestId = ColorPickFrameRequestId;
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
            NbrOfBullets--;
            if (NbrOfBullets >= 0)
            {
                textBlock.Text = NbrOfBullets + " / 10";
                Bitmap bitmap = (Bitmap)(writeableBitmap);
                Bitmap quad;
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


                //Bitmap bitmapCopie = null;
                try
                {
                    quad = quadrilateralTransformation.Apply(bitmap);
                }
                catch
                {
                    return null;
                }

                colorOfPlayer(quad);
            }
            return null;
        }

        private  void colorOfPlayer(Bitmap bitmap)
        {

            HSLFiltering GreenFilter = new HSLFiltering();
            GreenFilter.Hue = new IntRange(200, 250);
            GreenFilter.Saturation = new Range(0.5f, 1);
            GreenFilter.Luminance = new Range(0.2f, 0.6f);
            GreenFilter.ApplyInPlace(bitmap);
            wbt = (WriteableBitmap)bitmap;
            img2.Source = wbt;
            img2.Visibility = Visibility.Visible;
            //    this.PictureBox.Source = (ImageSource)concatenate.Apply(this.img2);

            BlobCounter blobCounter = new AForge.Imaging.BlobCounter();
            blobCounter.ObjectsOrder = ObjectsOrder.Area;
            blobCounter.FilterBlobs = true;
            blobCounter.MinHeight = 10;
            blobCounter.MinWidth = 10;
            blobCounter.ProcessImage(bitmap);
            Blob[] blobs = blobCounter.GetObjectsInformation();
            SimpleShapeChecker shapeChecker = new SimpleShapeChecker();


            try { 
            for (int i = 0, n = blobs.Length; i < n; i++)
            {
                


                List<IntPoint> edgePoints = blobCounter.GetBlobsEdgePoints(blobs[i]);
                AForge.Point center;
                float radius;
                // is circle ?
                if (shapeChecker.IsCircle(edgePoints, out center, out radius))
                {
                    if ((blobs[i].Area < 17000) && (blobs[i].Area > 3000))
                    {
                        score = score + 2;
                    }
                    else if ((blobs[i].Area < 3000) && (blobs[i].Area > 1300))
                    {
                        score = score + 4;
                    }
                    else if (blobs[i].Area < 1300)
                    {
                        score = score + 6;
                    }

                }

              
               
            }
            }catch(Exception eeeee)
            {

            }

            text_score.Text = "" + score;
            
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
                if (min == 1)
                {
                    IsolatedStorageHelper.SaveObject("1", score);
                    this.Frame.Navigate(typeof(SoloMode));
                    break;
                }
            } while (min <=1);
            
           
        }

        private void button_Shoot_Click(object sender, RoutedEventArgs e)
        {
            if (canShoot)
            {
                try
                {
                    canShoot = false;
                    _videoEngine.Messenger.FrameRequestId = ColorPickFrameRequestId;
                    canShoot = true;
                }
                catch (Exception ex)
                {
                    //text_exeption.Text = "Erreur capture";
                }
            }
        }



        private async void shoot(object sender, KeyRoutedEventArgs e)
        {

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (canShoot)
                {
                    try
                    {
                        canShoot = false;
                        _videoEngine.Messenger.FrameRequestId = ColorPickFrameRequestId;
                        canShoot = true;
                    }
                    catch (Exception ex)
                    {
                        //text_exeption.Text = "Erreur capture";
                    }
                }
            }
        }




     
       
        private void goRoomButton(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ListPlayer));
        }

        private void quitButton(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }


       

    }
}
