﻿using System;
using System.Linq;
using GenericServices;
using GenericServices.PublicButHidden;
using GenericServices.Setup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using MoneyFox.BusinessDbAccess.StatisticDataProvider;
using MoneyFox.BusinessLogic.Adapters;
using MoneyFox.BusinessLogic.Backup;
using MoneyFox.BusinessLogic.PaymentActions;
using MoneyFox.BusinessLogic.StatisticDataProvider;
using MoneyFox.DataLayer;
using MoneyFox.Foundation;
using MoneyFox.Foundation.Constants;
using MoneyFox.ServiceLayer.Authentication;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.Services;
using MoneyFox.ServiceLayer.ViewModels;
using MvvmCross;
using MvvmCross.IoC;
using MvvmCross.ViewModels;

namespace MoneyFox.Presentation
{
    /// <summary>
    ///     Entry point to the Application for MvvmCross.
    /// </summary>
    public class
        CoreApp : MvxApplication
    {
        public static AppPlatform CurrentPlatform { get; set; }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        public override void Initialize()
        {
            Mvx.IoCProvider.ConstructAndRegisterSingleton<IPasswordStorage, PasswordStorage>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton<ICrudServices, CrudServices>();
            Mvx.IoCProvider.LazyConstructAndRegisterSingleton(() => PublicClientApplicationBuilder
                                                                  .Create(ServiceConstants.MSAL_APPLICATION_ID)
                                                                  .WithRedirectUri($"msal{ServiceConstants.MSAL_APPLICATION_ID}://auth")
                                                                  .WithIosKeychainSecurityGroup("com.microsoft.adalcache")
                                                                  .Build());

            typeof(OneDriveService).Assembly.CreatableTypes()
                                  .EndingWith("Service")
                                  .AsInterfaces()
                                  .RegisterAsDynamic();

            typeof(BackupService).Assembly.CreatableTypes()
                                  .EndingWith("Service")
                                  .AsInterfaces()
                                  .RegisterAsDynamic();

            typeof(SettingsAdapter).Assembly.CreatableTypes()
                                 .EndingWith("Adapter")
                                 .AsInterfaces()
                                 .RegisterAsDynamic();

            typeof(SettingsFacade).Assembly.CreatableTypes()
                                 .EndingWith("Facade")
                                 .AsInterfaces()
                                 .RegisterAsDynamic();

            typeof(BackupManager).Assembly.CreatableTypes()
                                 .EndingWith("Manager")
                                 .AsInterfaces()
                                 .RegisterAsDynamic();

            typeof(ClearPaymentAction).Assembly.CreatableTypes()
                                 .EndingWith("Action")
                                 .AsInterfaces()
                                 .RegisterAsDynamic();

            Mvx.IoCProvider.RegisterType(() => new Session(Mvx.IoCProvider.Resolve<ISettingsFacade>()));

            typeof(StatisticDbAccess).Assembly.CreatableTypes()
                                 .EndingWith("DbAccess")
                                 .AsInterfaces()
                                 .RegisterAsDynamic();

            typeof(CashFlowDataProvider).Assembly.CreatableTypes()
                                 .EndingWith("DataProvider")
                                 .AsInterfaces()
                                 .RegisterAsDynamic();

            typeof(MainViewModel).Assembly.CreatableTypes()
                                 .EndingWith("ViewModel")
                                 .Where(x => !x.Name.StartsWith("DesignTime", StringComparison.InvariantCulture))
                                 .AsTypes()
                                 .RegisterAsDynamic();

            typeof(MainViewModel).Assembly.CreatableTypes()
                                 .EndingWith("ViewModel")
                                 .Where(x => !x.Name.StartsWith("DesignTime", StringComparison.InvariantCulture))
                                 .AsInterfaces()
                                 .RegisterAsDynamic();

            SetupContextAndCrudServices();
        }

        private void SetupContextAndCrudServices()
        {
            var context = SetupEfContext();

            Mvx.IoCProvider.RegisterSingleton<EfCoreContext>(SetupEfContext);
            Mvx.IoCProvider.RegisterType<ICrudServicesAsync>(() => SetUpCrudServices(context));
        }

        private static EfCoreContext SetupEfContext()
        {
            var context = new EfCoreContext();
            context.Database.Migrate();

            return context;
        }

        private static ICrudServicesAsync SetUpCrudServices(EfCoreContext context)
        {
            var utData = context.SetupSingleDtoAndEntities<AccountViewModel>();
            utData.AddSingleDto<CategoryViewModel>();
            utData.AddSingleDto<PaymentViewModel>();
            utData.AddSingleDto<RecurringPaymentViewModel>();

            return new CrudServicesAsync(context, utData.ConfigAndMapper);
        }
    }
}