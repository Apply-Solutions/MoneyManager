﻿using System;
using MoneyFox.Foundation;
using MoneyFox.Foundation.Interfaces;
using MvvmCross.Logging;
using MvvmCross.Navigation;

namespace MoneyFox.Business.ViewModels
{
    public interface ISettingsPersonalizationViewModel
    {
        int SelectedIndex { get; set; }
    }

    public class SettingsPersonalizationViewModel : BaseNavigationViewModel, ISettingsPersonalizationViewModel
    {
        private readonly ISettingsManager settingsManager;

        public SettingsPersonalizationViewModel(ISettingsManager settingsManager,
                                                IMvxLogProvider logProvider,
                                                IMvxNavigationService navigationService) : base(logProvider, navigationService)
        {
            this.settingsManager = settingsManager;
        }

        public int SelectedIndex
        {
            get => (int) settingsManager.Theme;
            set
            {
                var theme = (AppTheme)Enum.ToObject(typeof(AppTheme), value);
                settingsManager.Theme = theme;
                RaisePropertyChanged();
            }
        }
    }
}