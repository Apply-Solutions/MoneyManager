﻿using MoneyFox.Presentation.Dialogs;
using MoneyFox.Ui.Shared.ViewModels.Statistics;
using System;

namespace MoneyFox.Views.Statistics
{
    public partial class StatisticCashFlowPage
    {
        private StatisticCashFlowViewModel ViewModel => (StatisticCashFlowViewModel)BindingContext;

        public StatisticCashFlowPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.StatisticCashFlowViewModel;

            ViewModel.LoadedCommand.Execute(null);
        }

        private async void OpenFilterDialog(object sender, EventArgs e)
            => await new DateSelectionPopup(ViewModel.StartDate, ViewModel.EndDate).ShowAsync();
    }
}
