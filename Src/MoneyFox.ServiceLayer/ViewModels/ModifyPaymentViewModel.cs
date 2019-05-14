﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;
using DynamicData;
using GenericServices;
using Microsoft.EntityFrameworkCore;
using MoneyFox.Foundation;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Facades;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.Messages;
using MoneyFox.ServiceLayer.Parameters;
using MoneyFox.ServiceLayer.QueryObject;
using MoneyFox.ServiceLayer.Services;
using ReactiveUI;
using Splat;

namespace MoneyFox.ServiceLayer.ViewModels
{
    /// <summary>
    ///     Handles the logic of the ModifyPayment view
    /// </summary>
    public abstract class ModifyPaymentViewModel : RouteableViewModelBase
    {
        private readonly IBackupService backupService;
        private readonly ICrudServicesAsync crudServices;
        private readonly IDialogService dialogService;
        private readonly ISettingsFacade settingsFacade;

        private PaymentViewModel selectedPayment;
        private ReadOnlyObservableCollection<AccountViewModel> chargedAccounts;
        private ReadOnlyObservableCollection<AccountViewModel> targetAccounts;

        /// <summary>
        ///     Default constructor
        /// </summary>
        protected ModifyPaymentViewModel(IScreen screen,
                                         ICrudServicesAsync crudServices = null,
                                         IDialogService dialogService = null,
                                         ISettingsFacade settingsFacade = null,
                                         IBackupService backupService = null)
        {
            HostScreen = screen;

            this.crudServices = crudServices ?? Locator.Current.GetService<ICrudServicesAsync>();
            this.dialogService = dialogService ?? Locator.Current.GetService<IDialogService>();
            this.settingsFacade = settingsFacade ?? Locator.Current.GetService<ISettingsFacade>();
            this.backupService = backupService ?? Locator.Current.GetService<IBackupService>();

            MessageBus.Current.Listen<CategorySelectedMessage>().Subscribe(ReceiveMessage);

            this.WhenActivated(async disposable =>
            {
                var accountsSource = new SourceList<AccountViewModel>();
                accountsSource.AddRange(await this.crudServices.ReadManyNoTracked<AccountViewModel>()
                                            .OrderByName()
                                            .ToListAsync());

                accountsSource.Connect()
                              .ObserveOn(RxApp.MainThreadScheduler)
                              .StartWithEmpty()
                              .Bind(out chargedAccounts)
                              .Bind(out targetAccounts)
                              .Subscribe()
                              .DisposeWith(disposable);

                //SelectedItemChangedCommand = ReactiveCommand.Create(UpdateOtherComboBox).DisposeWith(disposable);
                SaveCommand = ReactiveCommand.CreateFromTask(SavePaymentBase).DisposeWith(disposable);
                GoToSelectCategoryDialogCommand =ReactiveCommand.Create(OpenSelectCategoryList).DisposeWith(disposable);
                CancelCommand = ReactiveCommand.Create(Cancel).DisposeWith(disposable);
                ResetCategoryCommand = ReactiveCommand.Create(ResetSelection).DisposeWith(disposable);
            });
        }

        public override IScreen HostScreen { get; }

        /// <summary>
        ///     Updates the targetAccountViewModel and chargedAccountViewModel Comboboxes' dropdown lists.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SelectedItemChangedCommand { get; set; }

        /// <summary>
        ///     Saves the PaymentViewModel or updates the existing depending on the IsEdit Flag.
        /// </summary>
        public ReactiveCommand<Unit, Unit> SaveCommand { get; set; }

        /// <summary>
        ///     Opens to the SelectCategoryView
        /// </summary>
        public ReactiveCommand<Unit, Unit> GoToSelectCategoryDialogCommand { get; set; }

        /// <summary>
        ///     Cancels the operations.
        /// </summary>
        public ReactiveCommand<Unit, Unit> CancelCommand { get; set; }

        /// <summary>
        ///     Resets the CategoryViewModel of the currently selected PaymentViewModel
        /// </summary>
        public ReactiveCommand<Unit, Unit> ResetCategoryCommand { get; set; }

        /// <summary>
        ///     Indicates if the PaymentViewModel is a transfer.
        /// </summary>
        public bool IsTransfer => SelectedPayment.IsTransfer;

        /// <summary>
        ///     List with the different recurrence types.
        ///     This has to have the same order as the enum
        /// </summary>
        public List<PaymentRecurrence> RecurrenceList => new List<PaymentRecurrence>
        {
            PaymentRecurrence.Daily,
            PaymentRecurrence.DailyWithoutWeekend,
            PaymentRecurrence.Weekly,
            PaymentRecurrence.Biweekly,
            PaymentRecurrence.Monthly,
            PaymentRecurrence.Bimonthly,
            PaymentRecurrence.Quarterly,
            PaymentRecurrence.Biannually,
            PaymentRecurrence.Yearly
        };

        /// <summary>
        ///     The selected PaymentViewModel
        /// </summary>
        public PaymentViewModel SelectedPayment
        {
            get => selectedPayment;
            set => this.RaiseAndSetIfChanged(ref selectedPayment, value);
        }

        /// <summary>
        ///     Gives access to all accounts for Charged Dropdown list
        /// </summary>
        public ReadOnlyObservableCollection<AccountViewModel> ChargedAccounts => chargedAccounts;

        /// <summary>
        ///     Gives access to all accounts for Target Dropdown list
        /// </summary>
        public ReadOnlyObservableCollection<AccountViewModel> TargetAccounts => targetAccounts;

        public virtual string Title { get; set; }

        /// <summary>
        ///     Returns the Header for the AccountViewModel field
        /// </summary>
        public string AccountHeader
            => SelectedPayment?.Type == PaymentType.Income
                ? Strings.TargetAccountLabel
                : Strings.ChargedAccountLabel;

        protected abstract Task SavePayment();

        private async Task SavePaymentBase()
        {
            if (SelectedPayment.ChargedAccount == null)
            {
                await dialogService.ShowMessage(Strings.MandatoryFieldEmptyTitle, Strings.AccountRequiredMessage);
                return;
            }

            await SavePayment();

            settingsFacade.LastExecutionTimeStampSyncBackup = DateTime.Now;
#pragma warning disable 4014
            backupService.EnqueueBackupTask();
#pragma warning restore 4014
        }

        /// <summary>
        ///     Moved to own method for debugg reasons
        /// </summary>
        /// <param name="message">Message stent.</param>
        private void ReceiveMessage(CategorySelectedMessage message)
        {
            if (SelectedPayment == null || message == null) return;
            SelectedPayment.Category = message.SelectedCategory;
        }

        private void OpenSelectCategoryList()
        {
            HostScreen.Router.Navigate.Execute(new SelectCategoryListViewModel(HostScreen));
        }

        private void ResetSelection()
        {
            SelectedPayment.Category = null;
        }

        private void Cancel()
        {
            HostScreen.Router.NavigateBack.Execute();
        }

        //private void UpdateOtherComboBox()
        //{
        //    var tempCollection = new ObservableCollection<AccountViewModel>(ChargedAccounts);
        //    foreach (AccountViewModel account in TargetAccounts)
        //    {
        //        if (!tempCollection.Contains(account)) tempCollection.Add(account);
        //    }

        //    foreach (AccountViewModel account in tempCollection)
        //    {
        //        //fills targetaccounts
        //        if (!TargetAccounts.Contains(account)) TargetAccounts.Add(account);

        //        //fills chargedaccounts
        //        if (!ChargedAccounts.Contains(account)) ChargedAccounts.Add(account);
        //    }

        //    TargetAccounts.Remove(selectedPayment.ChargedAccount);
        //}
    }
}