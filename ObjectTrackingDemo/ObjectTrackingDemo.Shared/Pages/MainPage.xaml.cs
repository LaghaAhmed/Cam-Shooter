using ObjectTrackingDemo;
using ObjectTrackingDemo.Pages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace CamShooter2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
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

        private void multiplayer_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(menu_room));

        }

        private void training_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SoloMode));
        }

        private void settings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(authentification));
        }

        private void shop_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Shop));
        }
        private void test_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(GameTest));
        }
    }
}
