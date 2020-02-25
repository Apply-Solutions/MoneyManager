﻿using System;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using MoneyFox.Application.Accounts.Queries.GetAccountById;
using MoneyFox.Application.Common.CloudBackup;
using MoneyFox.Application.Common.Facades;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Application.Payments.Commands.CreatePayment;
using MoneyFox.Application.Resources;
using MoneyFox.Domain;
using MoneyFox.Domain.Entities;
using MoneyFox.Domain.Exceptions;
using MoneyFox.Ui.Shared.Commands;
using MoneyFox.Ui.Shared.Utilities;
using MoneyFox.Uwp.Services;
using NLog;

namespace MoneyFox.Uwp.ViewModels
{
    public class AddPaymentViewModel : ModifyPaymentViewModel
    {
        private readonly ILogger logger = LogManager.GetCurrentClassLogger();

        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly NavigationService navigationService;
        private readonly IDialogService dialogService;

        public AddPaymentViewModel(IMediator mediator,
                                   IMapper mapper,
                                   IDialogService dialogService,
                                   ISettingsFacade settingsFacade,
                                   IBackupService backupService,
                                   NavigationService navigationService)
            : base(mediator, mapper, dialogService, settingsFacade, backupService, navigationService)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
        }

        public PaymentType PaymentType { get; set; }

        public AsyncCommand InitializeCommand => new AsyncCommand(InitializeAsync);

        protected override async Task InitializeAsync()
        {
            Title = PaymentTypeHelper.GetViewTitleForType(PaymentType, false);
            AmountString = HelperFunctions.FormatLargeNumbers(SelectedPayment.Amount);
            SelectedPayment.Type = PaymentType;

            await base.InitializeAsync();

            SelectedPayment.ChargedAccount = ChargedAccounts.FirstOrDefault();

            if (SelectedPayment.IsTransfer)
            {
                SelectedItemChangedCommand.Execute(null);
                SelectedPayment.TargetAccount = TargetAccounts.FirstOrDefault();
            }
        }

        protected override async Task SavePaymentAsync()
        {
            try
            {
                var payment = new Payment(SelectedPayment.Date,
                                          SelectedPayment.Amount,
                                          SelectedPayment.Type,
                                          await mediator.Send(new GetAccountByIdQuery(SelectedPayment.ChargedAccount.Id)),
                                          SelectedPayment.TargetAccount != null
                                              ? await mediator.Send(new GetAccountByIdQuery(SelectedPayment.TargetAccount.Id))
                                              : null,
                                          mapper.Map<Category>(SelectedPayment.Category),
                                          SelectedPayment.Note);

                if (SelectedPayment.IsRecurring)
                    payment.AddRecurringPayment(SelectedPayment.RecurringPayment.Recurrence, SelectedPayment.RecurringPayment.EndDate);

                await mediator.Send(new CreatePaymentCommand(payment));
                navigationService.GoBack();
            }
            catch (InvalidEndDateException)
            {
                await dialogService.ShowMessageAsync(Strings.InvalidEnddateTitle, Strings.InvalidEnddateMessage);
            }
            catch (Exception ex)
            {
                logger.Error(ex);
                throw;
            }
        }
    }
}