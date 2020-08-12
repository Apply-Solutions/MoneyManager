﻿
using AutoMapper;
using MediatR;
using MoneyFox.Application.Categories.Command.CreateCategory;
using MoneyFox.Domain.Entities;
using MoneyFox.Services;
using System.Threading.Tasks;

namespace MoneyFox.ViewModels.Categories
{
    public class AddCategoryViewModel : ModifyCategoryViewModel
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;

        public AddCategoryViewModel(IMediator mediator,
                                    IMapper mapper,
                                    IDialogService dialogService) : base(mediator, dialogService)
        {
            this.mediator = mediator;
            this.mapper = mapper;
        }

        protected override async Task SaveCategoryAsync()
        {
            await mediator.Send(new CreateCategoryCommand(mapper.Map<Category>(SelectedCategory)));
        }
    }
}