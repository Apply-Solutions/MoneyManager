﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Cirrious.MvvmCross.ViewModels;
using MoneyManager.Core.Helpers;
using MoneyManager.Core.ViewModels.CategoryList;
using MoneyManager.Foundation;
using MoneyManager.Foundation.Interfaces;
using MoneyManager.Foundation.Messages;
using MoneyManager.Foundation.Model;
using MoneyManager.Localization;
using MvvmCross.Plugins.Messenger;
using PropertyChanged;

namespace MoneyManager.Core.ViewModels
{
    [ImplementPropertyChanged]
    public class ModifyTransactionViewModel : BaseViewModel
    {
        private readonly IAccountRepository accountRepository;
        private readonly IDefaultManager defaultManager;
        private readonly IDialogService dialogService;
        private readonly ITransactionManager transactionManager;
        private readonly ITransactionRepository transactionRepository;

        //this token ensures that we will be notified when a message is sent.
        private readonly MvxSubscriptionToken token;

        public ModifyTransactionViewModel(ITransactionRepository transactionRepository,
            IAccountRepository accountRepository,
            IDialogService dialogService,
            ITransactionManager transactionManager,
            IDefaultManager defaultManager)
        {
            this.transactionRepository = transactionRepository;
            this.dialogService = dialogService;
            this.transactionManager = transactionManager;
            this.defaultManager = defaultManager;
            this.accountRepository = accountRepository;

            token = MessageHub.Subscribe<CategorySelectedMessage>(message => SelectedTransaction.Category = message.SelectedCategory);
        }

        /// <summary>
        ///     Init the view. Is executed after the constructor call
        /// </summary>
        /// <param name="typeString">Type of the transaction.</param>
        /// <param name="isEdit">Weather the transaction is in edit mode or not.</param>
        public void Init(string typeString, bool isEdit = false)
        {
            IsEdit = isEdit;
            IsEndless = true;

            amount = 0;

            if (IsEdit)
            {
                PrepareEdit();
            } 
            else
            {
                PrepareDefault(typeString);
            }
        }

        private void PrepareEdit()
        {
            // Monkey patch for issues with binding to the account selection
            // TODO: fix this that the binding works without this.
            SelectedTransaction.ChargedAccount =
                accountRepository.Data.FirstOrDefault(x => x.Id == SelectedTransaction.ChargedAccountId);

            IsTransfer = SelectedTransaction.IsTransfer;
            // set the private amount property. This will get properly formatted and then displayed.
            amount = SelectedTransaction.Amount;
            Recurrence = SelectedTransaction.IsRecurring
                ? SelectedTransaction.RecurringTransaction.Recurrence
                : 0;
            EndDate = SelectedTransaction.IsRecurring
                ? SelectedTransaction.RecurringTransaction.EndDate
                : DateTime.Now;
            IsEndless = !SelectedTransaction.IsRecurring || SelectedTransaction.RecurringTransaction.IsEndless;

        }

        private void PrepareDefault(string typeString)
        {
            var type = (TransactionType) Enum.Parse(typeof (TransactionType), typeString);

            SetDefaultTransaction(type);
            SelectedTransaction.ChargedAccount = defaultManager.GetDefaultAccount();
            IsTransfer = type == TransactionType.Transfer;
            EndDate = DateTime.Now;
        }

        private void SetDefaultTransaction(TransactionType transactionType)
        {
            SelectedTransaction = new FinancialTransaction
            {
                Type = (int) transactionType,
                Date = DateTime.Now,
                // Assign empty category to reset the GUI
                Category = new Category()
            };
        }

        private async void Save()
        {
            if (SelectedTransaction.ChargedAccount == null)
            {
                ShowAccountRequiredMessage();
                return;
            }

            if (SelectedTransaction.IsRecurring && !IsEndless && EndDate.Date <= DateTime.Today)
            {
                ShowInvalidEndDateMessage();
                return;
            }

            // Make sure that the old amount is removed to not count the amount twice.
            RemoveOldAmount();
            SelectedTransaction.Amount = amount;

            //Create a recurring transaction based on the financial transaction or update an existing
            await PrepareRecurringTransaction();

            // SaveItem or update the transaction and add the amount to the account
            transactionRepository.Save(SelectedTransaction);
            accountRepository.AddTransactionAmount(SelectedTransaction);

            Close(this);
        }

        private void RemoveOldAmount()
        {
            if (IsEdit)
            {
                accountRepository.RemoveTransactionAmount(SelectedTransaction);
            }
        }

        private async Task PrepareRecurringTransaction()
        {
            if ((IsEdit && await transactionManager.CheckForRecurringTransaction(SelectedTransaction))
                || SelectedTransaction.IsRecurring)
            {
                SelectedTransaction.RecurringTransaction = RecurringTransactionHelper.
                    GetRecurringFromFinancialTransaction(SelectedTransaction,
                        IsEndless,
                        Recurrence,
                        EndDate);
            }
        }

        private void OpenSelectCategoryList()
        {
            ShowViewModel<SelectCategoryListViewModel>();
        }

        private async void Delete()
        {
            if (
                await
                    dialogService.ShowConfirmMessage(Strings.DeleteTitle, Strings.DeleteTransactionConfirmationMessage))
            {
                transactionRepository.Delete(transactionRepository.Selected);
                accountRepository.RemoveTransactionAmount(SelectedTransaction);
                Close(this);
            }
        }

        private async void ShowAccountRequiredMessage()
        {
            await dialogService.ShowMessage(Strings.MandatoryFieldEmptyTitle,
                Strings.AccountRequiredMessage);
        }

        private async void ShowInvalidEndDateMessage()
        {
            await dialogService.ShowMessage(Strings.InvalidEnddateTitle,
                Strings.InvalidEnddateMessage);
        }


        private void ResetSelection()
        {
            SelectedTransaction.Category = null;
        }


        private void Cancel()
        {
            Close(this);
        }

        #region Commands

        /// <summary>
        ///     Saves the transaction or updates the existing depending on the IsEdit Flag.
        /// </summary>
        public IMvxCommand SaveCommand => new MvxCommand(Save);

        /// <summary>
        ///     Opens to the SelectCategoryView
        /// </summary>
        public IMvxCommand GoToSelectCategorydialogCommand => new MvxCommand(OpenSelectCategoryList);

        /// <summary>
        ///     Delets the transaction or updates the existing depending on the IsEdit Flag.
        /// </summary>
        public IMvxCommand DeleteCommand => new MvxCommand(Delete);

        /// <summary>
        ///     Cancels the operations.
        /// </summary>
        public IMvxCommand CancelCommand => new MvxCommand(Cancel);

        /// <summary>
        ///     Resets the category of the currently selected transaction
        /// </summary>
        public IMvxCommand ResetCategoryCommand => new MvxCommand(ResetSelection);

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates if the view is in Edit mode.
        /// </summary>
        public bool IsEdit { get; private set; }

        /// <summary>
        ///     Indicates if the transaction is a transfer.
        /// </summary>
        public bool IsTransfer { get; private set; }

        /// <summary>
        ///     Indicates if the reminder is endless
        /// </summary>
        public bool IsEndless { get; set; }

        /// <summary>
        ///     The Enddate for recurring transaction
        /// </summary>
        public DateTime EndDate { get; set; }

        /// <summary>
        ///     The selected recurrence
        /// </summary>
        public int Recurrence { get; set; }

        // This has to be static in order to keep the value even if you leave the page to select a category.
        private double amount;

        /// <summary>
        ///     Property to format amount string to double with the proper culture.
        ///     This is used to prevent issues when converting the amount string to double
        ///     without the correct culture.
        /// </summary>
        public string AmountString
        {
            get { return Utilities.FormatLargeNumbers(amount); }
            set
            {
                double convertedValue;
                if (double.TryParse(value, out convertedValue))
                {
                    amount = convertedValue;
                }
            }
        }

        /// <summary>
        ///     List with the different recurrence types.
        /// </summary>
        public List<string> RecurrenceList => new List<string>
        {
            Strings.DailyLabel,
            Strings.DailyWithoutWeekendLabel,
            Strings.WeeklyLabel,
            Strings.BiweeklyLabel,
            Strings.MonthlyLabel,
            Strings.YearlyLabel
        };

        /// <summary>
        ///     The selected transaction
        /// </summary>
        public FinancialTransaction SelectedTransaction
        {
            get { return transactionRepository.Selected; }
            set { transactionRepository.Selected = value; }
        }

        /// <summary>
        ///     Gives access to all accounts
        /// </summary>
        public ObservableCollection<Account> AllAccounts => accountRepository.Data;

        /// <summary>
        ///     Returns the Title for the page
        /// </summary>
        public string Title => TransactionTypeHelper.GetViewTitleForType(SelectedTransaction.Type, IsEdit);

        /// <summary>
        ///     Returns the Header for the account field
        /// </summary>
        public string AccountHeader
            => SelectedTransaction?.Type == (int) TransactionType.Income
                ? Strings.TargetAccountLabel
                : Strings.ChargedAccountLabel;

        /// <summary>
        ///     The transaction date
        /// </summary>
        public DateTime Date
        {
            get
            {
                if (!IsEdit && SelectedTransaction.Date == DateTime.MinValue)
                {
                    SelectedTransaction.Date = DateTime.Now;
                }
                return SelectedTransaction.Date;
            }
            set { SelectedTransaction.Date = value; }
        }

        #endregion
    }
}