﻿namespace MoneyFox.Foundation.Interfaces
{
    public interface ITileUpdateService
    {
        /// <summary>
        ///     Sets the MainTile with new Information
        /// </summary>
        /// <param name="income">Income of these month</param>
        /// <param name="spending">Expense of these month</param>
        /// <param name="earnings">Earnings of these month </param>
        void UpdateMainTile(string balance, string income, string spending, string earnings);
    }
}