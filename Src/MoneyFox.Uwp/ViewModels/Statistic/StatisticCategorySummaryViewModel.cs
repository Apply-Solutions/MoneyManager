﻿using GalaSoft.MvvmLight.Command;
using MediatR;
using MoneyFox.Application.Common.Facades;
using MoneyFox.Application.Statistics.Queries.GetCategorySummary;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyFox.Uwp.ViewModels.Statistic
{
    /// <inheritdoc cref="IStatisticCategorySummaryViewModel"/>
    public class StatisticCategorySummaryViewModel : StatisticViewModel, IStatisticCategorySummaryViewModel
    {
        private ObservableCollection<CategoryOverviewViewModel> categorySummary;

        public StatisticCategorySummaryViewModel(IMediator mediator,
                                                 ISettingsFacade settingsFacade) : base(mediator, settingsFacade)
        {
            CategorySummary = new ObservableCollection<CategoryOverviewViewModel>();
            IncomeExpenseBalance = new IncomeExpenseBalanceViewModel();
        }

        private IncomeExpenseBalanceViewModel incomeExpenseBalance;

        public IncomeExpenseBalanceViewModel IncomeExpenseBalance
        {
            get => incomeExpenseBalance;
            set
            {
                if(incomeExpenseBalance == value)
                    return;
                incomeExpenseBalance = value;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<CategoryOverviewViewModel> CategorySummary
        {
            get => categorySummary;
            private set
            {
                categorySummary = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(HasData));
            }
        }

        /// <inheritdoc/>
        public bool HasData => CategorySummary.Any();

        /// <summary>
        /// Overrides the load method to load the category summary data.
        /// </summary>
        protected override async Task LoadAsync()
        {
            CategorySummaryModel categorySummaryModel = await Mediator.Send(new GetCategorySummaryQuery { EndDate = EndDate, StartDate = StartDate });

            CategorySummary = new ObservableCollection<CategoryOverviewViewModel>(categorySummaryModel
                                                                                     .CategoryOverviewItems
                                                                                     .Select(x => new CategoryOverviewViewModel
                                                                                     {
                                                                                         CategoryId = x.CategoryId,
                                                                                         Value = x.Value,
                                                                                         Average = x.Average,
                                                                                         Label = x.Label,
                                                                                         Percentage = x.Percentage
                                                                                     }));

            IncomeExpenseBalance = new IncomeExpenseBalanceViewModel
            {
                TotalEarned = categorySummaryModel.TotalEarned,
                TotalSpent = categorySummaryModel.TotalSpent
            };
        }
    }
}
