using New_Goes.Common;
using New_Goes.CommonAPI;
using New_Goes.Data;
using New_Goes.Model;
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
    public sealed partial class Directions : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        bool isLoaded = false;

        public Directions()
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
                this.DefaultViewModel["Title"] = "Транспорт";
                Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
                await Task.Run(() => LoadRoutes());
                Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
                isLoaded = true;
            }
        }

        List<DirectionSQL> Buses;
        List<DirectionSQL> Trolls;
        List<DirectionSQL> Tramms;

        private async Task LoadRoutes()
        {
            SQLiteConnection connection = new SQLiteConnection(dbPath);
            Buses = new List<DirectionSQL>();
            Trolls = new List<DirectionSQL>();
            Tramms = new List<DirectionSQL>();
            /*"SELECT s.d_id as d_id, d.name as name, r.number as number,s.r_id as r_id,r.type as type " +
                "FROM stop AS s " +
                "INNER JOIN direction as d ON s.d_id = d.id " +
                "INNER JOIN route as r ON s.r_id = r.id " +
                //"GROUP BY name " +
                "ORDER BY r_id"*/
            var items = connection.Query<DirectionSQL>(
                "SELECT s.d_id as d_id, d.name as name, r.number as number,s.r_id as r_id,r.type as type " +
                "FROM stop AS s " +
                "INNER JOIN direction as d ON s.d_id = d.id " +
                "INNER JOIN route as r ON s.r_id = r.id " +
                "GROUP BY CAST(d.id AS INT)" +
                "ORDER BY r_id");

            foreach (var item in items)
            {
                /*TODO ИСПРАВИТЬ СПИСОК С ТРАНСПОРТОМ!!!!(MUST HAVE)*/
                if (item.type != null)
                {
                    if (item.type == 0)
                    {
                        Buses.Add(new DirectionSQL()
                        {
                            width = currentWidth,
                            d_id = item.d_id,
                            r_id = item.r_id,
                            type = item.type,
                            name = item.name,
                            number = item.number
                        });
                    }
                    else if (item.type == 1)
                    {
                        Trolls.Add(new DirectionSQL()
                        {
                            width = currentWidth,
                            d_id = item.d_id,
                            r_id = item.r_id,
                            type = item.type,
                            name = item.name,
                            number = item.number
                        });
                    }
                    else if (item.type == 2)
                    {
                        Tramms.Add(new DirectionSQL()
                        {
                            width = currentWidth,
                            d_id = item.d_id,
                            r_id = item.r_id,
                            type = item.type,
                            name = item.name,
                            number = item.number
                        });
                    }
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
            if (!Frame.Navigate(typeof(Views.DirectionStops), e.ClickedItem as DirectionSQL))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }
    }
}
