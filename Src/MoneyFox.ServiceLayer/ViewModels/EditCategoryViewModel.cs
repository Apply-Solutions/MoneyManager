﻿using System;
using System.Globalization;
using System.Threading.Tasks;
using GenericServices;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.Parameters;
using MoneyFox.ServiceLayer.Services;
using MvvmCross.Commands;
using MvvmCross.Logging;
using MvvmCross.Navigation;

namespace MoneyFox.ServiceLayer.ViewModels
{
    public class EditCategoryViewModel : ModifyCategoryViewModel
    {
        private readonly ICrudServicesAsync crudServices;
        private readonly IDialogService dialogService;
        private readonly ISettingsFacade settingsFacade;
        private readonly IBackupService backupService;

        public EditCategoryViewModel(ICrudServicesAsync crudServices,
            IDialogService dialogService,
            ISettingsFacade settingsFacade,
            IBackupService backupService,
            IMvxLogProvider logProvider,
            IMvxNavigationService navigationService)
            : base(crudServices, settingsFacade, backupService, dialogService, logProvider, navigationService)
        {
            this.crudServices = crudServices;
            this.dialogService = dialogService;
            this.settingsFacade = settingsFacade;
            this.backupService = backupService;
        }

        public override string Title { get; set; }

        public override async void Prepare(ModifyCategoryParameter parameter)
        {
            SelectedCategory = await crudServices.ReadSingleAsync<CategoryViewModel>(parameter.CategoryId)
                                                 ;
            Title = string.Format(CultureInfo.InvariantCulture, Strings.EditCategoryTitle, SelectedCategory.Name);

            base.Prepare(parameter);
        }

        /// <summary>
        ///     Delete the selected CategoryViewModel from the database
        /// </summary>
        public MvxAsyncCommand DeleteCommand => new MvxAsyncCommand(DeleteCategory);

        protected override async Task SaveCategory()
        {
            await crudServices.UpdateAndSaveAsync(SelectedCategory)
                              ;
            if (!crudServices.IsValid)
            {
                await dialogService.ShowMessage(Strings.GeneralErrorTitle, crudServices.GetAllErrors())
                                   ;
            }

            await CancelCommand.ExecuteAsync();

        }

        private async Task DeleteCategory()
        {
            await crudServices.DeleteAndSaveAsync<AccountViewModel>(SelectedCategory.Id)
                              ;
            settingsFacade.LastExecutionTimeStampSyncBackup = DateTime.Now;
#pragma warning disable 4014
            backupService.EnqueueBackupTask();
#pragma warning restore 4014
            await CancelCommand.ExecuteAsync();
        }
    }
}