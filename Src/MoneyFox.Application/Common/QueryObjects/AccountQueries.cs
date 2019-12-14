﻿using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MoneyFox.Domain.Entities;

namespace MoneyFox.Application.Common.QueryObjects
{
    /// <summary>
    ///     Query Objects for account queries.
    /// </summary>
    public static class AccountQueries
    {
        /// <summary>
        ///     Adds a filter to a query for excluded accounts
        /// </summary>
        /// <param name="query">Existing query.</param>
        /// <returns>Query with a filter for excluded accounts.</returns>
        public static IQueryable<Account> AreNotExcluded(this IQueryable<Account> query)
        {
            return query.Where(x => !x.IsExcluded);
        }

        /// <summary>
        ///     Adds a filter to a query for not excluded accounts
        /// </summary>
        /// <param name="query">Existing query.</param>
        /// <returns>Query with a filter for not excluded accounts.</returns>
        public static IQueryable<Account> AreExcluded(this IQueryable<Account> query)
        {
            return query.Where(x => x.IsExcluded);
        }

        /// <summary>
        ///     Order a query by a name.
        /// </summary>
        /// <param name="query">Existing query.</param>
        /// <returns>Query ordered by name.</returns>
        public static IQueryable<Account> OrderByName(this IQueryable<Account> query)
        {
            return query.OrderBy(x => x.Name);
        }

        /// <summary>
        ///     Checks if there is an account with the passed name.
        /// </summary>
        public static async Task<bool> AnyWithNameAsync(this IQueryable<Account> query, string name)
        {
            return await query.AnyAsync(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
