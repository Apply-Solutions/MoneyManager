﻿using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using MoneyFox.Shared.Groups;
using MoneyFox.Shared.Helpers;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Interfaces.ViewModels;
using MoneyFox.Shared.Model;
using MoneyFox.Shared.Resources;
using MvvmCross.Core.ViewModels;
using PropertyChanged;

namespace MoneyFox.Shared.ViewModels
{
    [ImplementPropertyChanged]
    public class PaymentListViewModel : BaseViewModel, IPaymentListViewModel, IDisposable
    {
        private readonly IDialogService dialogService;
        private readonly IPaymentManager paymentManager;
        private readonly IUnitOfWork unitOfWork;

        public PaymentListViewModel(IUnitOfWork unitOfWork,
            IDialogService dialogService, IPaymentManager paymentManager)
        {
            this.unitOfWork = unitOfWork;
            this.dialogService = dialogService;
            this.paymentManager = paymentManager;
        }

        public bool IsPaymentsEmtpy => RelatedPayments != null && !RelatedPayments.Any();

        public int AccountId { get; private set; }

        public void Dispose()
        {
            unitOfWork.Dispose();
        }

        public IBalanceViewModel BalanceViewModel { get; private set; }

        /// <summary>
        ///     Loads the data for this view.
        /// </summary>
        public virtual MvxCommand LoadCommand => new MvxCommand(LoadPayments);

        /// <summary>
        ///     Navigate to the add payment view.
        /// </summary>
        public MvxCommand<string> GoToAddPaymentCommand => new MvxCommand<string>(GoToAddPayment);

        /// <summary>
        ///     Deletes the current account and updates the balance.
        /// </summary>
        public MvxCommand DeleteAccountCommand => new MvxCommand(DeleteAccount);

        /// <summary>
        ///     Edits the passed payment.
        /// </summary>
        public MvxCommand<Payment> EditCommand { get; private set; }

        /// <summary>
        ///     Deletes the passed payment.
        /// </summary>
        public MvxCommand<Payment> DeletePaymentCommand => new MvxCommand<Payment>(DeletePayment);

        /// <summary>
        ///     Returns all Payment who are assigned to this repository
        ///     This has to stay until the android list with headers is implemented.
        ///     Currently only used for Android
        /// </summary>
        public ObservableCollection<Payment> RelatedPayments { get; set; }

        /// <summary>
        ///     Returns groupped related payments
        /// </summary>
        public ObservableCollection<DateListGroup<Payment>> Source { get; set; }

        /// <summary>
        ///     Returns the name of the account title for the current page
        /// </summary>
        public string Title => unitOfWork.AccountRepository.FindById(AccountId).Name;

        public void Init(int id)
        {
            AccountId = id;
            BalanceViewModel = new PaymentListBalanceViewModel(unitOfWork, AccountId);
        }

        private void LoadPayments()
        {
            EditCommand = null;
            //Refresh balance control with the current account
            BalanceViewModel.UpdateBalanceCommand.Execute();

            RelatedPayments = new ObservableCollection<Payment>(unitOfWork.PaymentRepository.Data
                .Where(x => x.ChargedAccountId == AccountId || x.TargetAccountId == AccountId)
                .OrderByDescending(x => x.Date)
                .ToList());

            foreach (var payment in RelatedPayments)
            {
                payment.CurrentAccountId = AccountId;
            }

            Source = new ObservableCollection<DateListGroup<Payment>>(
                DateListGroup<Payment>.CreateGroups(RelatedPayments,
                    CultureInfo.CurrentUICulture,
                    s => s.Date.ToString("MMMM", CultureInfo.InvariantCulture) + " " + s.Date.Year,
                    s => s.Date, true));

            //We have to set the command here to ensure that the selection changed event is triggered earlier
            EditCommand = new MvxCommand<Payment>(Edit);
        }

        // TODO: Use the actual enum rather than magic strings - Seth Bartlett 7/1/2016 12:07PM
        private void GoToAddPayment(string paymentType)
        {
            ShowViewModel<ModifyPaymentViewModel>(
                new {type = (PaymentType) Enum.Parse(typeof(PaymentType), paymentType)});
        }

        // TODO: I'm pretty sure this shouldn't exist in this ViewModel - Seth Bartlett 7/1/2016 12:06PM
        // This may actually exist from the buttons at the bottom right of the view, if so, this view should be separated out. - Seth Bartlett 7/1/2016 2:31AM
        private async void DeleteAccount()
        {
            if (await dialogService.ShowConfirmMessage(Strings.DeleteTitle, Strings.DeleteAccountConfirmationMessage))
            {
                if (unitOfWork.AccountRepository.Delete(unitOfWork.AccountRepository.FindById(AccountId)))
                    SettingsHelper.LastDatabaseUpdate = DateTime.Now;
                BalanceViewModel.UpdateBalanceCommand.Execute();
                Close(this);
            }
        }

        private void Edit(Payment payment)
        {
            ShowViewModel<ModifyPaymentViewModel>(new {paymentId = payment.Id});
        }

        private async void DeletePayment(Payment payment)
        {
            if (!await
                dialogService.ShowConfirmMessage(Strings.DeleteTitle, Strings.DeletePaymentConfirmationMessage))
            {
                return;
            }

            if (await paymentManager.CheckRecurrenceOfPayment(payment))
            {
                paymentManager.RemoveRecurringForPayment(payment);
                unitOfWork.RecurringPaymentRepository.Delete(payment.RecurringPayment);
            }

            var accountSucceded = paymentManager.RemovePaymentAmount(payment);
            var paymentSucceded = unitOfWork.PaymentRepository.Delete(payment);
            if (accountSucceded && paymentSucceded)
                SettingsHelper.LastDatabaseUpdate = DateTime.Now;
            LoadCommand.Execute();
        }
    }
}