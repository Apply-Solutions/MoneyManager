﻿using System.Threading.Tasks;
using GenericServices;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.Parameters;
using MoneyFox.ServiceLayer.QueryObject;
using MoneyFox.ServiceLayer.Services;
using MvvmCross.Logging;
using MvvmCross.Navigation;

namespace MoneyFox.ServiceLayer.ViewModels
{
    public class AddCategoryViewModel : ModifyCategoryViewModel
    {
        private readonly ICrudServicesAsync crudServices;
        private readonly IDialogService dialogService;

        public AddCategoryViewModel(ICrudServicesAsync crudServices,
            IDialogService dialogService,
            ISettingsFacade settingsFacade,
            IBackupService backupService,
            IMvxLogProvider logProvider,
            IMvxNavigationService navigationService) 
                : base(crudServices, settingsFacade, backupService, dialogService, logProvider, navigationService)
        {
            this.crudServices = crudServices;
            this.dialogService = dialogService;
        }

        public override void Prepare(ModifyCategoryParameter parameter)
        {
            SelectedCategory = new CategoryViewModel();
            Title = Strings.AddCategoryTitle;

            base.Prepare(parameter);
        }

        protected override async Task SaveCategory()
        {
            if (await crudServices.ReadManyNoTracked<AccountViewModel>().AnyWithNameAsync(SelectedCategory.Name))
            {
                await dialogService.ShowMessage(Strings.DuplicatedNameTitle, Strings.DuplicateCategoryMessage);
                return;
            }

            await crudServices.CreateAndSaveAsync(SelectedCategory, "ctor(2)");

            if (!crudServices.IsValid)
            {
                await dialogService.ShowMessage(Strings.GeneralErrorTitle, crudServices.GetAllErrors());
            }

            await NavigationService.Close(this);
        }
    }
}