﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Model;
using PropertyChanged;

namespace MoneyFox.Shared.DataAccess
{
    [ImplementPropertyChanged]
    public class RecurringPaymentDataAccess : AbstractDataAccess<RecurringPayment>
    {
        private readonly IDatabaseManager connectionCreator;

        public RecurringPaymentDataAccess(IDatabaseManager connectionCreator)
        {
            this.connectionCreator = connectionCreator;
        }

        /// <summary>
        ///     Saves an recurring payment to the database.
        /// </summary>
        /// <param name="itemToSave">Recurring Payment to save.</param>
        protected override void SaveToDb(RecurringPayment itemToSave)
        {
            using (var db = connectionCreator.GetConnection())
            {
                //Don't use insert or replace here, because it will always replace the first element
                if (itemToSave.Id == 0)
                {
                    db.Insert(itemToSave);
                    itemToSave.Id = db.Table<RecurringPayment>().OrderByDescending(x => x.Id).First().Id;
                }
                else
                {
                    db.Update(itemToSave);
                }
            }
        }

        /// <summary>
        ///     Deletres an recurring payment from the database.
        /// </summary>
        /// <param name="payment">recurring payment to delete.</param>
        protected override void DeleteFromDatabase(RecurringPayment payment)
        {
            using (var db = connectionCreator.GetConnection())
            {
                db.Delete(payment);
            }
        }

        /// <summary>
        ///     Loads a list of recurring payments from the database filtered by the filter expression.
        /// </summary>
        /// <param name="filter">Filter expression.</param>
        /// <returns>List of loaded recurring payments.</returns>
        protected override List<RecurringPayment> GetListFromDb(Expression<Func<RecurringPayment, bool>> filter)
        {
            using (var db = connectionCreator.GetConnection())
            {
                var listQuery = db.Table<RecurringPayment>();

                if (filter != null)
                {
                    listQuery = listQuery.Where(filter);
                }

                return listQuery.ToList();
            }
        }
    }
}