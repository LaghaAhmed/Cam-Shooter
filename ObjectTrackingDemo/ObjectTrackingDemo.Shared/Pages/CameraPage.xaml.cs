using System;
using System.Threading.Tasks;
using ObjectTrackingDemo.Common;
using Windows.ApplicationModel.Core;
using Windows.Media.Capture;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Input;
using System.Drawing;
using AForge.Imaging.Filters;
using AForge.Imaging;
using AForge.Math.Geometry;
using AForge;
using System.Collections.Generic;
using RealtimeFramework.Messaging;
using multiplayerc;
using CamShooter2;
using Windows.Phone.Devices.Notification;


namespace ObjectTrackingDemo
{
    public sealed partial class CameraPage : Page
    {
        int hittttt=0;
        int nbrLiving;
        Boolean okkk = false;
        OrtcExample ortcExample;
        private OrtcClient ortcClient;
        MediaCapture mediaCapture;
        int NbrOfBullets = 10;
        Boolean canShoot = true;
        int score = 0;
        int kil = 0;
        int death = 0;
        double hurtOpa = 0;
        int min = 0;
        int sec = 0;
        private const int ColorPickFrameRequestId = 42;
        VibrationDevice testVibrationDevice = VibrationDevice.GetDefault();
        public List<Player> maListe;

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
            DependencyProperty.Register("ControlsVisibility", typeof(Visibility), typeof(CameraPage),
                new PropertyMetadata(Visibility.Visible));

        private VideoEngine _videoEngine;
        private ActionQueue _actionQueue;
        private Settings _settings;


      //  private Windows.Foundation.Point _viewFinderCanvasTappedPoint;
        private NavigationHelper _navigationHelper;

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this._navigationHelper; }
        }

        public CameraPage()
        {
            InitializeComponent();
            try { gameUpdate.Text = SharedInformation.myNumber; } catch { gameUpdate.Text += "erreur 0"; }
            try { nbrLiving = SharedInformation.ListOfPlayers.Count; gameUpdate.Text += nbrLiving; } catch { gameUpdate.Text += "erreur counting"; }
            


            timing();
            message.Focus(FocusState.Keyboard);
            _navigationHelper = new NavigationHelper(this);
            controlBar.HideButtonClicked += OnHideButtonClicked;
            controlBar.ToggleFlashButtonClicked += OnToggleFlashButtonClicked;
            controlBar.SettingsButtonClicked += OnSettingsButtonClicked;
            this.Loaded += BasicPage_Loaded;
            this.LayoutUpdated += BasicPage_LayoutUpdated;

            _videoEngine = VideoEngine.Instance;
            
            _settings = App.Settings;

            NavigationCacheMode = NavigationCacheMode.Required;

            ortcClient = new RealtimeFramework.Messaging.OrtcClient();
            ortcExample = new OrtcExample();
          
            ortcExample.Channel = SharedInformation.savedRoom.name;
            ortcExample.AuthenticationToken = SharedInformation.savedRoom.password;

            if (ortcExample.Channel.Equals(SharedInformation.savedRoom.name))
            {

                message.DataContext = ortcExample;
                ortcExample.DoConnectDisconnect();

            }

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            
            message.Focus(FocusState.Keyboard);
            //await Dispatcher.RunAsync(CoreDispatcherPriority.Normal,() => buttonShoot.Focus(FocusState.Pointer));
            buttonShoot.Focus(FocusState.Pointer);
            viseur.Visibility = Visibility.Visible;
            gameOver_StackPanel.Visibility = Visibility.Collapsed;
            //buttonHp.Content = health + "";
            dead.DataContext = 0;
            imghurt.DataContext = hurtOpa;
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
            //showScore();
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
                Bitmap quad2;
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
                    quad2 = quadrilateralTransformation.Apply(bitmap);
                }
                catch
                {
                    return null;
                }
                if(colorOfPlayer(quad) == false)
                    colorOfPlayer2(quad2);
            }
            return null;
        }

        private Boolean colorOfPlayer(Bitmap bitmap)
        {
            Boolean ok = false;

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
            try { blobCounterRed.ProcessImage(bitmap); } catch (Exception es) { gameUpdate.Text = "filter"; }


            AForge.Imaging.Blob[] blobsRed = blobCounterRed.GetObjectsInformation();
            //Mean filter = new Mean();
            //// apply the filter
            //filter.ApplyInPlace(bitmap);
            bool fin = false;
            try
            {
                for (int iBlob = 0; iBlob < blobsRed.Length; iBlob++)
                {
                    for (int jBlob = iBlob + 1; jBlob < blobsRed.Length; jBlob++)
                    {
                        if ((Math.Abs(blobsRed[iBlob].CenterOfGravity.X - blobsRed[jBlob].CenterOfGravity.X) < blobsRed[iBlob].Rectangle.Width / 2)
                            && (Math.Abs(blobsRed[iBlob].CenterOfGravity.Y - blobsRed[jBlob].CenterOfGravity.Y) < 3 * blobsRed[iBlob].Rectangle.Height)
                         && (Math.Abs(blobsRed[iBlob].Area - blobsRed[jBlob].Area) < blobsRed[iBlob].Area / 2))
                        {
                           // gameUpdate.Text = "OK" + SharedInformation.myNumber + getPlayerNumberByColor("Red");
                            ortcExample.DoSendMessage(SharedInformation.myNumber + getPlayerNumberByColor("Red"));
                            fin = true;
                            return true;
                        }
                    }
                    if (fin) break;
                }
            }
            catch (Exception es) {

                gameUpdate.Text = "erreur";
            }
            return ok;
        }

        private void colorOfPlayer2(Bitmap bitmap2)
        {
            Bitmap bit2 = null;
            bit2 = bitmap2;
            bool fin = false;
            HSLFiltering filter = new HSLFiltering();
            // set color ranges to keep
            filter.Hue = new IntRange(150, 210);
            filter.Saturation = new Range(0.3f, 1);
            filter.Luminance = new Range(0.20f, 0.8f);
            filter.ApplyInPlace(bit2);
            AForge.Imaging.BlobCounter blobCounterGreen = new AForge.Imaging.BlobCounter();
            blobCounterGreen.FilterBlobs = true;
            blobCounterGreen.ObjectsOrder = ObjectsOrder.Area;
            blobCounterGreen.MinHeight = 10;
            blobCounterGreen.MinWidth = 10;
            blobCounterGreen.ProcessImage(bit2);

            AForge.Imaging.Blob[] blobsGreen = blobCounterGreen.GetObjectsInformation();


            try
            {
                for (int iBlob = 0; iBlob < blobsGreen.Length; iBlob++)
                {

                    for (int jBlob = iBlob + 1; jBlob < blobsGreen.Length; jBlob++)
                    {
                        if ((Math.Abs(blobsGreen[iBlob].CenterOfGravity.X - blobsGreen[jBlob].CenterOfGravity.X) < blobsGreen[iBlob].Rectangle.Width / 2)
                            && (Math.Abs(blobsGreen[iBlob].CenterOfGravity.Y - blobsGreen[jBlob].CenterOfGravity.Y) < 3 * blobsGreen[iBlob].Rectangle.Height)
                         && (Math.Abs(blobsGreen[iBlob].Area - blobsGreen[jBlob].Area) < blobsGreen[iBlob].Area / 2))
                        {
                            //gameUpdate.Text = "OK" + SharedInformation.myNumber + getPlayerNumberByColor("Blue");
                            ortcExample.DoSendMessage(SharedInformation.myNumber + getPlayerNumberByColor("Blue"));
                            fin = true;
                            break;
                        }
                    }
                    if (fin) break;

                }
            }
            catch (Exception es)
            {

                gameUpdate.Text = "erreur22";
            }

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

        /*private void buttonHp_Click(object sender, RoutedEventArgs e)
        {
            hurtOpa += 0.4;
            imghurt.DataContext = hurtOpa;

            if (hurtOpa >= 1)
            {
                hurtOpa += 1;
                viseur.Visibility = Visibility.Collapsed;
                nameOfKiller_text.DataContext = "Foulen";
                gameOver_StackPanel.Visibility = Visibility.Visible;
                canShoot = false;
                dead.DataContext = 0.7;
                buttonReload.Visibility = Visibility.Collapsed;
                //textBlock.Visibility = Visibility.Collapsed;
            }
        }*/

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
                    
                    FirstSound.Play();
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


        private async void textchange(object sender, TextChangedEventArgs e)
        {
            if (message.Text != "")
            {
                String test;
                test = message.Text;
                String numm = SharedInformation.myNumber;
                if (test.Equals("over"))
                {
                    showScore();
                }
                else if (getPlayerNameByNumber(test[1]).Equals(SharedInformation.myName))
                {
                    hurtOpa += 0.4;
                    imghurt.DataContext = hurtOpa;
                    hittttt++;
                    if (hittttt >= 3)
                    {
                        viseur.Visibility = Visibility.Collapsed;
                        nameOfKiller_text.DataContext = getPlayerNameByNumber(test[0]);
                        gameOver_StackPanel.Visibility = Visibility.Visible;

                        canShoot = false;
                        dead.DataContext = 0.7;
                        text_death.Text = "" + 1;
                        ortcExample.DoClearLog();
                        if (okkk == false)
                            ortcExample.DoSendMessage(test + "dead");



                        await Task.Delay(TimeSpan.FromSeconds(2));

                        SharedInformation.ListOfPlayers[0].kill = SharedInformation.ListOfPlayers[0].kill + 1;
                        SharedInformation.ListOfPlayers[0].score = SharedInformation.ListOfPlayers[0].score + 10;
                        SharedInformation.ListOfPlayers[1].death = SharedInformation.ListOfPlayers[1].death + 1;
                        SharedInformation.ListOfPlayers[1].score = SharedInformation.ListOfPlayers[1].score - 5;

                        nbrLiving--;
                        if (nbrLiving == 1)
                        {
                            maListe = SharedInformation.ListOfPlayers;
                            ListeScore.DataContext = maListe;
                            showScore();
                        }

                        okkk = true;
                        //textBlock.Visibility = Visibility.Collapsed;
                    }

                }
                else if (test.Length >=5)
                {
                    kil++;
                    int n1 = (int)test[0] + 10;
                    text_score.Text = n1 + "";
                    SharedInformation.ListOfPlayers[0].kill = SharedInformation.ListOfPlayers[0].kill + 1;
                    SharedInformation.ListOfPlayers[0].score = SharedInformation.ListOfPlayers[0].score + 10;
                    SharedInformation.ListOfPlayers[1].death = SharedInformation.ListOfPlayers[1].death + 1;
                    SharedInformation.ListOfPlayers[1].score = SharedInformation.ListOfPlayers[1].score - 5;

                    nbrLiving--;
                    if (nbrLiving == 1)
                    {
                        maListe = SharedInformation.ListOfPlayers;
                        ListeScore.DataContext = maListe;
                        showScore();
                    }
                    gameUpdate.Text = getPlayerNameByNumber(test[0]) + " killed " + getPlayerNameByNumber(test[1]);
                    ortcExample.DoClearLog();
                }
            }
        }

        private void shoot(object sender, KeyRoutedEventArgs e)
        {

            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                if (canShoot)

                {
                    try
                    {
                        testVibrationDevice.Vibrate(TimeSpan.FromSeconds(0.7));
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




        private Char getPlayerNumberByColor(String colorDetected)
        {
            Char cccc = ' ';
            foreach(Player ppp in SharedInformation.ListOfPlayers)
            {
                if (ppp.color.Equals(colorDetected))
                {
                    return ppp.number;
                }
            }
            return cccc;
        }


        private String getPlayerNameByNumber(Char numberDetected)
        {
            foreach (Player ppp in SharedInformation.ListOfPlayers)
            {
                if (ppp.number.Equals(numberDetected))
                {
                    return ppp.name;
                    
                }
            }
            return "";
        }

        private void goRoomButton(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(ListPlayer));
        }

        private void quitButton(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void showScore()
        {
            dead.DataContext = 0;
            imghurt.DataContext = 0;
            buttonShoot.Visibility = Visibility.Collapsed;
            buttonReload.Visibility = Visibility.Collapsed;
            viseur.Visibility = Visibility.Collapsed;
            gridCartouch.Visibility = Visibility.Collapsed;
            gridTopBar.Visibility = Visibility.Collapsed;
            gridScore.Visibility = Visibility.Collapsed;
            gridTime.Visibility = Visibility.Collapsed;
            gameUpdate.Visibility = Visibility.Collapsed;
            gameOver_Grid.Visibility = Visibility.Collapsed;
            scoreTableGrid.Visibility = Visibility.Visible;
            score2.Visibility = Visibility.Visible;
            score3.Visibility = Visibility.Visible;
        }

    
    }

}
