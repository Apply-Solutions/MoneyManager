﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using MediatR;
using MoneyFox.Application.Accounts.Commands.DeleteAccountById;
using MoneyFox.Application.Accounts.Queries.GetExcludedAccount;
using MoneyFox.Application.Accounts.Queries.GetIncludedAccount;
using MoneyFox.Application.Facades;
using MoneyFox.Application.Messages;
using MoneyFox.Application.Resources;
using MoneyFox.Presentation.Commands;
using MoneyFox.Presentation.Groups;
using MoneyFox.Presentation.Services;
using MoneyFox.Presentation.ViewModels.Interfaces;
using NLog;
using Xamarin.Forms;
using XF.Material.Forms.Models;
using IDialogService = MoneyFox.Presentation.Interfaces.IDialogService;

namespace MoneyFox.Presentation.ViewModels
{
    public class AccountListViewModel : BaseViewModel, IAccountListViewModel
    {
        private const int MENU_RESULT_EDIT_INDEX = 0;
        private const int MENU_RESULT_DELETE_INDEX = 1;

        private readonly Logger logManager = LogManager.GetCurrentClassLogger();

        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly IDialogService dialogService;
        private readonly ISettingsFacade settingsFacade;
        private readonly INavigationService navigationService;

        private ObservableCollection<AlphaGroupListGroupCollection<AccountViewModel>> accounts;

        /// <summary>
        ///     Constructor
        /// </summary>
        public AccountListViewModel(IMediator mediator,
                                    IMapper mapper,
                                    IBalanceCalculationService balanceCalculationService,
                                    IDialogService dialogService,
                                    ISettingsFacade settingsFacade,
                                    INavigationService navigationService,
                                    IMessenger messenger)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.settingsFacade = settingsFacade;

            MessengerInstance = messenger;

            BalanceViewModel = new BalanceViewModel(balanceCalculationService);
            ViewActionViewModel = new AccountListViewActionViewModel(mediator, this.navigationService);

            Accounts = new ObservableCollection<AlphaGroupListGroupCollection<AccountViewModel>>();

            MessengerInstance.Register<BackupRestoredMessage>(this, async message => await Load());
        }

        public IBalanceViewModel BalanceViewModel { get; }

        public IAccountListViewActionViewModel ViewActionViewModel { get; }

        public ObservableCollection<AlphaGroupListGroupCollection<AccountViewModel>> Accounts
        {
            get => accounts;
            private set
            {
                if (accounts == value) return;
                accounts = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasNoAccounts));
            }
        }

        public bool HasNoAccounts => !Accounts.Any();
        public List<string> MenuActions => new List<string>{Strings.EditLabel, Strings.DeleteLabel};

        public AsyncCommand LoadDataCommand => new AsyncCommand(Load);

        public RelayCommand<AccountViewModel> OpenOverviewCommand => new RelayCommand<AccountViewModel>(GoToPaymentOverView);

        public Command<MaterialMenuResult> MenuSelectedCommand => new Command<MaterialMenuResult>(MenuSelected);

        public RelayCommand<AccountViewModel> EditAccountCommand => new RelayCommand<AccountViewModel>(EditAccount);

        public AsyncCommand<AccountViewModel> DeleteAccountCommand => new AsyncCommand<AccountViewModel>(DeleteAsync);

        public RelayCommand GoToAddAccountCommand => new RelayCommand(GoToAddAccount);

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Major Bug", "S3168:\"async\" methods should not return \"void\"", Justification = "Acts as event handler.>")]
        private async void MenuSelected(MaterialMenuResult menuResult)
        {
            var accountViewModel = menuResult.Parameter as AccountViewModel;

            switch (menuResult.Index)
            {
                case MENU_RESULT_EDIT_INDEX:
                    navigationService.NavigateTo(ViewModelLocator.EditAccount, accountViewModel.Id);
                    break;

                case MENU_RESULT_DELETE_INDEX:
                    await DeleteAsync(accountViewModel);
                    break;

                default:
                    logManager.Warn("Invalid Index for Menu Selected in Account List. Index: {0}", menuResult.Index);
                    break;
            }
        }

        private void EditAccount(AccountViewModel accountViewModel)
        {
            navigationService.NavigateTo(ViewModelLocator.EditAccount, accountViewModel.Id);
        }

        private async Task Load()
        {
            try
            {
                await BalanceViewModel.UpdateBalanceCommand.ExecuteAsync();

                var includedAlphaGroup = new AlphaGroupListGroupCollection<AccountViewModel>(Strings.IncludedAccountsHeader);
                includedAlphaGroup.AddRange(mapper.Map<List<AccountViewModel>>(await mediator.Send(new GetIncludedAccountQuery())));

                var excludedAlphaGroup = new AlphaGroupListGroupCollection<AccountViewModel>(Strings.ExcludedAccountsHeader);
                excludedAlphaGroup.AddRange(mapper.Map<List<AccountViewModel>>(await mediator.Send(new GetExcludedAccountQuery())));

                Accounts.Clear();

                if (includedAlphaGroup.Any()) Accounts.Add(includedAlphaGroup);

                if (excludedAlphaGroup.Any()) Accounts.Add(excludedAlphaGroup);

                RaisePropertyChanged(nameof(HasNoAccounts));
            }
            catch (Exception ex)
            {
                logManager.Error(ex);
                await dialogService.ShowMessage(Strings.GeneralErrorTitle, ex.ToString());
            }
        }

        private void GoToPaymentOverView(AccountViewModel accountViewModel)
        {
            if (accountViewModel == null) return;

            navigationService.NavigateTo(ViewModelLocator.PaymentList, accountViewModel.Id);
        }

        private async Task DeleteAsync(AccountViewModel accountToDelete)
        {
            if (accountToDelete == null) return;

            if (await dialogService.ShowConfirmMessageAsync(Strings.DeleteTitle, Strings.DeleteAccountConfirmationMessage))
            {
                await mediator.Send(new DeleteAccountByIdCommand(accountToDelete.Id));
                logManager.Info("Account with Id {id} deleted.", accountToDelete.Id);

                Accounts.Clear();
                await Load();

                settingsFacade.LastDatabaseUpdate = DateTime.Now;
            }
        }

        private void GoToAddAccount()
        {
            navigationService.NavigateTo(ViewModelLocator.AddAccount);
        }
    }
}
