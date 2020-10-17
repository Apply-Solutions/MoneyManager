﻿using Autofac;
using Microsoft.EntityFrameworkCore;

namespace MoneyFox.Persistence
{
    public class PersistenceModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => EfCoreContextFactory.Create())
                   .As<DbContext>()
                   .AsImplementedInterfaces();

            builder.RegisterType<ContextAdapter>().AsImplementedInterfaces();
        }
    }
}
