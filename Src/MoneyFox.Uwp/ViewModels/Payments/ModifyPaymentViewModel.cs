using AutoMapper;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MediatR;
using MoneyFox.Application.Accounts.Queries.GetAccounts;
using MoneyFox.Application.Categories.Queries.GetCategoryById;
using MoneyFox.Application.Categories.Queries.GetCategoryBySearchTerm;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Application.Common.Messages;
using MoneyFox.Application.Resources;
using MoneyFox.Domain;
using MoneyFox.Domain.Exceptions;
using MoneyFox.Messages;
using MoneyFox.Ui.Shared.ViewModels.Accounts;
using MoneyFox.Ui.Shared.ViewModels.Categories;
using MoneyFox.Ui.Shared.ViewModels.Payments;
using MoneyFox.Uwp.Services;
using MoneyFox.Uwp.Views.Categories;
using MoneyFox.Uwp.Views.Payments;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Threading.Tasks;

#nullable enable
namespace MoneyFox.Uwp.ViewModels.Payments
{
    /// <summary>
    /// Handles the logic of the ModifyPayment view
    /// </summary>
    public abstract class ModifyPaymentViewModel : ViewModelBase, IModifyPaymentViewModel
    {
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private readonly IMapper mapper;
        private readonly IMediator mediator;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;
        private ObservableCollection<AccountViewModel> chargedAccounts = new ObservableCollection<AccountViewModel>();

        private PaymentRecurrence recurrence;
        private PaymentViewModel selectedPayment = new PaymentViewModel();
        private ObservableCollection<AccountViewModel> targetAccounts = new ObservableCollection<AccountViewModel>();
        private string title = Strings.AddPaymentLabel;

        /// <summary>
        /// Default constructor
        /// </summary>
        protected ModifyPaymentViewModel(IMediator mediator,
                                         IMapper mapper,
                                         IDialogService dialogService,
                                         INavigationService navigationService)
        {
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.mediator = mediator;
            this.mapper = mapper;
        }

        /// <summary>
        /// Updates the targetAccountViewModel and chargedAccountViewModel Comboboxes' dropdown lists.
        /// </summary>
        public RelayCommand SelectedItemChangedCommand => new RelayCommand(UpdateOtherComboBox);

        /// <summary>
        ///     Opens the create category dialog.
        /// </summary>
        public RelayCommand AddNewCategoryCommand
            => new RelayCommand(async () => await new AddCategoryDialog().ShowAsync());

        /// <summary>
        /// Saves the PaymentViewModel or updates the existing depending on the IsEdit Flag.
        /// </summary>
        public RelayCommand SaveCommand => new RelayCommand(async () => await SavePaymentBaseAsync());

        /// <inheritdoc />
        public RelayCommand CancelCommand => new RelayCommand(Cancel);

        /// <inheritdoc />
        public RelayCommand GoToSelectCategoryDialogCommand => new RelayCommand(async ()
            => await new SelectCategoryDialog { RequestedTheme = ThemeSelectorService.Theme }.ShowAsync());

        /// <summary>
        /// Resets the CategoryViewModel of the currently selected PaymentViewModel
        /// </summary>
        public RelayCommand ResetCategoryCommand => new RelayCommand(ResetSelection);

        public List<PaymentType> PaymentTypeList => new List<PaymentType>
        {
            PaymentType.Expense,
            PaymentType.Income,
            PaymentType.Transfer
        };

        /// <summary>
        /// The selected recurrence
        /// </summary>
        public PaymentRecurrence Recurrence
        {
            get => recurrence;
            set
            {
                if(recurrence == value)
                {
                    return;
                }

                recurrence = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// List with the different recurrence types.     This has to have the same order as the enum
        /// </summary>
        public List<PaymentRecurrence> RecurrenceList
                                       => new List<PaymentRecurrence>
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
        /// The selected PaymentViewModel
        /// </summary>
        public PaymentViewModel SelectedPayment
        {
            get => selectedPayment;
            set
            {
                if(selectedPayment == value)
                {
                    return;
                }

                selectedPayment = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(AccountHeader));
                RaisePropertyChanged(nameof(AmountString));
            }
        }

        private string amountString = "";

        public string AmountString
        {
            get => amountString;
            set
            {
                if(amountString == value)
                {
                    return;
                }

                amountString = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gives access to all accounts for Charged Dropdown list
        /// </summary>
        public ObservableCollection<AccountViewModel> ChargedAccounts
        {
            get => chargedAccounts;
            private set
            {
                chargedAccounts = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gives access to all accounts for Target Dropdown list
        /// </summary>
        public ObservableCollection<AccountViewModel> TargetAccounts
        {
            get => targetAccounts;
            private set
            {
                targetAccounts = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<CategoryViewModel> categories = new ObservableCollection<CategoryViewModel>();
        public ObservableCollection<CategoryViewModel> Categories
        {
            get => categories;
            private set
            {
                categories = value;
                RaisePropertyChanged();
            }
        }

        public virtual string Title
        {
            get => title;
            set
            {
                if(title == value)
                {
                    return;
                }

                title = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Returns the Header for the AccountViewModel field
        /// </summary>
        public string AccountHeader
                      => SelectedPayment?.Type == PaymentType.Income
                         ? Strings.TargetAccountLabel
                         : Strings.ChargedAccountLabel;

        protected abstract Task SavePaymentAsync();

        protected virtual async Task InitializeAsync()
        {
            List<AccountViewModel> accounts = mapper.Map<List<AccountViewModel>>(await mediator.Send(new GetAccountsQuery()));

            ChargedAccounts = new ObservableCollection<AccountViewModel>(accounts);
            TargetAccounts = new ObservableCollection<AccountViewModel>(accounts);
            Categories = new ObservableCollection<CategoryViewModel>(mapper.Map<List<CategoryViewModel>>(await mediator.Send(new GetCategoryBySearchTermQuery())));
        }

        public void Subscribe() => MessengerInstance.Register<CategorySelectedMessage>(this, async message => await ReceiveMessageAsync(message));

        public void Unsubscribe() => MessengerInstance.Unregister<CategorySelectedMessage>(this, async message => await ReceiveMessageAsync(message));

        private async Task SavePaymentBaseAsync()
        {
            if(SelectedPayment.ChargedAccount == null)
            {
                await dialogService.ShowMessageAsync(Strings.MandatoryFieldEmptyTitle, Strings.AccountRequiredMessage);
                return;
            }

            if(decimal.TryParse(AmountString, NumberStyles.Any, CultureInfo.CurrentCulture, out decimal convertedValue))
            {
                SelectedPayment.Amount = convertedValue;
            }
            else
            {
                logger.Warn($"Amount string {AmountString} could not be parsed to double.");
                await dialogService.ShowMessageAsync(Strings.InvalidNumberTitle, Strings.InvalidNumberCurrentBalanceMessage);
                return;
            }

            if(SelectedPayment.Amount < 0)
            {
                await dialogService.ShowMessageAsync(Strings.AmountMayNotBeNegativeTitle, Strings.AmountMayNotBeNegativeMessage);
                return;
            }

            if(SelectedPayment.Category != null
                && SelectedPayment.Category.RequireNote
                && string.IsNullOrEmpty(SelectedPayment.Note))
            {
                await dialogService.ShowMessageAsync(Strings.MandatoryFieldEmptyTitle, Strings.ANoteForPaymentIsRequired);
                return;
            }

            if(SelectedPayment.IsRecurring && !SelectedPayment.RecurringPayment!.IsEndless
                                           && SelectedPayment.RecurringPayment.EndDate != null
                                           && SelectedPayment.RecurringPayment.EndDate < DateTime.Now)
            {
                await dialogService.ShowMessageAsync(Strings.InvalidEnddateTitle, Strings.InvalidEnddateMessage);
                return;
            }

            try
            {
                await dialogService.ShowLoadingDialogAsync(Strings.SavingPaymentMessage);
                await SavePaymentAsync();
                MessengerInstance.Send(new ReloadMessage());
                navigationService.GoBack();
            }
            catch(Exception ex)
            {
                logger.Error(ex);
                throw;
            }
            finally
            {
                await dialogService.HideLoadingDialogAsync();
            }
        }

        public void Cancel() => navigationService.GoBack();

        private async Task ReceiveMessageAsync(CategorySelectedMessage message)
        {
            if(SelectedPayment == null || message == null)
            {
                return;
            }

            SelectedPayment.Category = mapper.Map<CategoryViewModel>(await mediator.Send(new GetCategoryByIdQuery(message.CategoryId)));
        }

        private void ResetSelection() => SelectedPayment.Category = null;

        private void UpdateOtherComboBox()
        {
            var tempCollection = new ObservableCollection<AccountViewModel>(ChargedAccounts);
            foreach(AccountViewModel account in TargetAccounts)
            {
                if(!tempCollection.Contains(account))
                {
                    tempCollection.Add(account);
                }
            }

            foreach(AccountViewModel account in tempCollection)
            {
                //fills target accounts
                if(!TargetAccounts.Contains(account))
                {
                    TargetAccounts.Add(account);
                }

                //fills charged accounts
                if(!ChargedAccounts.Contains(account))
                {
                    ChargedAccounts.Add(account);
                }
            }

            TargetAccounts.Remove(selectedPayment.ChargedAccount);
        }
    }
}
