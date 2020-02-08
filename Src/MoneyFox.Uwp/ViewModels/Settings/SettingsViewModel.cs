﻿using System.Collections.ObjectModel;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Application.Resources;
using MoneyFox.Domain;
using MoneyFox.Presentation.Models;

namespace MoneyFox.Presentation.ViewModels.Settings
{
    /// <summary>
    ///     ViewModel for the settings view.
    /// </summary>
    public class SettingsViewModel : ViewModelBase, ISettingsViewModel
    {
        private readonly INavigationService navigationService;

        public SettingsViewModel(INavigationService navigationService,
                                 IAboutViewModel aboutViewModel,
                                 ISettingsBackgroundJobViewModel settingsBackgroundJobViewModel,
                                 IRegionalSettingsViewModel regionalSettingsViewModel,
                                 ISettingsPersonalizationViewModel settingsPersonalizationViewModel)
        {
            this.navigationService = navigationService;

            AboutViewModel = aboutViewModel;
            BackgroundJobViewModel = settingsBackgroundJobViewModel;
            RegionalSettingsViewModel = regionalSettingsViewModel;
            PersonalizationViewModel = settingsPersonalizationViewModel;
        }

        public IAboutViewModel AboutViewModel { get; }

        public ISettingsBackgroundJobViewModel BackgroundJobViewModel { get; private set; }

        public ISettingsPersonalizationViewModel PersonalizationViewModel { get; private set; }

        public IRegionalSettingsViewModel RegionalSettingsViewModel { get; private set; }

        /// <inheritdoc />
        public ObservableCollection<SettingsSelectorType> SettingsList => new ObservableCollection<SettingsSelectorType>
        {
            new SettingsSelectorType
            {
                Name = Strings.SettingsPersonalizationLabel,
                Icon = "\uf27c",
                Description = Strings.SettingsPersonalizationDescription,
                Type = SettingsType.Personalization
            },
            new SettingsSelectorType
            {
                Name = Strings.SettingsRegionalLabel,
                Icon = "\uf8ae",
                Description = Strings.RegionalSettingsDescriptionText,
                Type = SettingsType.Regional
            },
            new SettingsSelectorType
            {
                Name = Strings.CategoriesLabel,
                Icon = "\uf316",
                Description = Strings.CategoriesSettingsDescription,
                Type = SettingsType.Categories
            },
            new SettingsSelectorType
            {
                Name = Strings.BackgroundJobLabel,
                Icon = "\uf4e6",
                Description = Strings.BackgroundJobSettingDescription,
                Type = SettingsType.BackgroundJob
            },
            new SettingsSelectorType
            {
                Name = Strings.BackupLabel,
                Icon = "\uf167",
                Description = Strings.BackupSettingsDescription,
                Type = SettingsType.Backup
            },
            new SettingsSelectorType
            {
                Name = Strings.AboutLabel,
                Icon = "\uf2fd",
                Description = Strings.AboutSettingsDescription,
                Type = SettingsType.About
            }
        };

        /// <inheritdoc />
        public RelayCommand<SettingsSelectorType> GoToSettingCommand => new RelayCommand<SettingsSelectorType>(GoToSettings);

        private void GoToSettings(SettingsSelectorType item)
        {
            switch (item.Type)
            {
                case SettingsType.Personalization:
                    navigationService.NavigateTo(ViewModelLocator.SettingsPersonalization);
                    break;

                case SettingsType.Regional:
                    navigationService.NavigateTo(ViewModelLocator.SettingsRegional);
                    break;

                case SettingsType.Categories:
                    navigationService.NavigateTo(ViewModelLocator.CategoryList);
                    break;

                case SettingsType.BackgroundJob:
                    navigationService.NavigateTo(ViewModelLocator.SettingsBackgroundJob);
                    break;

                case SettingsType.Backup:
                    navigationService.NavigateTo(ViewModelLocator.Backup);
                    break;

                case SettingsType.About:
                    navigationService.NavigateTo(ViewModelLocator.About);
                    break;
            }
        }
    }
}