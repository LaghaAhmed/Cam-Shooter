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
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CamShooter2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CreateRoom : Page
    {
        private MobileServiceCollection<Rooms, Rooms> rooms;
        private IMobileServiceTable<Rooms> roomTable = App.MobileService.GetTable<Rooms>();

        private MobileServiceCollection<Rooms, Rooms> items;
        private IMobileServiceTable<Rooms> todoTable = App.MobileService.GetTable<Rooms>();
        String nameText = "";
        String passwordText = "";
        public CreateRoom()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }


        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                e.Handled = true;
                Frame.Navigate(typeof(menu_room));
            }
        }

        private async Task RefreshTodoItems()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                items = await todoTable.ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items").ShowAsync();
            }
            else
            {
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

        private async void CreateR_Click(object sender, RoutedEventArgs e)
        {

            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                items = await todoTable.ToCollectionAsync();
            }
            catch (MobileServiceInvalidOperationException eeee)
            {
                exception = eeee;
            }

            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading items1").ShowAsync();
            }





            nameText = channel.Text;
            passwordText = token.Text;
            int verifier = 0;
            foreach (Rooms Rm in items)
            {

                if (Rm.name.Equals(channel.Text) && nameText != "")
                {
                    verifier += 1;
                }
            }


            if (verifier == 0)
            {
                SharedInformation.myNumber = "1";
                SharedInformation.roomCurrent = channel.Text;
                
                Rooms item = new Rooms
                {
                    name = nameText,
                    password = passwordText,
                    player1 = SharedInformation.myName,
                    player1color=SharedInformation.myColor
                };
                try { 
                await App.MobileService.GetTable<Rooms>().InsertAsync(item);

                }
                catch (MobileServiceInvalidOperationException ee)
                {

                    await new MessageDialog(ee+"", "Error loading items2").ShowAsync();
                }

                MobileServiceInvalidOperationException exceptions = null;
                try
                {
                    // This code refreshes the entries in the list view by querying the TodoItems table.
                    // The query excludes completed TodoItems.
                    rooms = await roomTable
                        .Where(rooms => rooms.name == SharedInformation.roomCurrent)
                        .ToCollectionAsync();

                    SharedInformation.savedRoom = rooms[0];
                }
                catch (MobileServiceInvalidOperationException ee)
                {
                    exceptions = ee;
                    await new MessageDialog(ee + "", "Error loading items3").ShowAsync();
                }


                this.Frame.Navigate(typeof(ListPlayer));
            }
            else
            {

                await new MessageDialog("le nom existe deja").ShowAsync();
            }



        }


    }
}
