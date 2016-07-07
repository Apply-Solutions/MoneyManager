﻿using System;
using System.Linq;
using MoneyFox.Shared.Helpers;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Model;

namespace MoneyFox.Shared.Manager
{
    public class RecurringPaymentManager : IRecurringPaymentManager
    {
        private readonly IAccountRepository accountRepository;
        private readonly IPaymentRepository paymentRepository;

        public RecurringPaymentManager(IPaymentRepository paymentRepository,
            IAccountRepository accountRepository)
        {
            this.paymentRepository = paymentRepository;
            this.accountRepository = accountRepository;
        }

        /// <summary>
        ///     Checks if one of the recurring payment has to be repeated
        /// </summary>
        public void CheckRecurringPayments()
        {
            var paymentList = paymentRepository.LoadRecurringList();

            foreach (var payment in paymentList.Where(x => x.ChargedAccount != null))
            {
                var relatedPayment = GetLastOccurence(payment);

                if (RecurringPaymentHelper.CheckIfRepeatable(payment.RecurringPayment, relatedPayment))
                {
                    var newPayment = RecurringPaymentHelper.GetPaymentFromRecurring(payment.RecurringPayment);

                    var paymentSucceded = paymentRepository.Save(newPayment);
                    var accountSucceded = accountRepository.AddPaymentAmount(newPayment);
                    if (paymentSucceded && accountSucceded)
                        SettingsHelper.LastDatabaseUpdate = DateTime.Now;
                }
            }
        }

        private Payment GetLastOccurence(Payment payment)
        {
            var transcationList = paymentRepository.Data
                .Where(x => x.RecurringPaymentId == payment.RecurringPaymentId)
                .OrderBy(x => x.Date)
                .ToList();

            return transcationList.LastOrDefault();
        }
    }
}