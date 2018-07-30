﻿using System.Collections.Generic;
using Microcharts;
using MoneyFox.Business.ViewModels;
using MoneyFox.Business.ViewModels.Statistic;

namespace MoneyFox.Windows.DesignTime
{
    public class DesignTimeStatisticCashFlowViewModel : BaseViewModel, IStatisticCashFlowViewModel
    {
        public string Title => "I AM A MIGHTY TITLE";

        public BarChart Chart => new BarChart
        {
            Entries = new List<Entry>
            {
                new Entry(1234) {Label = "Expense"},
                new Entry(1465) {Label = "Income"},
                new Entry(543) {Label = "Revenue"}
            }
        };
    }
}
