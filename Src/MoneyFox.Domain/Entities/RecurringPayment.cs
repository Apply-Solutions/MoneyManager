using MoneyFox.Domain.Exceptions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MoneyFox.Domain.Entities
{
    public class RecurringPayment
    {
        /// <summary>
        /// EF Core constructor
        /// </summary>
        private RecurringPayment()
        {
        }

        public RecurringPayment(DateTime startDate,
                                decimal amount,
                                PaymentType type,
                                PaymentRecurrence recurrence,
                                Account chargedAccount,
                                string note = "",
                                DateTime? endDate = null,
                                Account? targetAccount = null,
                                Category? category = null,
                                DateTime? lastRecurrenceCreated = null)
        {
            if(!IsEndless && endDate != null && endDate < DateTime.Today)
            {
                throw new InvalidEndDateException();
            }

            ChargedAccount = chargedAccount ?? throw new ArgumentNullException(nameof(chargedAccount));
            StartDate = startDate;
            EndDate = endDate;
            Amount = amount;
            Type = type;
            Recurrence = recurrence;
            Note = note;
            Category = category;
            TargetAccount = targetAccount;
            IsEndless = endDate == null;

            LastRecurrenceCreated = lastRecurrenceCreated ?? DateTime.Now;
            ModificationDate = DateTime.Now;
            CreationTime = DateTime.Now;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public DateTime StartDate { get; private set; }

        public DateTime? EndDate { get; private set; }

        public bool IsEndless { get; private set; }

        public decimal Amount { get; private set; }

        public PaymentType Type { get; private set; }

        public PaymentRecurrence Recurrence { get; private set; }

        public string? Note { get; set; }

        public DateTime LastRecurrenceCreated { get; private set; }

        public DateTime ModificationDate { get; private set; }

        public DateTime CreationTime { get; private set; }

        public virtual Category? Category { get; private set; }

        [Required]
        public virtual Account ChargedAccount { get; private set; } = null!;

        public virtual Account? TargetAccount { get; private set; }

        public virtual List<Payment> RelatedPayments { get; private set; } = new List<Payment>();

        public void UpdateRecurringPayment(decimal amount,
                                           PaymentRecurrence recurrence,
                                           Account chargedAccount,
                                           string note = "",
                                           DateTime? endDate = null,
                                           Account? targetAccount = null,
                                           Category? category = null)
        {
            if(!IsEndless && endDate != null && endDate < DateTime.Today)
            {
                throw new InvalidEndDateException();
            }

            ChargedAccount = chargedAccount ?? throw new ArgumentNullException(nameof(chargedAccount));
            EndDate = endDate;
            Amount = amount;
            Recurrence = recurrence;
            Note = note;
            Category = category;
            TargetAccount = targetAccount;
            IsEndless = endDate == null;
            ModificationDate = DateTime.Now;
        }

        public void SetLastRecurrenceCreatedDate()
        {
            LastRecurrenceCreated = DateTime.Now;
            ModificationDate = DateTime.Now;
        }
    }
}
