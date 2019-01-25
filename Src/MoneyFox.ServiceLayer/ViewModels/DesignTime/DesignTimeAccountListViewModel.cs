﻿using System.Collections.ObjectModel;
using System.Globalization;
using MoneyFox.Business.ViewModels;
using MoneyFox.Business.ViewModels.Interfaces;
using MoneyFox.DataLayer.Entities;
using MoneyFox.Foundation.Groups;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Utilities;
using MvvmCross.Commands;
using MvvmCross.ViewModels;

namespace MoneyFox.ServiceLayer.ViewModels.DesignTime
{
    public class DesignTimeAccountListViewModel : MvxViewModel, IAccountListViewModel
    {
        public LocalizedResources Resources { get; } = new LocalizedResources(typeof(Strings), CultureInfo.CurrentUICulture);

        public ObservableCollection<AlphaGroupListGroup<AccountViewModel>> Accounts => new ObservableCollection<AlphaGroupListGroup<AccountViewModel>>
        {
            new AlphaGroupListGroup<AccountViewModel>("Included")
            {
                new AccountViewModel(new Account{Name =  "Income", CurrentBalance = 1234})
            },
            new AlphaGroupListGroup<AccountViewModel>("Excluded")
            {
                new AccountViewModel(new Account{Name =  "Savings", CurrentBalance = 4325})
            }
        };

        public bool HasNoAccounts { get; } = true;
        public IBalanceViewModel BalanceViewModel { get; }
        public IAccountListViewActionViewModel ViewActionViewModel { get; }
        public MvxAsyncCommand<AccountViewModel> OpenOverviewCommand { get; }
        public MvxAsyncCommand<AccountViewModel> EditAccountCommand { get; }
        public MvxAsyncCommand<AccountViewModel> DeleteAccountCommand { get; }
        public MvxAsyncCommand GoToAddAccountCommand { get; }
    }
}
