﻿using Foundation;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using Microsoft.Identity.Client;
using MoneyFox.BusinessDbAccess.PaymentActions;
using MoneyFox.BusinessLogic.Adapters;
using MoneyFox.BusinessLogic.Backup;
using MoneyFox.BusinessLogic.PaymentActions;
using MoneyFox.DataLayer;
using MoneyFox.Foundation.Constants;
using MoneyFox.Presentation;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.Services;
using MvvmCross;
using MvvmCross.Forms.Platforms.Ios.Core;
using MvvmCross.Plugin.File;
using PCLAppConfig;
using PCLAppConfig.FileSystemStream;
using Rg.Plugins.Popup;
using SQLitePCL;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UIKit;
using Xamarin.Forms.Platform.iOS;

#if !DEBUG
using Microsoft.AppCenter;
#endif

namespace MoneyFox.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register(nameof(AppDelegate))]
    public class AppDelegate : MvxFormsApplicationDelegate<ApplicationSetup, CoreApp, App>
    {
        // Minimum number of seconds between a background refresh
        // 15 minutes = 60 * 60 = 3600 seconds
        private const double MINIMUM_BACKGROUND_FETCH_INTERVAL = 3600;

        /// <inheritdoc />
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            ConfigurationManager.Initialise(PortableStream.Current);

#if !DEBUG
            AppCenter.Start(ConfigurationManager.AppSettings["IosAppcenterSecret"], typeof(Analytics), typeof(Crashes));
#endif

            UINavigationBar.Appearance.BarTintColor = StyleHelper.PrimaryColor.ToUIColor();
            UINavigationBar.Appearance.TintColor = UIColor.White;

            UIApplication.SharedApplication.StatusBarStyle = UIStatusBarStyle.BlackOpaque;
            app.SetMinimumBackgroundFetchInterval(MINIMUM_BACKGROUND_FETCH_INTERVAL);
            UIApplication.SharedApplication.SetMinimumBackgroundFetchInterval(UIApplication.BackgroundFetchIntervalMinimum);

            EfCoreContext.DbPath = GetLocalFilePath();
            Batteries.Init();
            Popup.Init();

            return base.FinishedLaunching(app, options);
        }

        protected override async void RunAppStart(object hint = null)
        {
            base.RunAppStart(hint);

            await SyncBackup();
            await ClearPayments();
            await CreateRecurringPayments();
        }

        private static string GetLocalFilePath()
        {
            string docFolder = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            string libFolder = Path.Combine(docFolder, "..", "Library", "Databases");

            if (!Directory.Exists(libFolder))
            {
                Directory.CreateDirectory(libFolder);
            }

            return Path.Combine(libFolder, DatabaseConstants.DB_NAME);
        }

        // Needed for auth
        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            AuthenticationContinuationHelper.SetAuthenticationContinuationEventArgs(url);
            return true;
        }

        public override async void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        {
            if (!Mvx.IoCProvider.CanResolve<IRecurringPaymentManager>() || !Mvx.IoCProvider.CanResolve<IBackupManager>()) return;

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

        private async Task SyncBackup()
        {
            if (!Mvx.IoCProvider.CanResolve<IMvxFileStore>()) return;

            var settingsFacade = new SettingsFacade(new SettingsAdapter());
            if (!settingsFacade.IsBackupAutouploadEnabled || !settingsFacade.IsLoggedInToBackupService) return;

            try
            {
                EfCoreContext.DbPath = GetLocalFilePath();

                var pca = PublicClientApplicationBuilder
                    .Create(ServiceConstants.MSAL_APPLICATION_ID)
                    .WithRedirectUri($"msal{ServiceConstants.MSAL_APPLICATION_ID}://auth")
                    .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                    .Build();

                var backupManager = new BackupManager(
                    new OneDriveService(pca),
                    Mvx.IoCProvider.Resolve<IMvxFileStore>(),
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

        private async Task ClearPayments()
        {
            var settingsFacade = new SettingsFacade(new SettingsAdapter());
            try
            {
                Debug.WriteLine("ClearPayments Job started");
                EfCoreContext.DbPath = GetLocalFilePath();

                var context = new EfCoreContext();
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

        private async Task CreateRecurringPayments()
        {
            var settingsFacade = new SettingsFacade(new SettingsAdapter());

            try
            {
                Debug.WriteLine("RecurringPayment Job started.");
                EfCoreContext.DbPath = GetLocalFilePath();

                var context = new EfCoreContext();
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
