﻿using System;
using System.Threading.Tasks;
using MoneyFox.BusinessLogic;
using MoneyFox.BusinessLogic.PaymentActions;
using MoneyFox.DataLayer;
using MoneyFox.DataLayer.Entities;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.Interfaces;
using MoneyFox.ServiceLayer.ViewModels;

namespace MoneyFox.ServiceLayer.Services
{
    public interface IPaymentService
    {
        Task<OperationResult> SavePayment(PaymentViewModel paymentViewModel);

        Task<OperationResult> UpdatePayment(PaymentViewModel newPaymentViewModel);

        Task<OperationResult> DeletePayment(PaymentViewModel paymentViewModel);
    }

    public class PaymentService : IPaymentService
    {
        private readonly EfCoreContext context;
        private readonly IModifyPaymentAction modifyPaymentAction;
        private readonly IDialogService dialogService;


        public PaymentService(EfCoreContext context, IModifyPaymentAction modifyPaymentAction, IDialogService dialogService)
        {
            this.modifyPaymentAction = modifyPaymentAction;
            this.dialogService = dialogService;
            this.context = context;
        }

        public async Task<OperationResult> SavePayment(PaymentViewModel paymentViewModel)
        {
            var payment = await CreatePaymentFromViewModel(paymentViewModel).ConfigureAwait(false);

            var result = await modifyPaymentAction.AddPayment(payment)
                .ConfigureAwait(false);

            await context.SaveChangesAsync()
                .ConfigureAwait(false);

            return !result.Success
                ? OperationResult.Failed(result.Message)
                : OperationResult.Succeeded();
        }

        public async Task<OperationResult> UpdatePayment(PaymentViewModel newPaymentViewModel)
        {
            var payment = await CreatePaymentFromViewModel(newPaymentViewModel).ConfigureAwait(false);

            var result = await modifyPaymentAction.UpdatePayment(newPaymentViewModel.Id, payment)
                .ConfigureAwait(false);

            await context.SaveChangesAsync()
                .ConfigureAwait(false);

            return !result.Success
                ? OperationResult.Failed(result.Message)
                : OperationResult.Succeeded();
        }

        public async Task<OperationResult> DeletePayment(PaymentViewModel paymentViewModel)
        {
            if (!await dialogService.ShowConfirmMessage(Strings.DeleteTitle, Strings.DeletePaymentConfirmationMessage)
                .ConfigureAwait(false)) return OperationResult.Succeeded();

            if (paymentViewModel.IsRecurring
                && await dialogService
                    .ShowConfirmMessage(Strings.DeleteRecurringPaymentTitle, Strings.DeleteRecurringPaymentMessage)
                    .ConfigureAwait(false))
            {
                await modifyPaymentAction.DeleteRecurringPayment(paymentViewModel.RecurringPayment.Id)
                    .ConfigureAwait(false);
            }
            
            var result = await modifyPaymentAction.DeletePayment(paymentViewModel.Id)
                .ConfigureAwait(false);

            await context.SaveChangesAsync()
                .ConfigureAwait(false);

            return !result.Success
                ? OperationResult.Failed(result.Message)
                : OperationResult.Succeeded();
        }

        private async Task<Payment> CreatePaymentFromViewModel(PaymentViewModel paymentViewModel)
        {

            var chargedAccount = await context.Accounts
                .FindAsync(paymentViewModel.ChargedAccount.Id)
                .ConfigureAwait(false);

            Account targetAccount = null;
            if (paymentViewModel.TargetAccount != null)
                targetAccount = await context.Accounts
                    .FindAsync(paymentViewModel.TargetAccount.Id)
                    .ConfigureAwait(false);

            Category category = null;
            if (paymentViewModel.Category != null)
                category = await context.Categories
                    .FindAsync(paymentViewModel.Category.Id)
                    .ConfigureAwait(false);

            var payment = new Payment(paymentViewModel.Date, paymentViewModel.Amount, paymentViewModel.Type,
                chargedAccount,
                targetAccount, category, paymentViewModel.Note);
            try
            {
                if (paymentViewModel.IsRecurring)
                    payment.AddRecurringPayment(paymentViewModel.RecurringPayment.Recurrence,
                        paymentViewModel.RecurringPayment.IsEndless
                            ? null
                            : paymentViewModel.RecurringPayment.EndDate);

                return payment;
            }
            catch (Exception)
            {
                payment.ChargedAccount.RemovePaymentAmount(payment);
                payment.TargetAccount?.RemovePaymentAmount(payment);

                throw;
            }
        }
    }
}