using System;
using System.IO;
using System.Threading.Tasks;
using Android.App;
using Android.App.Job;
using Android.Content;
using Android.OS;
using Microsoft.AppCenter.Crashes;
using MoneyFox.BusinessDbAccess.PaymentActions;
using MoneyFox.BusinessLogic.Adapters;
using MoneyFox.BusinessLogic.PaymentActions;
using MoneyFox.DataLayer;
using MoneyFox.Foundation.Constants;
using MoneyFox.ServiceLayer.Facades;
using Debug = System.Diagnostics.Debug;
using Environment = System.Environment;
using JobSchedulerType = Android.App.Job.JobScheduler;

namespace MoneyFox.Droid.Jobs
{
    /// <summary>
    ///     Job to clear payments on a regular basis.
    /// </summary>
    [Service(Exported = true, Permission = "android.permission.BIND_JOB_SERVICE")]
    public class ClearPaymentsJob : JobService
    {
        private const int CLEARPAYMENT_JOB_ID = 10;

        /// <inheritdoc />
        public override bool OnStartJob(JobParameters args)
        {
            Task.Run(async () => await ClearPayments(args));
            return true;
        }

        /// <inheritdoc />
        public override bool OnStopJob(JobParameters args)
        {
            return true;
        }

        /// <inheritdoc />
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var callback = (Messenger)intent.GetParcelableExtra("messenger");
            var m = Message.Obtain();
            m.What = MainActivity.MESSAGE_SERVICE_CLEAR_PAYMENTS;
            m.Obj = this;
            try
            {
                callback.Send(m);
            }
            catch (RemoteException e)
            {
                Debug.WriteLine(e);
            }
            return StartCommandResult.NotSticky;
        }

        private async Task ClearPayments(JobParameters args)
        {
            var settingsManager = new SettingsFacade(new SettingsAdapter());
            try
            {
                Debug.WriteLine("ClearPayments Job started");
                EfCoreContext.DbPath =
                    Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal),
                                 DatabaseConstants.DB_NAME);

                var context = new EfCoreContext();
                await new ClearPaymentAction(new ClearPaymentDbAccess(context)).ClearPayments();
                context.SaveChanges();

                Debug.WriteLine("ClearPayments Job finished.");
                JobFinished(args, false);
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
            }
            finally
            {
                settingsManager.LastExecutionTimeStampClearPayments = DateTime.Now;
            }
        }

        /// <summary>
        ///     Schedules the task for execution.
        /// </summary>
        public void ScheduleTask()
        {
            var builder = new JobInfo.Builder(CLEARPAYMENT_JOB_ID,
                                              new ComponentName(
                                                  this, Java.Lang.Class.FromType(typeof(ClearPaymentsJob))));
            // Execute all 60 Minutes
            builder.SetPeriodic(60 * 60 * 1000);
            builder.SetPersisted(true);
            builder.SetRequiredNetworkType(NetworkType.None);
            builder.SetRequiresDeviceIdle(false);
            builder.SetRequiresCharging(false);

            var tm = (JobSchedulerType)GetSystemService(JobSchedulerService);
            tm.Schedule(builder.Build());
        }
    }
}