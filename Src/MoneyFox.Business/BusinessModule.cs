﻿using Autofac;
using MoneyFox.Business.Authentication;
using MoneyFox.Foundation.Interfaces;
using MoneyFox.Foundation.Resources;
using MoneyFox.Service;
using MvvmCross.Localization;
using MvvmCross.Plugins.ResxLocalization;

namespace MoneyFox.Business
{
    /// <summary>
    ///     Registers the dependencies for the business module
    /// </summary>
    public class BusinessModule : Module
    {
        /// <summary>
        ///     Registers the dependencies for the business module
        /// </summary>
        /// <param name="builder">Containerbuilder</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterModule<ServiceModule>();

            builder.RegisterType<PasswordStorage>().As<IPasswordStorage>();
            builder.RegisterInstance(new MvxResxTextProvider(Strings.ResourceManager)).As<IMvxTextProvider>();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .AsSelf()
                .SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("Provider"))
                .AsSelf()
                .SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("Repository"))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("DataProvider"))
                .AsSelf()
                .SingleInstance();
            
            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("Manager"))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .Where(x => !x.Name.StartsWith("DesignTime"))
                .AsImplementedInterfaces()
                .SingleInstance();

            builder.RegisterAssemblyTypes(ThisAssembly)
                .Where(t => t.Name.EndsWith("ViewModel"))
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<Session>();
        }
    }
}
