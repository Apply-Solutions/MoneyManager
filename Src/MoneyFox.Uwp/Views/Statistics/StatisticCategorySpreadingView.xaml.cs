﻿using MoneyFox.Application.Resources;
using MoneyFox.Uwp.Views.Dialogs;
using Windows.UI.Xaml;

#nullable enable
namespace MoneyFox.Uwp.Views.Statistics
{
    public sealed partial class StatisticCategorySpreadingView
    {
        public override string Header => Strings.CategorySpreadingTitle;

        public StatisticCategorySpreadingView()
        {
            InitializeComponent();
        }

        private async void SetDate(object sender, RoutedEventArgs e)
        {
            await new SelectDateRangeDialog
            {
                DataContext = ViewModelLocator.SelectDateRangeDialogVm
            }.ShowAsync();
        }
    }
}
