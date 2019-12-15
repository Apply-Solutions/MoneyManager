﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MoneyFox.Application.Common;
using MoneyFox.BusinessDbAccess.PaymentActions;
using MoneyFox.Domain.Entities;

namespace MoneyFox.BusinessLogic.PaymentActions
{
    /// <summary>
    ///     Provides different operations for recurring payment.
    /// </summary>
    public interface IRecurringPaymentAction
    {
        Task CreatePaymentsUpToRecur();
    }

    public class RecurringPaymentAction : IRecurringPaymentAction
    {
        private readonly IRecurringPaymentDbAccess recurringPaymentDbAccess;

        public RecurringPaymentAction(IRecurringPaymentDbAccess recurringPaymentDbAccess)
        {
            this.recurringPaymentDbAccess = recurringPaymentDbAccess;
        }

        public async Task CreatePaymentsUpToRecur()
        {
            List<RecurringPayment> recurringPayments = await recurringPaymentDbAccess.GetRecurringPaymentsAsync();

            List<Payment> recPaymentsToCreate = recurringPayments
                                                .Where(x => x.RelatedPayments.Any())
                                                .Where(x => RecurringPaymentHelper
                                                           .CheckIfRepeatable(x.RelatedPayments
                                                                               .OrderByDescending(d => d.Date)
                                                                               .First()))
                                                .Select(x => new Payment(
                                                            RecurringPaymentHelper.GetPaymentDateFromRecurring(x),
                                                            x.Amount,
                                                            x.Type,
                                                            x.ChargedAccount,
                                                            x.TargetAccount,
                                                            x.Category,
                                                            x.Note,
                                                            x))
                                                .ToList();

            await recurringPaymentDbAccess.SaveNewPaymentsAsync(recPaymentsToCreate);
        }
    }
}
