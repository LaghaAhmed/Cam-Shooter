using Microsoft.WindowsAzure.MobileServices;
using multiplayerc;
using ObjectTrackingDemo;
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
using ObjectTrackingDemo;
using RealtimeFramework.Messaging;
using System.Diagnostics;
using Windows.UI.Popups;
using Windows.Phone.UI.Input;
// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CamShooter2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListPlayer : Page
    {
        Player plays = new Player();
        List<Player> lstjoueur = new List<Player>();
        private MobileServiceCollection<Rooms, Rooms> rooms;
        private IMobileServiceTable<Rooms> roomTable = App.MobileService.GetTable<Rooms>();
        Room r1 = new Room();
        Rooms currentroom = new Rooms();
        String myname = SharedInformation.myName; String mynumber = SharedInformation.myNumber;
        OrtcExample ortcExample;
        private OrtcClient ortcClient;
        public ListPlayer()
        {
            this.InitializeComponent();
            //currentroom.name = SharedInformation.roomCurrent;


            p1.Visibility = Visibility.Collapsed; p1co.Visibility = Visibility.Collapsed; pp1.Visibility = Visibility.Collapsed;
            p2.Visibility = Visibility.Collapsed; p2co.Visibility = Visibility.Collapsed; pp2.Visibility = Visibility.Collapsed;
            p3.Visibility = Visibility.Collapsed; p3co.Visibility = Visibility.Collapsed; pp3.Visibility = Visibility.Collapsed;
            p4.Visibility = Visibility.Collapsed; p4co.Visibility = Visibility.Collapsed; pp4.Visibility = Visibility.Collapsed;

            //Getroominfo();







            p1co.DataContext = SharedInformation.savedRoom.player1color;
            p1.Text = SharedInformation.savedRoom.player1;
            p1.Visibility = Visibility.Visible; p1co.Visibility = Visibility.Visible; pp1.Visibility = Visibility.Visible;


            //plays.name= SharedInformation.savedRoom.player1; plays.color= SharedInformation.savedRoom.player1color; plays.score = "0";plays.number = '1';
            //SharedInformation.ListOfPlayers.Add(plays);
            if (SharedInformation.savedRoom.player2 != "" && SharedInformation.savedRoom.player2 != null)
            {
                p2co.DataContext = SharedInformation.savedRoom.player2color;
                p2.Text = SharedInformation.savedRoom.player2;
                p2.Visibility = Visibility.Visible; p2co.Visibility = Visibility.Visible; pp2.Visibility = Visibility.Visible;

                //plays.name = SharedInformation.savedRoom.player2; plays.color = SharedInformation.savedRoom.player2color; plays.score = "0"; plays.number = '2';
                //SharedInformation.ListOfPlayers.Add(plays);
            }

            if (SharedInformation.savedRoom.player3 != "" && SharedInformation.savedRoom.player3 != null)
            {
                p3co.DataContext = SharedInformation.savedRoom.player3color;
                p3.Text = SharedInformation.savedRoom.player3;
                p3.Visibility = Visibility.Visible; p3co.Visibility = Visibility.Visible; pp3.Visibility = Visibility.Visible;

                //plays.name = SharedInformation.savedRoom.player3; plays.color = SharedInformation.savedRoom.player3color; plays.score = "0"; plays.number = '3';
                //SharedInformation.ListOfPlayers.Add(plays);

            }

            if (SharedInformation.savedRoom.player4 != "" && SharedInformation.savedRoom.player4 != null)
            {
                p4.Text = SharedInformation.savedRoom.player4;
                p4.Visibility = Visibility.Visible; p4co.Visibility = Visibility.Visible; pp4.Visibility = Visibility.Visible;

                p4co.DataContext = SharedInformation.savedRoom.player4color;

                //plays.name = SharedInformation.savedRoom.player4; plays.color = SharedInformation.savedRoom.player4color; plays.score = "0"; plays.number = '4';
                //SharedInformation.ListOfPlayers.Add(plays);
            }





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





        private async Task Getroominfo()
        {

            MobileServiceInvalidOperationException exception = null;
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
                exception = ee;
            }
        }


        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            r1.channel = SharedInformation.savedRoom.name; r1.token = SharedInformation.savedRoom.password;



            ortcClient = new RealtimeFramework.Messaging.OrtcClient();
            ortcExample = new OrtcExample();

            //r1 = IsolatedStorageHelper.GetObject<Room>("100");
            ortcExample.Channel = r1.channel;
            ortcExample.AuthenticationToken = r1.token;

            if (ortcExample.Channel.Equals(r1.channel))
            {
                message.DataContext = ortcExample;
                //ConsoleGrid.DataContext = ortcExample;
                ortcExample.DoConnectDisconnect();

            }
            await Task.Delay(TimeSpan.FromSeconds(3));

            ortcExample.DoSendMessage(SharedInformation.myNumber + SharedInformation.myColor.ToLower() + "/" + SharedInformation.myName);


            await Task.Delay(TimeSpan.FromSeconds(3));
            //ortcExample.DoClearLog();

            //await Getroominfo();
            await Windows.UI.ViewManagement.StatusBar.GetForCurrentView().HideAsync();
        }


        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {
                // This code refreshes the entries in the list view by querying the TodoItems table.
                // The query excludes completed TodoItems.
                rooms = await roomTable
                    .Where(rooms => rooms.name == SharedInformation.roomCurrent)
                    .ToCollectionAsync();

                SharedInformation.ListOfPlayers.Add(new Player() { name = SharedInformation.savedRoom.player1, color = SharedInformation.savedRoom.player1color, number = '1', score = 0, kill = 0, death = 0 });
                if (SharedInformation.savedRoom.player2 != "" && SharedInformation.savedRoom.player2 != null)
                    SharedInformation.ListOfPlayers.Add(new Player() { name = SharedInformation.savedRoom.player2, color = SharedInformation.savedRoom.player2color, number = '2', score = 0, kill = 0, death = 0 });
                if (SharedInformation.savedRoom.player3 != "" && SharedInformation.savedRoom.player3 != null)
                    SharedInformation.ListOfPlayers.Add(new Player() { name = SharedInformation.savedRoom.player3, color = SharedInformation.savedRoom.player3color, number = '3', score = 0, kill = 0, death = 0 });
                if (SharedInformation.savedRoom.player4 != "" && SharedInformation.savedRoom.player4 != null)
                    SharedInformation.ListOfPlayers.Add(new Player() { name = SharedInformation.savedRoom.player4, color = SharedInformation.savedRoom.player4color, number = '4', score = 0, kill = 0, death = 0 });

            }
            catch (MobileServiceInvalidOperationException ee)
            {
                exception = ee;
            }



            Frame.Navigate(typeof(CameraPage));
        }





        private void textchange(object sender, TextChangedEventArgs e)
        {
            String test;
            test = this.message.Text; String playernames = "";
            try
            {
                int iiii = test.IndexOf("/");
                playernames = test.Substring(iiii+1);
            }
            catch (Exception ee)
            {

            }



            if (test.Contains("1"))
            {
                if (test.Contains("red"))
                {
                    p1co.DataContext = "red";

                }
                if (test.Contains("blue"))
                {
                    p1co.DataContext = "blue";
                }
                if (test.Contains("green"))
                {
                    p1co.DataContext = "green";
                }
                if (test.Contains("yellow"))
                {
                    p1co.DataContext = "yellow";
                }
                if (test.Contains("orange"))
                {
                    p1co.DataContext = "orange";
                }
                if (test.Contains("purple"))
                {
                    p1co.DataContext = "purple";
                }
                p1.Text = playernames;

                SharedInformation.savedRoom.player1 = playernames;
                SharedInformation.savedRoom.player1color = test.Substring(1);
                ortcExample.DoClearLog();
            }
            if (test.Contains("2"))
            {
                if (test.Contains("red"))
                {
                    p2co.DataContext = "red";
                }
                if (test.Contains("blue"))
                {
                    p2co.DataContext = "blue";
                }
                if (test.Contains("green"))
                {
                    p2co.DataContext = "green";
                }
                if (test.Contains("yellow"))
                {
                    p2co.DataContext = "yellow";
                }
                if (test.Contains("orange"))
                {
                    p2co.DataContext = "orange";
                }
                if (test.Contains("purple"))
                {
                    p2co.DataContext = "purple";
                }

                p2.Text = playernames;

                SharedInformation.savedRoom.player2 = playernames;
                SharedInformation.savedRoom.player2color = test.Substring(1);
                p2.Visibility = Visibility.Visible; p2co.Visibility = Visibility.Visible; pp2.Visibility = Visibility.Visible;
                ortcExample.DoClearLog();
            }
            if (test.Contains("3"))
            {
                if (test.Contains("red"))
                {
                    p3co.DataContext = "red";
                }
                if (test.Contains("blue"))
                {
                    p3co.DataContext = "blue";
                }
                if (test.Contains("green"))
                {
                    p3co.DataContext = "green";
                }
                if (test.Contains("yellow"))
                {
                    p3co.DataContext = "yellow";
                }
                if (test.Contains("orange"))
                {
                    p3co.DataContext = "orange";
                }
                if (test.Contains("purple"))
                {
                    p3co.DataContext = "purple";
                }

                p3.Text = playernames;

                SharedInformation.savedRoom.player3 = playernames;
                SharedInformation.savedRoom.player3color = test.Substring(1);
                p3.Visibility = Visibility.Visible; p3co.Visibility = Visibility.Visible; pp3.Visibility = Visibility.Visible;
                ortcExample.DoClearLog();
            }

            if (test.Contains("4"))
            {
                if (test.Contains("red"))
                {
                    p4co.DataContext = "red";
                }
                if (test.Contains("blue"))
                {
                    p4co.DataContext = "blue";
                }
                if (test.Contains("green"))
                {
                    p4co.DataContext = "green";
                }
                if (test.Contains("yellow"))
                {
                    p4co.DataContext = "yellow";
                }
                if (test.Contains("orange"))
                {
                    p4co.DataContext = "orange";
                }
                if (test.Contains("purple"))
                {
                    p4co.DataContext = "purple";
                }

                p4.Text = playernames;

                SharedInformation.savedRoom.player3 = playernames;
                SharedInformation.savedRoom.player3color = test.Substring(1);
                p4.Visibility = Visibility.Visible; p4co.Visibility = Visibility.Visible; pp4.Visibility = Visibility.Visible;
                ortcExample.DoClearLog();
            }


          

        }

    }
}
