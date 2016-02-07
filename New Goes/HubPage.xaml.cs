using New_Goes.Common;
using New_Goes.CommonAPI;
using New_Goes.Data;
using New_Goes.DataModel;
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
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Hub Application template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace New_Goes
{
    /// <summary>
    /// A page that displays a grouped collection of items.
    /// </summary>
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            this.InitializeComponent();
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

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
        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if (!isLoaded)
            {
                TemplateSource menu = new TemplateSource();
                var items_menu = await menu.getMenuItems();
                this.DefaultViewModel["MenuItems"] = items_menu;
                isLoaded = true;
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            // TODO: Save the unique state of the page here.
        }

        /// <summary>
        /// Shows the details of a clicked group in the <see cref="SectionPage"/>.
        /// </summary>
        private async void MenuItem_Click(object sender, ItemClickEventArgs e)
        {
            var position = ((MenuItem)e.ClickedItem).position;
            if (Int32.Parse(position) == 0)
            {
                if (!Frame.Navigate(typeof(Views.Directions)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            else if(Int32.Parse(position) == 1)
            {
                if (!Frame.Navigate(typeof(Views.Stops)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            else if (Int32.Parse(position) == 2)
            {
                if (!Frame.Navigate(typeof(Views.Favorite)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            else if (Int32.Parse(position) == 3)
            {
                if (!Frame.Navigate(typeof(Views.Fast_Buses.Selector)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
            else if (Int32.Parse(position) == 4)
            {
                if (!Frame.Navigate(typeof(Views.Taxi.Selector)))
                {
                    throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
                }
            }
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
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
            LoadSettings();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
            if (e.NavigationMode == NavigationMode.Back)
                NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        #endregion

        private async void Update_Schedule(object sender, RoutedEventArgs e)
        {
            Constant.Loader(this.resourceLoader.GetString("GlobalLoading"), true);
            Status status = await Task.Run(() => updateSchedule());
            Constant.Loader(this.resourceLoader.GetString("GlobalLoadingSuccess"), false);
            await new MessageDialog(status.reason).ShowAsync();
        }

        private async Task<Status> updateSchedule()
        {
            Database.DropDatabase();
            Schedule scedule = new Schedule();
            return await scedule.GetSchedule(LocalProperties.LoadFromToLP(LocalProperties.LP_selected_city));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(Loading)))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        bool isLoadedSettings = false;
        bool isLoaded = false;
        private async void LoadSettings()
        {
            if (!isLoadedSettings)
            {
                Toggle_Theme.IsOn = LocalProperties.LoadFromToLP(LocalProperties.LP_theme) == LocalProperties.theme_light;
                Settings_Latest_Database.Text = LocalProperties.LoadFromToLP(LocalProperties.LP_current_version);

                New_Goes.CommonAPI.Metadata.StatusMetaData metadata = await Metadata.Get_MetaData();
                if (metadata.obj != null)
                {
                    string city = LocalProperties.LoadFromToLP(LocalProperties.LP_selected_city);
                    int version = 0;

                    switch (city)
                    {
                        case "brest":
                            version = metadata.obj.cities_versions.brest;
                            break;
                        case "vitebsk":
                            version = metadata.obj.cities_versions.vitebsk;
                            break;
                        case "grodno":
                            version = metadata.obj.cities_versions.grodno;
                            break;
                        case "gomel":
                            version = metadata.obj.cities_versions.gomel;
                            break;
                        case "mogilev":
                            version = metadata.obj.cities_versions.mogilev;
                            break;
                        case "minsk":
                            version = metadata.obj.cities_versions.minsk;
                            break;
                    }
                    Settings_Newest_Database.Text = version.ToString();
                    if (version > Int32.Parse(LocalProperties.LoadFromToLP(LocalProperties.LP_current_version)))
                        Constant.ShowNotification("Доступно обновление расписания");
                }
            }
            
            isLoadedSettings = true;
        }

        private void ToggleSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if(isLoadedSettings) {
                TextBlock_ReloadApp.Visibility = Visibility.Visible;
                TextBlock_ReloadApp.Text = this.resourceLoader.GetString("Reload_Application");
                if (LocalProperties.LoadFromToLP(LocalProperties.LP_theme) == LocalProperties.theme_light)
                    LocalProperties.SaveToLP(LocalProperties.LP_theme, LocalProperties.theme_dark);
                else
                    LocalProperties.SaveToLP(LocalProperties.LP_theme, LocalProperties.theme_light);
            }
        }
    }
}
