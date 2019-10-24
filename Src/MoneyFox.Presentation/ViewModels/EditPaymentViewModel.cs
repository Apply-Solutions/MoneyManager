﻿using System;
using System.Threading.Tasks;
using AutoMapper;
using GalaSoft.MvvmLight.Views;
using MediatR;
using MoneyFox.Application.Payments.Queries.GetPaymentById;
using MoneyFox.Application.Resources;
using MoneyFox.Domain.Exceptions;
using MoneyFox.Presentation.Commands;
using MoneyFox.Presentation.Facades;
using MoneyFox.Presentation.Services;
using MoneyFox.Presentation.Utilities;
using IDialogService = MoneyFox.Presentation.Interfaces.IDialogService;

namespace MoneyFox.Presentation.ViewModels
{
    public class EditPaymentViewModel : ModifyPaymentViewModel
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly IPaymentService paymentService;
        private readonly INavigationService navigationService;
        private readonly IBackupService backupService;
        private readonly IDialogService dialogService;

        public EditPaymentViewModel(IMediator mediator,
                                    IMapper mapper,
                                    IPaymentService paymentService,
                                    IDialogService dialogService,
                                    ISettingsFacade settingsFacade,
                                    IBackupService backupService,
                                    INavigationService navigationService)
            : base(mediator, mapper, dialogService, settingsFacade, backupService, navigationService)
        {
            this.mediator = mediator;
            this.mapper = mapper;
            this.paymentService = paymentService;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.backupService = backupService;
            this.paymentService = paymentService;
        }

        public int PaymentId { get; set; }

        /// <summary>
        ///     Delete the selected CategoryViewModel from the database
        /// </summary>
        public AsyncCommand DeleteCommand => new AsyncCommand(DeletePayment);

        public AsyncCommand InitializeCommand => new AsyncCommand(Initialize);

        protected override async Task Initialize()
        {
            await base.Initialize();

            SelectedPayment = mapper.Map<PaymentViewModel>(await mediator.Send(new GetPaymentByIdQuery(PaymentId)));

            // We have to set this here since otherwise the end date is null. This causes a crash on android.
            // Also it's user unfriendly if you the default end date is the 1.1.0001.
            if (SelectedPayment.IsRecurring && SelectedPayment.RecurringPayment.IsEndless) SelectedPayment.RecurringPayment.EndDate = DateTime.Today;

            Title = PaymentTypeHelper.GetViewTitleForType(SelectedPayment.Type, true);
        }

        protected override async Task SavePayment()
        {
            try
            {
                await paymentService.UpdatePayment(SelectedPayment);
                navigationService.GoBack();
            }
            catch (InvalidEndDateException)
            {
                await dialogService.ShowMessage(Strings.InvalidEnddateTitle, Strings.InvalidEnddateMessage);
            }
        }

        private async Task DeletePayment()
        {
            await paymentService.DeletePayment(SelectedPayment);
#pragma warning disable 4014
            backupService.EnqueueBackupTask();
#pragma warning restore 4014
            CancelCommand.Execute(null);
        }
    }
}
