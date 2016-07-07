﻿using MoneyFox.Shared.Helpers;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.StatisticDataProvider;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using PropertyChanged;

namespace MoneyFox.Shared.ViewModels
{
    [ImplementPropertyChanged]
    public class StatisticCashFlowViewModel : StatisticViewModel
    {
        private readonly CashFlowDataProvider cashFlowDataProvider;

        public StatisticCashFlowViewModel(IPaymentRepository paymentRepository)
        {
            cashFlowDataProvider = new CashFlowDataProvider(paymentRepository);

            CashFlowModel = GetCashFlowModel();
        }

        /// <summary>
        ///     Contains the PlotModel for the CashFlow graph
        /// </summary>
        public PlotModel CashFlowModel { get; set; }

        /// <summary>
        ///     Loads the cashflow with the current start and end date.
        /// </summary>
        protected override void Load()
        {
            CashFlowModel = null;
            CashFlowModel = GetCashFlowModel();
        }

        /// <summary>
        ///     Set a custom CashFlowModel with the set Start and Enddate
        /// </summary>
        public PlotModel GetCashFlowModel()
        {
            var cashFlow = cashFlowDataProvider.GetValues(StartDate, EndDate);

            var model = new PlotModel();

            var columnSeries = new ColumnSeries();
            var axe = new CategoryAxis
            {
                AxislineColor = OxyColors.Black,
                TextColor = OxyColors.Black,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                Angle = 45
            };

            if (SettingsHelper.IsDarkThemeSelected)
            {
                axe.AxislineColor = OxyColors.White;
                axe.AxislineColor = OxyColors.White;

                model.Background = OxyColors.Black;
                model.TextColor = OxyColors.White;
            }
            else
            {
                axe.AxislineColor = OxyColors.Black;
                axe.AxislineColor = OxyColors.Black;

                model.Background = OxyColors.White;
                model.TextColor = OxyColors.Black;
            }

            columnSeries.Items.Add(new ColumnItem(cashFlow.Income.Value) {Color = OxyColors.LightGreen});
            axe.Labels.Add(cashFlow.Income.Label);
            columnSeries.Items.Add(new ColumnItem(cashFlow.Spending.Value) {Color = OxyColor.FromRgb(196, 54, 51)});
            axe.Labels.Add(cashFlow.Spending.Label);
            columnSeries.Items.Add(new ColumnItem(cashFlow.Revenue.Value) {Color = OxyColors.Cyan});
            axe.Labels.Add(cashFlow.Revenue.Label);

            model.Axes.Add(axe);
            model.Series.Add(columnSeries);
            return model;
        }
    }
}