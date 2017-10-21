using ObjectTrackingDemo;
using RealtimeFramework.Messaging;
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

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace multiplayerc
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class room : Page
    {


       
        public room()
        {
            this.InitializeComponent();
            try { 
            Room p1 = IsolatedStorageHelper.GetObject<Room>("100");
            if (!p1.channel.Equals(""))
            {
                IsolatedStorageHelper.DeleteObject("100");
            }
            }
            catch{ }

        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
        }

        private void BtConnectDisconnect_Click(object sender, RoutedEventArgs e)
        {
            Room p = new Room() { channel = this.channel.Text, token = this.token.Text };
            IsolatedStorageHelper.SaveObject("100", p);
            this.Frame.Navigate(typeof(CameraPage));
            

        }
    }
}
