﻿using System;
using MoneyFox.ServiceLayer.Utilities;
using MoneyFox.ServiceLayer.ViewModels.Interfaces;
using MvvmCross.Commands;

namespace MoneyFox.ServiceLayer.ViewModels.DesignTime
{
    public class DesignTimePaymentListViewActionViewModel
    {
        public LocalizedResources Resources { get; }
        public MvxAsyncCommand GoToAddIncomeCommand { get; }
        public MvxAsyncCommand GoToAddExpenseCommand { get; }
        public MvxAsyncCommand GoToAddTransferCommand { get; }
        public bool IsAddIncomeAvailable { get; }
        public bool IsAddExpenseAvailable { get; }
        public bool IsTransferAvailable { get; }
        public MvxAsyncCommand DeleteAccountCommand { get; }
        public bool IsClearedFilterActive { get; set; }
        public bool IsRecurringFilterActive { get; set; }
        public DateTime TimeRangeStart { get; set; }
        public DateTime TimeRangeEnd { get; set; }
    }
}
