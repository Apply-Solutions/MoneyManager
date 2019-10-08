﻿using Foundation;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using MoneyFox.BusinessDbAccess.PaymentActions;
using MoneyFox.BusinessLogic.Adapters;
using MoneyFox.BusinessLogic.Backup;
using MoneyFox.BusinessLogic.PaymentActions;
using MoneyFox.Presentation;
using PCLAppConfig;
using PCLAppConfig.FileSystemStream;
using Rg.Plugins.Popup;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Autofac;
using CommonServiceLocator;
using MoneyFox.Application;
using MoneyFox.Presentation.Facades;
using MoneyFox.Presentation.Services;
using MoneyFox.Presentation.Utilities;
using NLog;
using NLog.Targets;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using LogLevel = NLog.LogLevel;
using MoneyFox.Application.Constants;
using MoneyFox.Application.FileStore;
using MoneyFox.Persistence;

#if !DEBUG
using Microsoft.AppCenter;
#endif

namespace MoneyFox.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the
    // User Interface of the application, as well as listening (and optionally responding) to
    // application events from iOS.
    [Register(nameof(AppDelegate))]
    public class AppDelegate : FormsApplicationDelegate
    {
        // Minimum number of seconds between a background refresh
        // 15 minutes = 60 * 60 = 3600 seconds
        private const double MINIMUM_BACKGROUND_FETCH_INTERVAL = 3600;

        /// <inheritdoc />
        public override bool FinishedLaunching(UIApplication uiApplication, NSDictionary launchOptions)
        {
            InitLogger();
            ConfigurationManager.Initialise(PortableStream.Current);
            ExecutingPlatform.Current = AppPlatform.iOS;

#if !DEBUG
            AppCenter.Start(ConfigurationManager.AppSettings["IosAppcenterSecret"], typeof(Analytics), typeof(Crashes));
#endif
            RegisterServices();

            Forms.Init();
            FormsMaterial.Init();
            XF.Material.iOS.Material.Init();
            LoadApplication(new App());
            Popup.Init();

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackOpaque;
            uiApplication.SetMinimumBackgroundFetchInterval(MINIMUM_BACKGROUND_FETCH_INTERVAL);
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);

            RunAppStart().FireAndForgetSafeAsync();

            return base.FinishedLaunching(uiApplication, launchOptions);
        }

        private static void RegisterServices()
        {
            var builder = new ContainerBuilder();
            builder.RegisterModule<IosModule>();
            ViewModelLocator.RegisterServices(builder);
        }

        protected static async Task RunAppStart()
        {
            await SyncBackup();
            await ClearPayments();
            await CreateRecurringPayments();
        }

        private void InitLogger()
        {
            var config = new NLog.Config.LoggingConfiguration();

            var logfile = new FileTarget("logfile")
            {
                FileName = GetLogPath(),
                AutoFlush = true,
                ArchiveEvery = FileArchivePeriod.Month
            };
            var debugTarget = new DebugTarget("console");

            config.AddRule(LogLevel.Info, LogLevel.Fatal, debugTarget);
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);

            LogManager.Configuration = config;
        }

        private static string GetLogPath()
        {
            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

            if (!Directory.Exists(libFolder))
            {
                Directory.CreateDirectory(libFolder);
            }

            return Path.Combine(libFolder, "moneyfox.log");
        }

        // Needed for auth
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return true;
        }

        public override async void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            Debug.Write("Enter Background Task");
            var successful = false;
            try
            {
                Analytics.TrackEvent("Start background fetch.");

                await SyncBackup();
                await ClearPayments();
                await CreateRecurringPayments();

                successful = true;
                Analytics.TrackEvent("Background fetch finished successfully.");
            } catch (Exception ex)
            {
                Debug.Write(ex);
                Crashes.TrackError(ex);
            }

            completionHandler(successful ? UIBackgroundFetchResult.NewData : UIBackgroundFetchResult.Failed);
        }

        public override async void WillEnterForeground(UIApplication uiApplication)
        {
            base.WillEnterForeground(uiApplication);

            await SyncBackup();
            await ClearPayments();
            await CreateRecurringPayments();
        }

        private static async Task SyncBackup()
        {
            var settingsFacade = new SettingsFacade(new SettingsAdapter());
            if (!settingsFacade.IsBackupAutouploadEnabled || !settingsFacade.IsLoggedInToBackupService) return;

            try
            {
                var pca = PublicClientApplicationBuilder
                    .Create(ServiceConstants.MSAL_APPLICATION_ID)
                    .WithRedirectUri($"msal{ServiceConstants.MSAL_APPLICATION_ID}://auth")
                    .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                    .Build();

                var backupManager = new BackupManager(
                    new OneDriveService(pca),
                    ServiceLocator.Current.GetInstance<IFileStore>(),
                    new ConnectivityAdapter());

                var backupService = new BackupService(backupManager, settingsFacade);

                var backupDate = await backupService.GetBackupDate();
                if (settingsFacade.LastDatabaseUpdate > backupDate) return;

                await backupService.RestoreBackup();

            } catch (Exception ex)
            {
                Debug.Write(ex);
            } finally
            {
                settingsFacade.LastExecutionTimeStampSyncBackup = DateTime.Now;
            }
        }

        private static async Task ClearPayments()
        {
            var settingsFacade = new SettingsFacade(new SettingsAdapter());
            try
            {
                Debug.WriteLine("ClearPayments Job started");

                var context = EfCoreContextFactory.Create();
                await new ClearPaymentAction(new ClearPaymentDbAccess(context)).ClearPayments();
                context.SaveChanges();

                Debug.WriteLine("ClearPayments Job finished.");
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
            } finally
            {
                settingsFacade.LastExecutionTimeStampClearPayments = DateTime.Now;
            }
        }

        private static async Task CreateRecurringPayments()
        {
            var settingsFacade = new SettingsFacade(new SettingsAdapter());

            try
            {
                Debug.WriteLine("RecurringPayment Job started.");

                var context = EfCoreContextFactory.Create();
                await new RecurringPaymentAction(new RecurringPaymentDbAccess(context))
                    .CreatePaymentsUpToRecur();
                context.SaveChanges();

                Debug.WriteLine("RecurringPayment Job finished.");
            } catch (Exception ex)
            {
                Crashes.TrackError(ex);
            } finally
            {
                settingsFacade.LastExecutionTimeStampRecurringPayments = DateTime.Now;
            }
        }
    }
}
