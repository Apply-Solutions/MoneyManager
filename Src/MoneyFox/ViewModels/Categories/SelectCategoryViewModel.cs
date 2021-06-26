using AutoMapper;
using GalaSoft.MvvmLight.Command;
using MediatR;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Messages;
using MoneyFox.Ui.Shared.ViewModels.Categories;

namespace MoneyFox.ViewModels.Categories
{
    public class SelectCategoryViewModel : CategoryListViewModel
    {
        public SelectCategoryViewModel(IMediator mediator, IMapper mapper, IDialogService dialogService)
            : base(mediator, mapper, dialogService)
        {
        }

        public RelayCommand<CategoryViewModel> SelectCategoryCommand => new RelayCommand<CategoryViewModel>(async (c) =>
        {
            MessengerInstance.Send(new CategorySelectedMessage(c.Id));
            await App.Current.MainPage.Navigation.PopModalAsync();
        });
    }
}
