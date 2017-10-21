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

namespace CamShooter2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ListRoom1 : Page
    {
        public ListRoom1()
        {
            this.InitializeComponent();
            List<Room> lstR = new List<Room>();
          
            lstR.Add(new Room { name = "4sim1", imagePath = "Assets/roomi.jpg" });
            lstR.Add(new Room { name = "4sim2", imagePath = "Assets/roomi.jpg" });
            lstR.Add(new Room { name = "4sim3", imagePath = "Assets/roomi.jpg" });
            lstR.Add(new Room { name = "4sim4", imagePath = "Assets/roomi.jpg" });
            lst.DataContext = lstR;
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

        private void lst_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //sharedInformation.sharedSelectedPerson = lstSource[lst.SelectedIndex];
            Frame.Navigate(typeof(JoinRoom));
        }

    }
}
