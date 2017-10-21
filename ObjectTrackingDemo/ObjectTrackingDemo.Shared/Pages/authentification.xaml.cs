using ObjectTrackingDemo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CamShooter2
{
   
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class authentification : Page
    {
        public authentification()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
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
        }

        private async void Connect_Click(object sender, RoutedEventArgs e)
        {
            /*
            string message = "";
            if ((login.Text == "hedi") && (password.Password == "123"))
            {
                Frame.Navigate(typeof(menu_room));
            }
            else
            {
                message = message = "Wrong Username/password";
                var dialog = new MessageDialog(message);
                await dialog.ShowAsync();
            }
            */
            SharedInformation.myName = Login.Text;
            SharedInformation.myColor = ((ComboBoxItem)ComboBoxMenu.SelectedItem).Content.ToString();
            
            Frame.Navigate(typeof(menu_room));
        }

    }
}
