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
    public sealed partial class MainSchedule : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        string dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, "goes.db");

        public MainSchedule()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
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
        ScheduleSQL param;
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            Time time = new Time();
            Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
            param = JsonConvert.DeserializeObject<ScheduleSQL>(e.NavigationParameter.ToString());
            this.DefaultViewModel["Direction"] = param.d_name;
            this.DefaultViewModel["Number"] = param.number;
            this.DefaultViewModel["Stop"] = param.s_name;
            this.DefaultViewModel["NextBus"] = param.next_bus;
            this.DefaultViewModel["BorderColor"] = Constant.TransportColors[param.type];
            this.DefaultViewModel["Favorite"] = param.favorite ? Constant.FavoriteStar : Constant.UnFavoriteStar;
            await Task.Run(() => LoadRoutes(param));
            Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);

            await Task.Run(async () =>
            {
                while (true)
                {
                    await Task.Delay(10000);
                    this.DefaultViewModel["NextBus"] = time.getNextBusTime(param.schedule, param.days);
                }
            });
        }
        int selectedPivot;

        private async Task LoadRoutes(ScheduleSQL param)
        {
            Time time = new Time();
            List<List<New_Goes.CommonAPI.Time.TimeView>> list = time.getScheduleList(param.width, param.days, param.schedule);
            Debug.WriteLine("Count");
            await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
            () =>
            {
                this.DefaultViewModel["Monday"] = list[0];
                this.DefaultViewModel["Tuesday"] = list[1];
                this.DefaultViewModel["Wednesday"] = list[2];
                this.DefaultViewModel["Thursday"] = list[3];
                this.DefaultViewModel["Friday"] = list[4];
                this.DefaultViewModel["Saturday"] = list[5];
                this.DefaultViewModel["Sunday"] = list[6];

                selectedPivot = Time.getCurrentDaySchedule(param.schedule, param.days);

                if (list[0].Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemMonday));
                if (list[1].Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemTuesday));
                if (list[2].Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemWednesday));
                if (list[3].Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemThursday));
                if (list[4].Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemFriday));
                if (list[5].Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemSaturday));
                if (list[6].Count == 0)
                    PivotMain.Items.RemoveAt(PivotMain.Items.IndexOf(PItemSunday));
                PivotMain.SelectedIndex = selectedPivot;
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
        }

        #endregion

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Database.AddOrRemoveFromFavorite(param.n_id, param.r_id, param.d_id);
            this.DefaultViewModel["Favorite"] = this.DefaultViewModel["Favorite"] == Constant.FavoriteStar ? Constant.UnFavoriteStar : Constant.FavoriteStar;
        }

        private void List_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var tt = (((PivotMain.SelectedItem as PivotItem).Content as ListView).ItemsSource as List<New_Goes.CommonAPI.Time.TimeView>).Where(item => item.hour == DateTime.Now.Hour);
            if (selectedPivot == Int16.Parse((sender as ListView).Tag.ToString()) && tt.Count() != 0)
                ((PivotMain.SelectedItem as PivotItem).Content as ListView).ScrollIntoView((((PivotMain.SelectedItem as PivotItem).Content as ListView).ItemsSource as List<New_Goes.CommonAPI.Time.TimeView>).Where(item => item.hour == DateTime.Now.Hour).First(), ScrollIntoViewAlignment.Leading);
        }
    }
}
