using New_Goes.Common;
using New_Goes.CommonAPI;
using New_Goes.Data;
using New_Goes.Model;
using Newtonsoft.Json;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkID=391641

namespace New_Goes.Views.Taxi
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class Selector : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        bool isLoaded = false;

        public Selector()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        /// <summary>
        /// Gets the view model for this <see cref="Page"/>.
        /// This can be changed to a strongly typed view model.
        /// </summary>
        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation. Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>.
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!isLoaded)
            {
                this.DefaultViewModel["Title"] = "Такси";
                Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
                await Task.Run(() => LoadRoutes());
                Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
                isLoaded = true;
            }
        }

        private async Task LoadRoutes()
        {
            this.DefaultViewModel["Taxis"] = Constant.CityTaxis;
        }


        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/>.</param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Provides data for navigation methods and event
        /// handlers that cannot cancel the navigation request.</param>
        /// 
        double currentWidth;
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            currentWidth = Window.Current.Bounds.Width;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        #endregion

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (LocalProperties.LoadFromToLP(LocalProperties.LP_active_premium) != "true")
            {
                MessageDialog msg = new MessageDialog(this.resourceLoader.GetString("MessageDialogPremiumContent"), this.resourceLoader.GetString("MessageDialogPremiumTitle"));
                msg.Commands.Add(new UICommand(this.resourceLoader.GetString("StaticButtonCancel"), new UICommandInvokedHandler(CommandHandlers)));
                msg.Commands.Add(new UICommand(this.resourceLoader.GetString("StaticButtonBuy"), new UICommandInvokedHandler(CommandHandlers)));
                msg.ShowAsync();
            }
            else
            {
                if (!Frame.Navigate(typeof(Views.Taxi.Cities.First), JsonConvert.SerializeObject(e.ClickedItem as StaticFBusesData)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
        }

        public async void CommandHandlers(IUICommand commandLabel)
        {
            var Actions = commandLabel.Label;
            if (this.resourceLoader.GetString("StaticButtonBuy") == Actions)
            {
                if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                {
                    ListingInformation listing = await CurrentApp.LoadListingInformationAsync();
                    var superweapon = listing.ProductListings.FirstOrDefault(p => p.Value.ProductId == Constant.IAP_PREMIUN);

                    try
                    {
                        ListingInformation LicensePremiumID = await Windows.ApplicationModel.Store.CurrentApp.LoadListingInformationByProductIdsAsync(new string[] { Constant.IAP_PREMIUN });

                        string x = await CurrentApp.RequestProductPurchaseAsync(LicensePremiumID.ProductListings.ToList()[0].Value.ProductId, false);

                        var productLicenses = CurrentApp.LicenseInformation.ProductLicenses;
                        ProductLicense tokenLicense = productLicenses[Constant.IAP_PREMIUN];

                        if (tokenLicense.IsActive)
                        {
                            LocalProperties.SaveToLP(LocalProperties.LP_active_premium, "true");
                            new MessageDialog("Платный функционал успешно активирован").ShowAsync();
                        }
                        else
                        {
                            new MessageDialog("Не удалось активировать!").ShowAsync();
                        }

                    }
                    catch (Exception ex)
                    {
                        new MessageDialog("Неизвестная ошибка").ShowAsync();
                    }
                }
                else
                {
                    new MessageDialog(resourceLoader.GetString("Error_InternetConnection"), resourceLoader.GetString("Error")).ShowAsync();
                }
            }
            else
            {
                this.resourceLoader.GetString("StaticButtonCancel");
            }
        }
    }
}
