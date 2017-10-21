using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure;
using ObjectTrackingDemo;
using Microsoft.WindowsAzure.MobileServices;
using Windows.Phone.UI.Input;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CamShooter2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class JoinRoom : Page
    {
        List<Player> lstjoueur = new List<Player>();
        private MobileServiceCollection<Rooms, Rooms> rooms;
        private IMobileServiceTable<Rooms> roomTable = App.MobileService.GetTable<Rooms>();
        public JoinRoom()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }


        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.Navigate(typeof(ListRoom));
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

        private async void btnstart_Click(object sender, RoutedEventArgs e)
        {



            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                rooms = await roomTable
                    .Where(rooms => rooms.name == SharedInformation.roomCurrent)
                    .ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException ee)
            {
                exception = ee;
            }








            string message = "";


            if (pwdRoom.Password == rooms[0].password)
            {
                if (rooms[0].player1 == null || rooms[0].player1 == "")
                {
                    rooms[0].player1 = SharedInformation.myName; rooms[0].player1color = SharedInformation.myColor; await roomTable.UpdateAsync(rooms[0]);
                    SharedInformation.savedRoom = rooms[0];
                    SharedInformation.myNumber = "1";
                    Frame.Navigate(typeof(ListPlayer));

                }
                else if (rooms[0].player2 == null || rooms[0].player2 == "")
                {
                    rooms[0].player2 = SharedInformation.myName; rooms[0].player2color = SharedInformation.myColor; await roomTable.UpdateAsync(rooms[0]);
                    SharedInformation.savedRoom = rooms[0];
                    SharedInformation.myNumber = "2";
                    Frame.Navigate(typeof(ListPlayer));

                }
                else if (rooms[0].player3 == null || rooms[0].player3 == "")
                {
                    rooms[0].player3 = SharedInformation.myName; rooms[0].player3color = SharedInformation.myColor; await roomTable.UpdateAsync(rooms[0]);
                    SharedInformation.savedRoom = rooms[0];
                    SharedInformation.myNumber = "3";
                    Frame.Navigate(typeof(ListPlayer));

                }
                else if (rooms[0].player4 == null || rooms[0].player4 == "")
                {
                    rooms[0].player4 = SharedInformation.myName; rooms[0].player4color = SharedInformation.myColor; await roomTable.UpdateAsync(rooms[0]);
                    SharedInformation.savedRoom = rooms[0];
                    SharedInformation.myNumber = "4";
                    Frame.Navigate(typeof(ListPlayer));
                }
                else
                {
                    rooms[0].player4 = ""; await roomTable.UpdateAsync(rooms[0]);
                    message = "room is full";
                    var dialog = new MessageDialog(message);
                    await dialog.ShowAsync();
                }


                //await roomTable.UpdateAsync(rooms[0]);



            }
            else
            {
                message = "verify password room";
                var dialog = new MessageDialog(message);
                await dialog.ShowAsync();
            }

        }
    }
}
