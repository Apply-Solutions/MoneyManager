﻿using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediatR;
using MoneyFox.Application.Accounts.Commands.DeleteAccountById;
using MoneyFox.Application.Accounts.Queries.GetAccounts;
using MoneyFox.Application.Accounts.Queries.GetTotalEndOfMonthBalance;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Application.Common.Messages;
using MoneyFox.Application.Resources;
using MoneyFox.Extensions;
using MoneyFox.Ui.Shared.Groups;
using MoneyFox.Ui.Shared.ViewModels.Accounts;
using MoneyFox.Views.Accounts;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MoneyFox.ViewModels.Accounts
{
    public class AccountListViewModel : ViewModelBase
    {
        private ObservableCollection<AlphaGroupListGroupCollection<AccountViewModel>> accounts = new ObservableCollection<AlphaGroupListGroupCollection<AccountViewModel>>();

        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly IDialogService dialogService;

        private bool isRunning;

        public AccountListViewModel(IMediator mediator, IMapper mapper, IDialogService dialogService)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.dialogService = dialogService;
        }

        public void Subscribe()
            => MessengerInstance.Register<ReloadMessage>(this, async (m) => await ReloadMessageSent());

        public void Unsubscribe()
            => MessengerInstance.Unregister<ReloadMessage>(this, async (m) => await ReloadMessageSent());

        public async Task OnAppearingAsync()
        {
            try
            {
                if(isRunning)
                {
                    return;
                }

                isRunning = true;

                Accounts.Clear();

                List<AccountViewModel>? accountVms = mapper.Map<List<AccountViewModel>>(await mediator.Send(new GetAccountsQuery()));
                accountVms.ForEach(async x => x.EndOfMonthBalance = await mediator.Send(new GetAccountEndOfMonthBalanceQuery(x.Id)));

                var includedAccountGroup = new AlphaGroupListGroupCollection<AccountViewModel>(Strings.IncludedAccountsHeader);
                var excludedAccountGroup = new AlphaGroupListGroupCollection<AccountViewModel>(Strings.ExcludedAccountsHeader);

                includedAccountGroup.AddRange(accountVms.Where(x => !x.IsExcluded));
                excludedAccountGroup.AddRange(accountVms.Where(x => x.IsExcluded));

                if(includedAccountGroup.Any())
                {
                    Accounts.Add(includedAccountGroup);
                }

                if(excludedAccountGroup.Any())
                {
                    Accounts.Add(excludedAccountGroup);
                }
            }
            finally
            {
                isRunning = false;
            }
        }

        public ObservableCollection<AlphaGroupListGroupCollection<AccountViewModel>> Accounts
        {
            get => accounts;
            set
            {
                accounts = value;
                RaisePropertyChanged();
            }
        }

        public RelayCommand GoToAddAccountCommand => new RelayCommand(async () => await Shell.Current.GoToModalAsync(ViewModelLocator.AddAccountRoute));

        public RelayCommand<AccountViewModel> GoToEditAccountCommand
            => new RelayCommand<AccountViewModel>(async (accountViewModel)
                => await Shell.Current.Navigation.PushModalAsync(
                    new NavigationPage(new EditAccountPage(accountViewModel.Id)) { BarBackgroundColor = Color.Transparent }));

        public RelayCommand<AccountViewModel> GoToTransactionListCommand
            => new RelayCommand<AccountViewModel>(async (accountViewModel)
                => await Shell.Current.GoToAsync($"{ViewModelLocator.PaymentListRoute}?accountId={accountViewModel.Id}"));

        public RelayCommand<AccountViewModel> DeleteAccountCommand
            => new RelayCommand<AccountViewModel>(async (accountViewModel)
                => await DeleteAccountAsync(accountViewModel));

        private async Task ReloadMessageSent() => await OnAppearingAsync();

        private async Task DeleteAccountAsync(AccountViewModel accountViewModel)
        {
            if(await dialogService.ShowConfirmMessageAsync(Strings.DeleteTitle,
                Strings.DeleteAccountConfirmationMessage,
                Strings.YesLabel,
                Strings.NoLabel))
            {
                await mediator.Send(new DeleteAccountByIdCommand(accountViewModel.Id));
                await OnAppearingAsync();
            }
        }
    }
}
