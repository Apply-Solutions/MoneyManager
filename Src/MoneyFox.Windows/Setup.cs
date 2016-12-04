using Windows.UI.Xaml.Controls;
using Autofac;
using Autofac.Extras.MvvmCross;
using Cheesebaron.MvxPlugins.Connectivity;
using Cheesebaron.MvxPlugins.Connectivity.WindowsCommon;
using Cheesebaron.MvxPlugins.Settings.Interfaces;
using Cheesebaron.MvxPlugins.Settings.WindowsCommon;
using MoneyFox.Business;
using MoneyFox.Foundation.Interfaces;
using MoneyFox.Windows.Services;
using MvvmCross.Core.ViewModels;
using MvvmCross.Platform;
using MvvmCross.Platform.Platform;
using MvvmCross.Platform.Plugins;
using MvvmCross.Platform.UI;
using MvvmCross.Plugins.Email;
using MvvmCross.Plugins.File;
using MvvmCross.Plugins.File.WindowsCommon;
using MvvmCross.Plugins.Sqlite;
using MvvmCross.Plugins.Sqlite.WindowsUWP;
using MvvmCross.Plugins.Visibility.WindowsCommon;
using MvvmCross.Plugins.WebBrowser;
using MvvmCross.Plugins.WebBrowser.WindowsCommon;
using MvvmCross.WindowsUWP.Platform;
using PluginLoader = MvvmCross.Plugins.Messenger.PluginLoader;
using MoneyFox.Windows.Business;
using MvvmCross.Platform.IoC;
using MvvmCross.Plugins.Email.WindowsUWP;

namespace MoneyFox.Windows
{
    public class Setup : MvxWindowsSetup
    {
        public Setup(Frame frame)
            : base(frame)
        {
        }

        public override void LoadPlugins(IMvxPluginManager pluginManager)
        {
            base.LoadPlugins(pluginManager);
            pluginManager.EnsurePluginLoaded<PluginLoader>();

            //We have to do this here, since the loading via bootloader won't work for UWP projects
            Mvx.RegisterType<IMvxComposeEmailTask, MvxComposeEmailTask>();
            Mvx.RegisterType<IMvxWebBrowserTask, MvxWebBrowserTask>();
            Mvx.RegisterType<IMvxSqliteConnectionFactory, WindowsSqliteConnectionFactory>();
            Mvx.RegisterType<IMvxFileStore, MvxWindowsCommonFileStore>();
            Mvx.RegisterType<ISettings, WindowsCommonSettings>();
            Mvx.RegisterType<IConnectivity, Connectivity>();
            Mvx.RegisterType<IMvxNativeVisibility, MvxWinRTVisibility>();
        }

        protected override IMvxIoCProvider CreateIocProvider()
        {
            var cb = new ContainerBuilder();

            cb.RegisterModule<BusinessModule>();
            cb.RegisterModule<WindowsModule>();

            return new AutofacMvxIocProvider(cb.Build());
        }

        protected override void InitializeFirstChance()
        {
            base.InitializeFirstChance();
            Mvx.RegisterType<IDialogService, DialogService>();
            Mvx.RegisterType<ITileUpdateService, TileUpdateService>();
            Mvx.RegisterType<IOneDriveAuthenticator, OneDriveAuthenticator>();
            Mvx.RegisterType<IProtectedData, ProtectedData>();
            Mvx.RegisterType<INotificationService, NotificationService>();
            Mvx.RegisterType<IBackgroundTaskManager, BackgroundTaskManager>();
            Mvx.RegisterType<ITileManager, TileManager>();
        }

        protected override IMvxApplication CreateApp() => new MoneyFox.Business.App();

        protected override IMvxTrace CreateDebugTrace() => new DebugTrace();
    }
}