using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using multiplayerc;
using ObjectTrackingDemo;
using Microsoft.WindowsAzure.MobileServices;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.Phone.UI.Input;
using ObjectTrackingDemo.Pages;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CamShooter2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SoloMode : Page
    {
      
      
        public SoloMode()
        {

            
          
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            try
            {
                score.Text = IsolatedStorageHelper.GetObject<String>("1");
            }
            catch (Exception e)
            {

            }
        }


        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.Navigate(typeof(MainPage));
            }
        }

     

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
            //await RefreshTodoItems();
        }

        private  void CreateR_Click(object sender, RoutedEventArgs e)
        {
          this.Frame.Navigate(typeof(cameraSolo));
        }


    }
}
