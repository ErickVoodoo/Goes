using New_Goes.Common;
using New_Goes.CommonAPI;
using New_Goes.Data;
using New_Goes.Model;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace New_Goes.Views
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class Route : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        bool isLoaded = false;

        public Route()
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
        /// 
        double width;
        Time time = new Time();
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!isLoaded)
            {
                Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
                this.DefaultViewModel["Title"] = "Route";
                await Task.Run(() => LoadRoute());
                isLoaded = true;
                Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    async () =>
                    {
                        while (true)
                        {
                            await Task.Delay(10000);
                            foreach (var item in Buses)
                            {
                                Buses.Where(d => d.d_id == item.d_id).First().Next_Bus = time.getNextBusTime(item.schedule, item.days);
                            }
                            foreach (var item in Trolls)
                            {
                                Trolls.Where(d => d.d_id == item.d_id).First().Next_Bus = time.getNextBusTime(item.schedule, item.days);
                            }
                            foreach (var item in Tramms)
                            {
                                Tramms.Where(d => d.d_id == item.d_id).First().Next_Bus = time.getNextBusTime(item.schedule, item.days);
                            }
                        }
                    }
                );
            }
        }

        ObservableCollection<DirectionStopSQL> Buses;
        ObservableCollection<DirectionStopSQL> Trolls;
        ObservableCollection<DirectionStopSQL> Tramms;

        ObservableCollection<DirectionStopSQL> Stops;

        private async Task LoadRoute()
        {
            Stops = new ObservableCollection<DirectionStopSQL>();

            Buses = new ObservableCollection<DirectionStopSQL>();
            Trolls = new ObservableCollection<DirectionStopSQL>();
            Tramms = new ObservableCollection<DirectionStopSQL>();

            SQLiteConnection connection = new SQLiteConnection(dbPath);

            var items = connection.Query<DirectionStopSQL>(
                "SELECT s.n_id as n_id, REPLACE(sn.name,'ул. ','') as name FROM stop AS s LEFT JOIN stopname as sn ON s.n_id = sn.id GROUP BY s.n_id ORDER BY name");
            foreach (var item in items)
            {
                Stops.Add(new DirectionStopSQL()
                {
                    n_id = item.n_id,
                    name = item.name
                });
            }
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            width = Window.Current.Bounds.Width;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        #endregion

        private void Firts_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Length > 1)
                {
                    sender.ItemsSource = Stops.Where(text => text.name.ToUpper().Contains(sender.Text.ToUpper()));
                }
                else
                {
                    sender.ItemsSource = new List<String> { };
                }
            }
        }

        private void Second_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                if (sender.Text.Length > 1)
                {
                    sender.ItemsSource = Stops.Where(text => text.name.ToUpper().Contains(sender.Text.ToUpper()));
                }
                else
                {
                    sender.ItemsSource = new List<String> { };
                }
            }
        }

        DirectionStopSQL firstStop;
        DirectionStopSQL secondStop;

        private void First_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            firstStop = args.SelectedItem as DirectionStopSQL;
            sender.Text = firstStop.name;
            sender.IsEnabled = false;
            sender.IsEnabled = true;
        }

        private void Second_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            secondStop = args.SelectedItem as DirectionStopSQL;
            sender.Text = secondStop.name;
            sender.IsEnabled = false;
            sender.IsEnabled = true;
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
            if (LocalProperties.LoadFromToLP(LocalProperties.LP_active_premium) != "true")
            {
                MessageDialog msg = new MessageDialog(this.resourceLoader.GetString("MessageDialogPremiumContent"), this.resourceLoader.GetString("MessageDialogPremiumTitle"));
                msg.Commands.Add(new UICommand(this.resourceLoader.GetString("StaticButtonCancel"), new UICommandInvokedHandler(CommandHandlers)));
                msg.Commands.Add(new UICommand(this.resourceLoader.GetString("StaticButtonBuy"), new UICommandInvokedHandler(CommandHandlers)));
                msg.ShowAsync();
            }
            else
            {
                await Task.Run(() => GetRoutes());
            }
            Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), false);
        }

        private async void Swap_Click(object sender, RoutedEventArgs e)
        {
            if (firstStop != null && secondStop != null)
            {
                DirectionStopSQL tempObj = new DirectionStopSQL();
                tempObj = secondStop;
                secondStop = firstStop;
                firstStop = tempObj;
                FirstAutoSug.Text = firstStop.name;
                SecondAutoSug.Text = secondStop.name;
            }
        }

        public async void CommandHandlers(IUICommand commandLabel)
        {
            var Actions = commandLabel.Label;
            if(this.resourceLoader.GetString("StaticButtonBuy") == Actions){
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
            } else {
                this.resourceLoader.GetString("StaticButtonCancel");
            }
        }

        private async void GetRoutes()
        {
            if (secondStop != null && firstStop != null)
            {
                SQLiteConnection connection = new SQLiteConnection(dbPath);

                Buses = new ObservableCollection<DirectionStopSQL>();
                Trolls = new ObservableCollection<DirectionStopSQL>();
                Tramms = new ObservableCollection<DirectionStopSQL>();

                List<DirectionStopSQL> firstRoutes = connection.Query<DirectionStopSQL>(
                    "SELECT s.r_id as r_id, s.n_id as n_id, s.d_id as d_id,d.name as name, r.number as number, s.schedule as schedule, s.days as days, r.type as type " +
                    "FROM stop AS s " +
                    "LEFT JOIN direction as d ON d_id = d.id " +
                    "LEFT JOIN route as r ON r_id = r.id " +
                    "WHERE n_id=" + firstStop.n_id);

                List<DirectionStopSQL> secondRoutes = connection.Query<DirectionStopSQL>(
                    "SELECT s.r_id as r_id, s.n_id as n_id, s.d_id as d_id,d.name as name, r.number as number, s.schedule as schedule, s.days as days, r.type as type " +
                    "FROM stop AS s " +
                    "LEFT JOIN direction as d ON d_id = d.id " +
                    "LEFT JOIN route as r ON r_id = r.id " +
                    "WHERE n_id=" + secondStop.n_id);

                List<DirectionStopSQL> SecondBus = new List<DirectionStopSQL>();
                List<DirectionStopSQL> SecondTroll = new List<DirectionStopSQL>();
                List<DirectionStopSQL> SecondTramm = new List<DirectionStopSQL>();

                foreach (var item in secondRoutes)
                {
                    if (item.type == 0)
                    {
                        SecondBus.Add(new DirectionStopSQL()
                        {
                            width = width,
                            name = item.name,
                            r_id = item.r_id,
                            number = item.number,
                            type = item.type,
                            d_id = item.d_id,
                            n_id = item.n_id,
                            days = item.days,
                            schedule = item.schedule,
                            next_bus = time.getNextBusTime(item.schedule, item.days),
                        });
                    }
                    else if (item.type == 1)
                    {
                        SecondTroll.Add(new DirectionStopSQL()
                        {
                            width = width,
                            name = item.name,
                            r_id = item.r_id,
                            number = item.number,
                            d_id = item.d_id,
                            type = item.type,
                            n_id = item.n_id,
                            days = item.days,
                            schedule = item.schedule,
                            next_bus = time.getNextBusTime(item.schedule, item.days),
                        });
                    }
                    else if (item.type == 2)
                    {
                        SecondTramm.Add(new DirectionStopSQL()
                        {
                            width = width,
                            name = item.name,
                            r_id = item.r_id,
                            number = item.number,
                            d_id = item.d_id,
                            type = item.type,
                            n_id = item.n_id,
                            days = item.days,
                            schedule = item.schedule,
                            next_bus = time.getNextBusTime(item.schedule, item.days),
                        });
                    }
                }

                foreach (var bus in SecondBus)
                {
                    List<DirectionStopSQL> firstItem = connection.Query<DirectionStopSQL>(
                        "SELECT s.r_id as r_id, s.n_id as n_id, s.d_id as d_id, REPLACE(sn.name,'ул. ','') as name, s.schedule as schedule, r.number as number, s.days as days " +
                        "FROM stop AS s " +
                        "LEFT JOIN route as r ON r_id = r.id " +
                        "LEFT JOIN stopname as sn ON s.n_id = sn.id " +
                        "WHERE s.d_id=" + bus.d_id + " AND s.r_id=" + bus.r_id + " " +
                        "GROUP BY name " +
                        "ORDER BY CAST(s.id AS INT)");

                    DirectionStopSQL search_first = firstItem.Where(item => item.name.ToUpper() == firstStop.name.ToUpper()).FirstOrDefault();
                    DirectionStopSQL search_second = firstItem.Where(item => item.name.ToUpper() == secondStop.name.ToUpper()).FirstOrDefault();

                    if (search_first != null
                        && search_second != null)
                    {
                        int first_index = firstItem.IndexOf(search_first);
                        int second_index = firstItem.IndexOf(search_second);

                        if (second_index > first_index)
                        {
                            DirectionStopSQL stop = firstRoutes.Where(item => item.d_id == bus.d_id && item.r_id == bus.r_id && item.type == 0).FirstOrDefault();
                            stop.stop_count = this.resourceLoader.GetString("String_StopCount") + ": " + (second_index - first_index).ToString();
                            stop.next_bus = time.getNextBusTime(stop.schedule, stop.days);
                            stop.width = width;
                            Buses.Add(stop);
                        }
                    }
                }

                foreach (var troll in SecondTroll)
                {
                    List<DirectionStopSQL> firstItem = connection.Query<DirectionStopSQL>(
                        "SELECT s.r_id as r_id, s.n_id as n_id, s.d_id as d_id, REPLACE(sn.name,'ул. ','') as name, s.schedule as schedule, r.number as number, s.days as days " +
                        "FROM stop AS s " +
                        "LEFT JOIN route as r ON r_id = r.id " +
                        "LEFT JOIN stopname as sn ON s.n_id = sn.id " +
                        "WHERE s.d_id=" + troll.d_id + " AND s.r_id=" + troll.r_id + " " +
                        "GROUP BY name " +
                        "ORDER BY CAST(s.id AS INT)");

                    DirectionStopSQL search_first = firstItem.Where(item => item.name.ToUpper() == firstStop.name.ToUpper()).FirstOrDefault();
                    DirectionStopSQL search_second = firstItem.Where(item => item.name.ToUpper() == secondStop.name.ToUpper()).FirstOrDefault();

                    if (search_first != null
                        && search_second != null)
                    {
                        int first_index = firstItem.IndexOf(search_first);
                        int second_index = firstItem.IndexOf(search_second);

                        if (second_index > first_index)
                        {
                            DirectionStopSQL stop = firstRoutes.Where(item => item.d_id == troll.d_id && item.r_id == troll.r_id && item.type == 1).FirstOrDefault();
                            stop.stop_count = this.resourceLoader.GetString("String_StopCount") + ": " + (second_index - first_index).ToString();
                            stop.next_bus = time.getNextBusTime(stop.schedule, stop.days);
                            stop.width = width;
                            Trolls.Add(stop);
                        }
                    }
                }

                foreach (var tramm in SecondTramm)
                {
                    List<DirectionStopSQL> firstItem = connection.Query<DirectionStopSQL>(
                        "SELECT s.r_id as r_id, s.n_id as n_id, s.d_id as d_id, REPLACE(sn.name,'ул. ','') as name, s.schedule as schedule, r.number as number, s.days as days " +
                        "FROM stop AS s " +
                        "LEFT JOIN route as r ON r_id = r.id " +
                        "LEFT JOIN stopname as sn ON s.n_id = sn.id " +
                        "WHERE s.d_id=" + tramm.d_id + " AND s.r_id=" + tramm.r_id + " " +
                        "GROUP BY name " +
                        "ORDER BY CAST(s.id AS INT)");

                    DirectionStopSQL search_first = firstItem.Where(item => item.name.ToUpper() == firstStop.name.ToUpper()).FirstOrDefault();
                    DirectionStopSQL search_second = firstItem.Where(item => item.name.ToUpper() == secondStop.name.ToUpper()).FirstOrDefault();

                    if (search_first != null
                        && search_second != null)
                    {
                        int first_index = firstItem.IndexOf(search_first);
                        int second_index = firstItem.IndexOf(search_second);

                        if (second_index > first_index)
                        {
                            DirectionStopSQL stop = firstRoutes.Where(item => item.d_id == tramm.d_id && item.r_id == tramm.r_id && item.type == 2).FirstOrDefault();
                            stop.stop_count = this.resourceLoader.GetString("String_StopCount") + ": " + (second_index - first_index).ToString();
                            stop.next_bus = time.getNextBusTime(stop.schedule, stop.days);
                            stop.width = width;
                            Tramms.Add(stop);
                        }
                    }
                }

                this.DefaultViewModel["Buses"] = Buses.OrderBy(item => item.r_id);
                this.DefaultViewModel["Trolls"] = Trolls.OrderBy(item => item.r_id);
                this.DefaultViewModel["Tramms"] = Tramms.OrderBy(item => item.r_id);
            }
        }
    }
}
