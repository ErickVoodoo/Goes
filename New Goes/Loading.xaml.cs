using New_Goes.Common;
using New_Goes.CommonAPI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace New_Goes
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Loading : Page
    {
        private NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private ObservableDictionary defaultViewModel = new ObservableDictionary();

        public Loading()
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
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.defaultViewModel["Cities"] = Constant.City;
            cities = Constant.City;
            /*if (LocalProperties.LoadFromToLP(LocalProperties.LP_first_message) == null)
            {
                LocalProperties.SaveToLP(LocalProperties.LP_first_message, "true");
                new MessageDialog("Данное приложение было полностью сделано с 0 в связи с утерей файлов предыдущего проекта. Если Вы устанавливали прежнюю версию данного приложения - Вам придется заново загрузить расписание города и выбрать Ваши избранные остановки, приношу свои извинения за данное неудобство. Спасибо.","Внимание").ShowAsync();
            }*/
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        StaticData[] cities;
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
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
        StaticData selected;

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            LoadSchedule.IsEnabled = true;
            Button button = sender as Button;
            selected = ((sender as Button).Parent as Grid).DataContext as StaticData;
            for (int i = 0; i < Constant.City.Length; i++)
            {
                if (cities[i].key == selected.key)
                {
                    cities[i].background = "LightGray";
                }
                else
                {
                    cities[i].background = "Transparent";
                }
            }
            Cities.ItemsSource = null;
            Cities.ItemsSource = cities;
        }

        private async void LoadSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (!Constant.checkNetworkConnection())
            {
                ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
                await new MessageDialog(resourceLoader.GetString("Error_InternetConnection"), resourceLoader.GetString("Error")).ShowAsync();
                return;
            }
            Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
            LoadSchedule.IsEnabled = false;
            Cities.IsEnabled = false;
            Database.CreateTables(); 
            Database.DropDatabase();
            Status status = await Task.Run(() => loadSchedule());
            if (status.isSuccess)
            {
                Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
                LocalProperties.SaveToLP(LocalProperties.LP_selected_city, selected.key);
                if (!Frame.Navigate(typeof(HubPage), "true"))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            else if (status.reason != null)
            {
                new MessageDialog(status.reason).ShowAsync();
            }
        }

        public async Task<Status> loadSchedule() {
            Schedule schedule = new Schedule();
            return await schedule.GetSchedule(selected.key);
        }
    }
}
