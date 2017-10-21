using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Text;
using Windows.UI.Popups;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace CamShooter2
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Shield : Page
    { 
        public string receipt;
        public Shield()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }

         private async void shield1_Click(object sender, RoutedEventArgs e)
         {

           // var result = await CurrentApp.GetProductReceiptAsync("shield");
                try {
                    //await Windows.System.Launcher.LaunchUriAsync(
    //new Uri("ms-windows-store:reviewapp?appid=B5EB66C4-C37A-4DAF-9280-CA85810D0C52"));
                    await CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(
                              CoreDispatcherPriority.Normal,
                              async () =>
                              {
                                  try {
                                    PurchaseResults result =  await  CurrentApp.RequestProductPurchaseAsync("shield");

                                   //  var result = await CurrentApp.LoadListingInformationAsync();
                                      var hasPaid = CurrentApp.LicenseInformation.ProductLicenses[("shield")].IsActive;
                                     switch (result.Status)
                                       {
                                           case ProductPurchaseStatus.Succeeded:
                                               await CurrentApp.ReportConsumableFulfillmentAsync("shield", result.TransactionId);
                                           //  OnPurchaseDismissed("1");
                                           break;
                                           case ProductPurchaseStatus.NotFulfilled:
                                           //   InAppPurchaseManager.Instance.CheckUnfulfilledConsumables();
                                           await CurrentApp.ReportConsumableFulfillmentAsync("shield", result.TransactionId);
                                           //   OnPurchaseDismissed("1");
                                           break;
                                       }

                }
                  catch (Exception ex)
                  {
                      String message = ex.ToString();
                      var dialog1 = new MessageDialog(message);
                      await dialog1.ShowAsync();  }
              });
     }
    catch (Exception ex)
    {
    ex.ToString();
    }
    
        }
        private async Task<bool> GetLicenseStatus(string id)
        {
            try
            {
                var result = await CurrentApp.GetProductReceiptAsync(id);
            }

            catch //user didnt buy
            {
                return false;
            }

            return true;
        }

        private async void shield2_Click(object sender, RoutedEventArgs e)
        {
            var listing = await CurrentAppSimulator.LoadListingInformationAsync();
            listing.ToString();
           // LicenseInformation listing = await CurrentApp.LoadListingInformationAsync();
            var superweapon =
                listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == "1");
             String message1 = superweapon.Value.Name;
            // var dialog = new MessageDialog(listing.ToString());
             //await dialog.ShowAsync();
           // CurrentApp.AppId.ToString();
            try
            {

                if (CurrentAppSimulator.LicenseInformation.ProductLicenses["1"].IsActive)
                {
                    PurchaseResults result = await CurrentAppSimulator.RequestProductPurchaseAsync("1");

                    var dialog1 = new MessageDialog("you buy super shield");
                    await dialog1.ShowAsync();

                }
                else
                {
                    try {

                       //  var receipt = await CurrentApp.RequestProductPurchaseAsync("shield", false);
                            PurchaseResults receipt = await CurrentAppSimulator.RequestProductPurchaseAsync("shield");
                            message.Visibility = Windows.UI.Xaml.Visibility.Visible;
                            message.Text = CurrentApp.AppId.ToString();
                           
                            var dialo2 = new MessageDialog(message.Text);
                            await dialo2.ShowAsync();
                            
                       
                    }
                    catch (Exception ex)
                    {

                       String message = ex.ToString();
                      //  var dialog3 = new MessageDialog(message);
                      //  await dialog3.ShowAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                
                String message = ex.ToString();
               // var dialog3 = new MessageDialog(message);
              //  await dialog3.ShowAsync();
            }
            //MessageBox.Show(sb.ToString(), "List all products", MessageBoxButton.OK);
            

        }
    
    }
}
