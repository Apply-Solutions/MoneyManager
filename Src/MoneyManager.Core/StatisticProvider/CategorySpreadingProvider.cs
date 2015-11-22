﻿using System;
using System.Collections.Generic;
using System.Linq;
using MoneyManager.Foundation;
using MoneyManager.Foundation.Interfaces;
using MoneyManager.Foundation.Model;

namespace MoneyManager.Core.StatisticProvider
{
    public class CategorySpreadingProvider : IStatisticProvider<IEnumerable<StatisticItem>>
    {
        private readonly IRepository<Category> categoryRepository;
        private readonly ITransactionRepository transactionRepository;

        public CategorySpreadingProvider(ITransactionRepository transactionRepository,
            IRepository<Category> categoryRepository)
        {
            this.transactionRepository = transactionRepository;
            this.categoryRepository = categoryRepository;
        }

        /// <summary>
        ///     Selects transactions from the given timeframe and calculates the spreading for the six categories
        ///     with the highest spendings. All others are summarized in a "other" item.
        /// </summary>
        /// <param name="startDate">Startpoint form which to select data.</param>
        /// <param name="endDate">Endpoint form which to select data.</param>
        /// <returns>Statistic value for the given time. </returns>
        public IEnumerable<StatisticItem> GetValues(DateTime startDate, DateTime endDate)
        {
            var transactionListFunc =
                new Func<List<FinancialTransaction>>(() =>
                    transactionRepository.Data
                        .Where(x => x.Category != null)
                        .Where(x => x.Date >= startDate.Date && x.Date <= endDate.Date)
                        .Where(x => x.Type == (int) TransactionType.Spending)
                        .ToList());

            return GetSpreadingStatisticItems(transactionListFunc);
        }

        private List<StatisticItem> GetSpreadingStatisticItems(
            Func<List<FinancialTransaction>> getTransactionListFunc)
        {
            var transactionList = getTransactionListFunc();

            var tempStatisticList = categoryRepository.Data.Select(category => new StatisticItem
            {
                Category = category.Name,
                Value = transactionList
                    .Where(x => x.Category.Id == category.Id)
                    .Sum(x => x.Amount)
            }).ToList();

            RemoveNullList(tempStatisticList);

            tempStatisticList = tempStatisticList.OrderByDescending(x => x.Value).ToList();
            var statisticList = tempStatisticList.Take(6).ToList();

            AddOtherItem(tempStatisticList, statisticList);

            IncludeIncome(statisticList);

            return statisticList;
        }

        private void RemoveNullList(ICollection<StatisticItem> tempStatisticList)
        {
            var nullerList = tempStatisticList.Where(x => Math.Abs(x.Value) < 0.001).ToList();
            foreach (var statisticItem in nullerList)
            {
                tempStatisticList.Remove(statisticItem);
            }
        }

        private void SetLabel(StatisticItem item)
        {
            item.Label = item.Category + ": " + item.Value;
        }

        private void IncludeIncome(IEnumerable<StatisticItem> statisticList)
        {
            foreach (var statisticItem in statisticList)
            {
                statisticItem.Value -= transactionRepository.Data
                    .Where(x => x.Type == (int) TransactionType.Income)
                    .Where(x => x.Category != null)
                    .Where(x => x.Category.Name == statisticItem.Category)
                    .Sum(x => x.Amount);

                SetLabel(statisticItem);

                if (statisticItem.Value <= 0)
                {
                    statisticItem.Value = 0;
                }
            }
        }

        private void AddOtherItem(IEnumerable<StatisticItem> tempStatisticList,
            ICollection<StatisticItem> statisticList)
        {
            if (statisticList.Count < 6)
            {
                return;
            }

            var othersItem = new StatisticItem
            {
                Category = "Others",
                Value = tempStatisticList
                    .Where(x => !statisticList.Contains(x))
                    .Sum(x => x.Value)
            };
            othersItem.Label = othersItem.Category + ": " + othersItem.Value;

            if (othersItem.Value > 0)
            {
                statisticList.Add(othersItem);
            }
        }
    }
}