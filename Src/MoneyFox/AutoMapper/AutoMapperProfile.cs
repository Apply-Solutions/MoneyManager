﻿using AutoMapper;
using MoneyFox.Application.Common.Interfaces.Mapping;
using MoneyFox.ViewModels.Payments;
using System.Collections.Generic;

namespace MoneyFox.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            LoadStandardMappings();
            LoadCustomMappings();
        }

        private void LoadStandardMappings()
        {
            IList<Map> mapsFrom = MapperProfileHelper.LoadStandardMappings(typeof(PaymentViewModel).Assembly);

            foreach(Map map in mapsFrom)
            {
                CreateMap(map.Source, map.Destination).ReverseMap();
            }
        }

        private void LoadCustomMappings()
        {
            IList<IHaveCustomMapping> mapsFrom = MapperProfileHelper.LoadCustomMappings(typeof(PaymentViewModel).Assembly);

            foreach(IHaveCustomMapping map in mapsFrom)
            {
                map.CreateMappings(this);
            }
        }
    }
}