﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MoneyFox.Shared.Model;
using PropertyChanged;
using SQLite.Net;

namespace MoneyFox.Shared.DataAccess
{
    /// <summary>
    ///     Handles the access to the Payments table on the database
    /// </summary>
    [ImplementPropertyChanged]
    public class PaymentDataAccess : AbstractDataAccess<Payment>
    {
        private readonly SQLiteConnection dbConnection;

        public PaymentDataAccess(SQLiteConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        /// <summary>
        ///     Saves a new item or updates an existing
        /// </summary>
        /// <param name="itemToSave">Item to SaveItem</param>
        protected override void SaveToDb(Payment itemToSave)
        {
            //Don't use insert or replace here, because it will always replace the first element
            if (itemToSave.Id == 0)
            {
                dbConnection.Insert(itemToSave);
                itemToSave.Id = dbConnection.Table<Payment>().OrderByDescending(x => x.Id).First().Id;
            }
            else
            {
                dbConnection.Update(itemToSave);
            }
        }

        /// <summary>
        ///     Deletes an item from the database
        /// </summary>
        /// <param name="payment">Item to Delete.</param>
        protected override void DeleteFromDatabase(Payment payment)
        {
            dbConnection.Delete(payment);
        }

        /// <summary>
        ///     Loads a list of payments from the database filtered by the expression
        /// </summary>
        /// <param name="filter">filter expression.</param>
        /// <returns>List of loaded payments.</returns>
        protected override List<Payment> GetListFromDb(Expression<Func<Payment, bool>> filter)
        {
            var listQuery = dbConnection.Table<Payment>();

            if (filter != null)
            {
                listQuery = listQuery.Where(filter);
            }

            var payments = listQuery.ToList();
            var accounts = dbConnection.Table<Account>().ToList();

            var recurringTransactionsQuery = dbConnection.Table<RecurringPayment>();
            var categoriesQuery = dbConnection.Table<Category>();

            foreach (var payment in payments)
            {
                payment.ChargedAccount = accounts.FirstOrDefault(x => x.Id == payment.ChargedAccountId);
                payment.TargetAccount = accounts.FirstOrDefault(x => x.Id == payment.TargetAccountId);

                payment.Category = categoriesQuery.FirstOrDefault(x => x.Id == payment.CategoryId);

                if (payment.IsRecurring)
                {
                    payment.RecurringPayment =
                        recurringTransactionsQuery.FirstOrDefault(x => x.Id == payment.RecurringPaymentId);
                }
            }

            return payments;
        }
    }
}