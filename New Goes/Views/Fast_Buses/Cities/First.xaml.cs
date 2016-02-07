using New_Goes.Common;
using New_Goes.CommonAPI;
using New_Goes.Data;
using New_Goes.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel.Resources;
using Windows.Data.Json;
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

namespace New_Goes.Views.Fast_Buses.Cities
{
    /// <summary>
    /// A page that displays details for a single item within a group.
    /// </summary>
    public sealed partial class First : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        bool isLoaded = false;

        public First()
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
        StaticFBusesData param;
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!isLoaded)
            {
                param = e.NavigationParameter as StaticFBusesData;
                this.DefaultViewModel["Title"] = this.resourceLoader.GetString("TitleFBus") + "(" + param.title + ")  ";
                this.DefaultViewModel["City"] = param.title;
                Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
                await Task.Run(() => LoadRoutes());
                Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
                isLoaded = true;
            }
        }

        List<FBusList> fbuslist;

        private async Task LoadRoutes()
        {
            fbuslist = new List<FBusList>();

            string FBus = Database.GetFBusJSON(param.key);
            string result = "";
            if (FBus == null)
            {
                var data = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("secret", Constant.PREMIUM_API_KEY),
                    new KeyValuePair<string, string>("city", param.key),
                };
                string res = Constant.GetJsonFromURI(param.value, data).Result;
                JObject obj = JObject.Parse(res);
                result = obj["message"].ToString();
            }
            else
            {
                result = FBus;
            }


            FBus BUS = JsonConvert.DeserializeObject<FBus>(result);

            if (FBus == null)
            {
                Database.AddFbus(param.key, result);
            }

            foreach (ScheduleObj schedule in BUS.schedule)
            {
                fbuslist.Add(new FBusList()
                {
                    direction = BUS.title.direction,
                    interval = BUS.title.interval,
                    number = BUS.title.number,
                    alternative = BUS.title.alternative,
                    time_available = BUS.title.time_available,
                    name = BUS.title.name,

                    direction_schedule = schedule.direction,
                    interval_schedule = schedule.interval,
                    number_schedule = schedule.number,
                    alternative_schedule = schedule.alternative,
                    time_available_schedule = schedule.time_available,
                    name_schedule = schedule.name,
                    width = currentWidth,

                    isNameVisible = schedule.name == null || schedule.name == " " ? "Collapsed" : "Visible",
                    isAltVisible = schedule.alternative == null || schedule.alternative == " " ? "Collapsed" : "Visible",
                    isTimeAvVisible = schedule.time_available == null || schedule.time_available == " " ? "Collapsed" : "Visible"
                });
            }

            this.DefaultViewModel["FBuses"] = fbuslist;
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

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DefaultViewModel["FBuses"] = fbuslist.Where(text => (text.number_schedule.ToUpper().Contains((sender as TextBox).Text.ToUpper())) || (text.direction_schedule.ToUpper().Contains((sender as TextBox).Text.ToUpper())));
        }
    }
}
