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
    public sealed partial class Stops : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        public Stops()
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
        bool isLoaded = false;
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
             this.DefaultViewModel["Title"] = "Остановки";
        }

        List<StopNameAllSQL> Transport;

        private async Task LoadStopNames()
        {
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            Transport = new List<StopNameAllSQL>();
            var items = connection.Query<StopNameSQL>(
                "SELECT s.n_id as id, REPLACE(sn.name,'ул. ','') as name FROM stop AS s LEFT JOIN stopname as sn ON s.n_id = sn.id GROUP BY s.n_id ORDER BY name");

            List<StopNameSQL> buses = connection.Query<StopNameSQL>(
                "SELECT s.n_id as id, REPLACE(sn.name,'ул. ','') as name,r.type as type FROM stop AS s LEFT JOIN stopname as sn ON s.n_id = sn.id LEFT JOIN route as r ON s.r_id = r.id WHERE r.type= 0 GROUP BY s.n_id ORDER BY name");

            List<StopNameSQL> trolls = connection.Query<StopNameSQL>(
                "SELECT s.n_id as id, REPLACE(sn.name,'ул. ','') as name,r.type as type FROM stop AS s LEFT JOIN stopname as sn ON s.n_id = sn.id LEFT JOIN route as r ON s.r_id = r.id WHERE r.type= 1 GROUP BY s.n_id ORDER BY name");

            List<StopNameSQL> tramms = connection.Query<StopNameSQL>(
                "SELECT s.n_id as id, REPLACE(sn.name,'ул. ','') as name,r.type as type FROM stop AS s LEFT JOIN stopname as sn ON s.n_id = sn.id LEFT JOIN route as r ON s.r_id = r.id WHERE r.type= 2 GROUP BY s.n_id ORDER BY name");

            foreach(var item in items) {
                Transport.Add(new StopNameAllSQL()
                {
                    id = item.id,
                    isBus = buses.Find(x => x.name == item.name) != null ? (LocalProperties.LoadFromToLP(LocalProperties.LP_theme) != LocalProperties.theme_light    ? "/Assets/MenuItemsLogo/ic_bus_white.png"   : "/Assets/MenuItemsLogo/ic_bus_black.png") : null,
                    isTroll = trolls.Find(x => x.name == item.name) != null ? (LocalProperties.LoadFromToLP(LocalProperties.LP_theme) != LocalProperties.theme_light ? "/Assets/MenuItemsLogo/ic_troll_white.png" : "/Assets/MenuItemsLogo/ic_troll_black.png") : null,
                    isTramm = tramms.Find(x => x.name == item.name) != null ? (LocalProperties.LoadFromToLP(LocalProperties.LP_theme) != LocalProperties.theme_light ? "/Assets/MenuItemsLogo/ic_tram_white.png"  : "/Assets/MenuItemsLogo/ic_tram_black.png") : null,
                    name = item.name,
                    width = screenWidth
                });
            }


            this.DefaultViewModel["Transport"] = Transport.Count != 0 ? Transport : null;

            /*await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
                {
                    if (Buses.Count == 0)
                        PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemBus));
                    if (Trolls.Count == 0)
                        PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemTroll));
                    if (Tramms.Count == 0) 
                        PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemTramm));
                }
            );*/
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
        double screenWidth;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (!isLoaded)
            {
                this.navigationHelper.OnNavigatedTo(e);
                screenWidth = Window.Current.Bounds.Width;
                Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
                await Task.Run(() => LoadStopNames());
                Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
                isLoaded = true;
            }           
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
            if (!Frame.Navigate(typeof(Views.StopsTransport), JsonConvert.SerializeObject(e.ClickedItem as StopNameAllSQL)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.DefaultViewModel["Transport"] = Transport.Where(text => text.name.ToUpper().Contains((sender as TextBox).Text.ToUpper()));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(Views.Route)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }
    }
}
