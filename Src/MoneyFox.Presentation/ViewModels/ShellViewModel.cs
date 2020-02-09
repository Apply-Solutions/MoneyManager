﻿using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Presentation.Services;

namespace MoneyFox.Presentation.ViewModels
{
    /// <summary>
    ///     Representation of the MainView
    /// </summary>
    public class ShellViewModel : ViewModelBase
    {
        private readonly INavigationService navigationService;

        public ShellViewModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public ICommand ShowAccountListCommand
            => new RelayCommand(() => navigationService.NavigateTo(ViewModelLocator.AccountList));

        public ICommand ShowStatisticSelectorCommand
            => new RelayCommand(() => navigationService.NavigateTo(ViewModelLocator.StatisticSelector));

        public ICommand ShowCategoryListCommand
            => new RelayCommand(() => navigationService.NavigateTo(ViewModelLocator.CategoryList));

        public ICommand ShowBackupViewCommand
            => new RelayCommand(() => navigationService.NavigateTo(ViewModelLocator.Backup));

        public ICommand ShowSettingsCommand
            => new RelayCommand(() => navigationService.NavigateTo(ViewModelLocator.Settings));

        public ICommand ShowAboutCommand
            => new RelayCommand(() => navigationService.NavigateTo(ViewModelLocator.About));
    }
}
