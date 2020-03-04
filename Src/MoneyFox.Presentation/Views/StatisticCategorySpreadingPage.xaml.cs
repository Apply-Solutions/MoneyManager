﻿using MoneyFox.Application.Resources;
using MoneyFox.Presentation.Dialogs;
using MoneyFox.Presentation.ViewModels.Statistic;
using MoneyFox.Ui.Shared.Utilities;
using Rg.Plugins.Popup.Extensions;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace MoneyFox.Presentation.Views
{
    public partial class StatisticCategorySpreadingPage
    {
        private StatisticCategorySpreadingViewModel ViewModel => BindingContext as StatisticCategorySpreadingViewModel;

        public StatisticCategorySpreadingPage()
        {
            InitializeComponent();
            BindingContext = ViewModelLocator.StatisticCategorySpreadingVm;

            var filterItem = new ToolbarItem
                             {
                                 Command = new Command(async() => await OpenDialog()),
                                 Text = Strings.SelectDateLabel,
                                 Priority = 0,
                                 Order = ToolbarItemOrder.Primary
                             };

            ToolbarItems.Add(filterItem);

            ViewModel.LoadedCommand.ExecuteAsync().FireAndForgetSafeAsync();
        }

        private async Task OpenDialog()
        {
            await Navigation.PushPopupAsync(new DateSelectionPopup
            {
                BindingContext = ViewModelLocator.SelectDateRangeDialogVm
            });
        }
    }
}
