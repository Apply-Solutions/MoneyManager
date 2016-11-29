using Autofac;
using MoneyFox.Droid.Manager;
using MoneyFox.Droid.OneDriveAuth;
using MoneyFox.Droid.Services;
using MoneyFox.Foundation.Interfaces;

namespace MoneyFox.Droid
{
    public class DroidModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<DialogService>().As<IDialogService>();
            builder.RegisterType<ModifyDialogService>().As<IModifyDialogService>();
            builder.RegisterType<OneDriveAuthenticator>().As<IOneDriveAuthenticator>();
            builder.RegisterType<ProtectedData>().As<IProtectedData>();
            builder.RegisterType<NotificationService>().As<INotificationService>();
            builder.RegisterType<BackgroundTaskManager>().As<IBackgroundTaskManager>();
            builder.RegisterType<TileManager>().As<ITileManager>();
        }
    }
}