using New_Goes.Common;
using New_Goes.CommonAPI;
using New_Goes.Data;
using New_Goes.Model;
using Newtonsoft.Json;
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
    public sealed partial class DirectionStops : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        bool isLoaded = false;

        public DirectionStops()
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
        DirectionSQL param;
        Time time = new Time();
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!isLoaded)
            {
                Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
                param = JsonConvert.DeserializeObject<DirectionSQL>(e.NavigationParameter.ToString());
                this.DefaultViewModel["Number"] = param.number;
                this.DefaultViewModel["Direction"] = param.name;
                this.DefaultViewModel["BorderColor"] = Constant.TransportColors[param.type];
                await Task.Run(() => LoadRoutes(param));
                Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
                isLoaded = true;

                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                    async () =>
                    {
                        while (true)
                        {
                            await Task.Delay(10000);
                            foreach (var item in Stops)
                            {
                                Stops.Where(d => d.n_id == item.n_id).First().Next_Bus = time.getNextBusTime(item.schedule, item.days);
                            }
                        }
                    }
                );
            }
        }

        ObservableCollection<DirectionStopSQL> Stops;

        private async Task LoadRoutes(DirectionSQL param)
        {
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            Stops = new ObservableCollection<DirectionStopSQL>();
            var items = connection.Query<DirectionStopSQL>(
                "SELECT s.id as s_id, s.r_id as r_id, s.n_id as n_id, s.d_id as d_id,s.favorite as favorite, REPLACE(sn.name,'ул. ','') as name, s.schedule as schedule, s.days as days " +
                "FROM stop AS s " +
                "LEFT JOIN stopname as sn ON n_id = sn.id " + 
                "WHERE d_id=" + param.d_id + " AND r_id=" + param.r_id + " " +
                "GROUP BY name " +
                "ORDER BY s_id");

            foreach (var item in items)
            {
                Stops.Add(new DirectionStopSQL()
                    {
                        width = param.width,
                        name = item.name,
                        r_id = item.r_id,
                        d_id = item.d_id,
                        n_id = item.n_id,
                        days = item.days,
                        next_bus = time.getNextBusTime(item.schedule, item.days),
                        favorite = Int32.Parse(item.favorite) == 1 ? Constant.FavoriteStar : Constant.UnFavoriteStar,
                        schedule = item.schedule
                    });
            }

            this.DefaultViewModel["Stops"] = Stops.Count != 0 ? Stops : null;
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
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
            DirectionStopSQL paramItem = e.ClickedItem as DirectionStopSQL;
            ScheduleSQL mainSchedule = new ScheduleSQL()
            {
                favorite = (paramItem.favorite == Constant.FavoriteStar) ? true : false,
                type = param.type,
                width = param.width,
                days = paramItem.days,
                schedule = paramItem.schedule,
                number = param.number,
                d_name = param.name,
                s_name = paramItem.name,
                d_id = paramItem.d_id,
                r_id = paramItem.r_id,
                next_bus = paramItem.next_bus,
                n_id = paramItem.n_id
            };
            if (!Frame.Navigate(typeof(Views.MainSchedule), JsonConvert.SerializeObject(mainSchedule)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DirectionStopSQL model = (((sender as Button).Parent as Border).Parent as Grid).DataContext as DirectionStopSQL;
            Stops.Where(d => d.n_id == model.n_id).First().Favorite = (model.favorite == Constant.FavoriteStar) ? Constant.UnFavoriteStar : Constant.FavoriteStar;
            Database.AddOrRemoveFromFavorite(model.n_id, model.r_id, model.d_id);
        }
    }
}
