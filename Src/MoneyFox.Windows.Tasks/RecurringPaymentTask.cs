﻿using System.Diagnostics;
using Windows.ApplicationModel.Background;
using EntityFramework.DbContextScope;
using MoneyFox.Business.Manager;
using MoneyFox.DataAccess;
using MoneyFox.Foundation.Constants;
using MoneyFox.Service.DataServices;

namespace MoneyFox.Windows.Tasks
{
    /// <summary>
    ///     Periodically tests if there are new recurring payments and creates these.
    /// </summary>
    public sealed class RecurringPaymentTask : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            Debug.WriteLine("RecurringPaymentTask started.");

            ApplicationContext.DbPath = DatabaseConstants.DB_NAME;

            try
            {
                var dbContextScopeFactory = new DbContextScopeFactory();
                var ambientDbContextLocator = new AmbientDbContextLocator();

                await new RecurringPaymentManager(
                        new RecurringPaymentService(dbContextScopeFactory, ambientDbContextLocator),
                        new PaymentService(dbContextScopeFactory, ambientDbContextLocator))
                    .CreatePaymentsUpToRecur();
            }
            finally
            {
                Debug.WriteLine("RecurringPaymentTask stopped.");
                deferral.Complete();
            }
        }
    }
}