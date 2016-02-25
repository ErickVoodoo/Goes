﻿using New_Goes.Common;
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
    public sealed partial class StopsTransport : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        bool isLoaded = false;

        public StopsTransport()
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
        StopNameAllSQL param;
        Time time = new Time();
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!isLoaded)
            {
                Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
                param = JsonConvert.DeserializeObject<StopNameAllSQL>(e.NavigationParameter.ToString());
                this.DefaultViewModel["Title"] = param.name; 
                this.DefaultViewModel["Favorite"] = Database.IfAllStopsAreFavorite(Int32.Parse(param.id)) ? Constant.FavoriteStar : Constant.UnFavoriteStar;
                await Task.Run(() => LoadRoutes(param));
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

        private async Task LoadRoutes(StopNameAllSQL param)
        {
            Time time = new Time();
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            Buses = new ObservableCollection<DirectionStopSQL>();
            Trolls = new ObservableCollection<DirectionStopSQL>();
            Tramms = new ObservableCollection<DirectionStopSQL>();
            var items = connection.Query<DirectionStopSQL>(
                "SELECT s.r_id as r_id, s.n_id as n_id, s.d_id as d_id,s.favorite as favorite,d.name as name, r.number as number, s.schedule as schedule, s.days as days, r.type as type " +
                "FROM stop AS s " +
                "LEFT JOIN direction as d ON d_id = d.id " +
                "LEFT JOIN route as r ON r_id = r.id " + 
                "WHERE n_id=" + param.id + " " +
                "GROUP BY name,r_id " +
                "ORDER BY r_id");

            foreach (var item in items)
            {
                if (item.type == 0)
                {
                    Buses.Add(new DirectionStopSQL()
                    {
                        width = param.width,
                        name = item.name,
                        r_id = item.r_id,
                        number = item.number,
                        type = item.type,
                        d_id = item.d_id,
                        n_id = item.n_id,
                        days = item.days,
                        schedule = item.schedule,
                        next_bus = time.getNextBusTime(item.schedule, item.days),
                        favorite = Int32.Parse(item.favorite) == 1 ? Constant.FavoriteStar : Constant.UnFavoriteStar
                    });
                }
                else if (item.type == 1)
                {
                    Trolls.Add(new DirectionStopSQL()
                    {
                        width = param.width,
                        name = item.name,
                        r_id = item.r_id,
                        number = item.number,
                        d_id = item.d_id,
                        type = item.type,
                        n_id = item.n_id,
                        days = item.days,
                        schedule = item.schedule,
                        next_bus = time.getNextBusTime(item.schedule, item.days),
                        favorite = Int32.Parse(item.favorite) == 1 ? Constant.FavoriteStar : Constant.UnFavoriteStar
                    });
                }
                else if (item.type == 2)
                {
                    Tramms.Add(new DirectionStopSQL()
                    {
                        width = param.width,
                        name = item.name,
                        r_id = item.r_id,
                        number = item.number,
                        d_id = item.d_id,
                        type = item.type,
                        n_id = item.n_id,
                        days = item.days,
                        schedule = item.schedule,
                        next_bus = time.getNextBusTime(item.schedule, item.days),
                        favorite = Int32.Parse(item.favorite) == 1 ? Constant.FavoriteStar : Constant.UnFavoriteStar
                    });
                }
            }

            this.DefaultViewModel["Buses"] = Buses.Count != 0 ? Buses : null;
            this.DefaultViewModel["Trolls"] = Trolls.Count != 0 ? Trolls : null;
            this.DefaultViewModel["Tramms"] = Tramms.Count != 0 ? Tramms : null;

            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                if (Buses.Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemBus));
                if (Trolls.Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemTroll));
                if (Tramms.Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemTramm));
            }
            );
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
                type = paramItem.type,
                width = param.width,
                days = paramItem.days,
                schedule = paramItem.schedule,
                number = paramItem.number,
                d_name = paramItem.name,
                s_name = param.name,
                d_id = paramItem.d_id,
                r_id = paramItem.r_id,
                n_id = paramItem.n_id,
                next_bus = paramItem.next_bus,
            };
            if (!Frame.Navigate(typeof(Views.MainSchedule), JsonConvert.SerializeObject(mainSchedule)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DirectionStopSQL model = (((((sender as Button).Parent as Border).Parent as StackPanel).Parent as Grid).Parent as StackPanel).DataContext as DirectionStopSQL;
            if (model.type == 0)
            {
                Buses.Where(d => d.d_id == model.d_id).First().Favorite = (model.favorite == Constant.FavoriteStar) ? Constant.UnFavoriteStar : Constant.FavoriteStar;
            }
            else if (model.type == 1)
            {
                Trolls.Where(d => d.d_id == model.d_id).First().Favorite = (model.favorite == Constant.FavoriteStar) ? Constant.UnFavoriteStar : Constant.FavoriteStar;
            }
            else if (model.type == 2)
            {
                Tramms.Where(d => d.d_id == model.d_id).First().Favorite = (model.favorite == Constant.FavoriteStar) ? Constant.UnFavoriteStar : Constant.FavoriteStar;
            }
           Database.AddOrRemoveFromFavorite(model.n_id, model.r_id, model.d_id);
        }

        private void AddAllStops_Button(object sender, RoutedEventArgs e)
        {
            this.DefaultViewModel["Favorite"] = MainAddButton.Symbol.ToString() == Constant.UnFavoriteStar ? Constant.FavoriteStar : Constant.UnFavoriteStar;
            if(Buses.Count != 0) {
                foreach(var bus in Buses) {
                    bus.Favorite = this.DefaultViewModel["Favorite"].ToString();
                }
            }
            if (Trolls.Count != 0)
            {
                foreach (var troll in Trolls)
                {
                    troll.Favorite = this.DefaultViewModel["Favorite"].ToString();
                }
            }
            if(Tramms.Count != 0) {
                foreach (var tramm in Tramms)
                {
                    tramm.Favorite = this.DefaultViewModel["Favorite"].ToString();
                }
            }
            Database.AddOrRemoveFromFavoriteWholeStop(Int32.Parse(param.id));
        }
    }
}
