using MediaAppSample.Core.Commands;
using MediaAppSample.Core.Models;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace MediaAppSample.Core.ViewModels
{
    public partial class GeneralSettingsViewModel : ViewModelBase
    {
        #region Properties

        private string _BackgroundTasksStatus;
        public string BackgroundTasksStatus
        {
            get { return _BackgroundTasksStatus; }
            private set { this.SetProperty(ref _BackgroundTasksStatus, value); }
        }
        
        private CommandBase _ClearAppDataCacheCommand = null;
        public CommandBase ClearAppDataCacheCommand
        {
            get { return _ClearAppDataCacheCommand ?? (_ClearAppDataCacheCommand = new NavigationCommand("ClearAppDataCacheCommand", async () => await this.ClearAppDataCacheAsync())); }
        }
        
        private NotifyTaskCompletion<string> _AppCacheTask;
        public NotifyTaskCompletion<string> AppCacheTask
        {
            get { return _AppCacheTask; }
            private set { this.SetProperty(ref _AppCacheTask, value); }
        }
        
        #endregion

        #region Constructors

        public GeneralSettingsViewModel()
        {
            this.Title = Strings.Resources.TextTitleGeneral;

            if (DesignMode.DesignModeEnabled)
                return;
        }

        #endregion

        #region Methods

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e, bool isFirstRun)
        {
            if(this.View != null)
                this.View.GotFocus += View_GotFocus;

            this.AppCacheTask = new NotifyTaskCompletion<string>(Platform.Current.Storage.GetAppDataCacheFolderSizeAsync());

            await base.OnLoadStateAsync(e, isFirstRun);
        }

        public override void OnApplicationResuming()
        {
            this.View_GotFocus(null, null);
            base.OnApplicationResuming();
        }

        private void View_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var t = this.UpdateBackgroundTasksStatus();
        }

        private async Task UpdateBackgroundTasksStatus()
        {
            try
            {
                var allowed = Platform.Current.BackgroundTasks.CheckIfAllowed();

                this.BackgroundTasksStatus = !allowed ? Strings.BackgroundTasks.TextBackgroundAppDisabledStatus : string.Empty;

                if (!Platform.Current.BackgroundTasks.AreTasksRegistered && allowed)
                    await Platform.Current.BackgroundTasks.RegisterAllAsync();
            }
            catch(Exception ex)
            {
                Platform.Current.Logger.LogError(ex, "Error during UpdateBackgroundTasksStatus()");
            }
        }

        protected override Task OnSaveStateAsync(SaveStateEventArgs e)
        {
            Platform.Current.SaveSettings();
            if (this.View != null)
                this.View.GotFocus -= View_GotFocus;
            return base.OnSaveStateAsync(e);
        }

        private async Task ClearAppDataCacheAsync()
        {
            await Platform.Current.Storage.ClearAppDataCacheFolderAsync();
            this.AppCacheTask = new NotifyTaskCompletion<string>(Platform.Current.Storage.GetAppDataCacheFolderSizeAsync());
        }

        #endregion
    }

    public partial class GeneralSettingsViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public GeneralSettingsViewModel ViewModel { get { return this; } }
    }
}

namespace MediaAppSample.Core.ViewModels.Designer
{
    public sealed class GeneralSettingsViewModel : MediaAppSample.Core.ViewModels.GeneralSettingsViewModel
    {
        public GeneralSettingsViewModel()
        {
        }
    }
}