﻿using EntityFramework.DbContextScope;
using Microsoft.EntityFrameworkCore;
using MoneyFox.Business.Authentication;
using MoneyFox.Business.ViewModels;
using MoneyFox.DataAccess;
using MoneyFox.Foundation;
using MvvmCross;
using MvvmCross.ViewModels;

namespace MoneyFox
{
    /// <summary>
    ///     Entry piont to the Application for MvvmCross.
    /// </summary>
    public class CoreApp : MvxApplication
    {
        public static AppPlatform CurrentPlatform { get; set; }

        /// <summary>
        ///     Initializes this instance.
        /// </summary>
        public override async void Initialize()
        {
            var dbContextScopeFactory = new DbContextScopeFactory();
            var ambientDbContextLocator = new AmbientDbContextLocator();

            using (dbContextScopeFactory.Create())
            {
                await ambientDbContextLocator.Get<ApplicationContext>().Database.MigrateAsync();
            }

            if (Mvx.Resolve<Session>().ValidateSession())
            {
                if (CurrentPlatform == AppPlatform.UWP)
                {
                    RegisterAppStart<AccountListViewModel>();
                }
                else
                {
                    RegisterAppStart<MainViewModel>();
                }
            } else
            {
                RegisterAppStart<LoginViewModel>();
            }
        }
    }
}