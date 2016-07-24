﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MoneyFox.Shared.Model;
using PropertyChanged;
using SQLite.Net;

namespace MoneyFox.Shared.DataAccess
{
    [ImplementPropertyChanged]
    public class RecurringPaymentDataAccess : AbstractDataAccess<RecurringPayment>
    {
        private readonly SQLiteConnection dbConnection;

        public RecurringPaymentDataAccess(SQLiteConnection dbConnection)
        {
            this.dbConnection = dbConnection;
        }

        /// <summary>
        ///     Saves an recurring payment to the database.
        /// </summary>
        /// <param name="itemToSave">Recurring Payment to save.</param>
        protected override void SaveToDb(RecurringPayment itemToSave)
        {
            //Don't use insert or replace here, because it will always replace the first element
            if (itemToSave.Id == 0)
            {
                dbConnection.Insert(itemToSave);
                itemToSave.Id = dbConnection.Table<RecurringPayment>().OrderByDescending(x => x.Id).First().Id;
            }
            else
            {
                dbConnection.Update(itemToSave);
            }
        }

        /// <summary>
        ///     Deletres an recurring payment from the database.
        /// </summary>
        /// <param name="payment">recurring payment to delete.</param>
        protected override void DeleteFromDatabase(RecurringPayment payment)
        {
            dbConnection.Delete(payment);
        }

        /// <summary>
        ///     Loads a list of recurring payments from the database filtered by the filter expression.
        /// </summary>
        /// <param name="filter">Filter expression.</param>
        /// <returns>List of loaded recurring payments.</returns>
        protected override List<RecurringPayment> GetListFromDb(Expression<Func<RecurringPayment, bool>> filter)
        {
            var listQuery = dbConnection.Table<RecurringPayment>();

            if (filter != null)
            {
                listQuery = listQuery.Where(filter);
            }

            return listQuery.ToList();
        }
    }
}