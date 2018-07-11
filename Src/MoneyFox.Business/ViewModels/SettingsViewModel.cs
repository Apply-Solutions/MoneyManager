﻿using System.Threading.Tasks;
using MoneyFox.Foundation;
using MoneyFox.Foundation.Interfaces;
using MoneyFox.Foundation.Models;
using MoneyFox.Foundation.Resources;
using MvvmCross.Commands;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Plugin.Connectivity.Abstractions;

namespace MoneyFox.Business.ViewModels
{
    public interface ISettingsViewModel : IBaseViewModel
    {
        /// <summary>
        ///     Contains all available Settingsitems.
        /// </summary>
        MvxObservableCollection<SettingsSelectorType> SettingsList { get; }

        /// <summary>
        ///     Navigate to a concrete settings page.
        ///     Used in Xamarin Forms.
        /// </summary>
        MvxAsyncCommand<SettingsSelectorType> GoToSettingCommand { get; }
    }

    /// <summary>
    ///     ViewModel for the settings view.
    /// </summary>
    public class SettingsViewModel : BaseViewModel, ISettingsViewModel
    {
        private readonly IMvxNavigationService navigationService;

        public SettingsViewModel(ISettingsManager settingsManager,
                                 IPasswordStorage passwordStorage,
                                 ITileManager tileManager,
                                 IBackgroundTaskManager backgroundTaskManager,
                                 IDialogService dialogService,
                                 IMvxNavigationService navigationService,
                                 IBackupManager backupManager,
                                 IConnectivity connectivity)
        {
            this.navigationService = navigationService;

            SettingsGeneralViewModel = new SettingsGeneralViewModel(backupManager, dialogService, connectivity, settingsManager);
            SettingsSecurityViewModel = new SettingsSecurityViewModel(settingsManager, passwordStorage, dialogService);
            SettingsShortcutsViewModel = new SettingsShortcutsViewModel(settingsManager, tileManager);
            SettingsPersonalizationViewModel = new SettingsPersonalizationViewModel(settingsManager);
        }

        public SettingsGeneralViewModel SettingsGeneralViewModel { get; }

        public SettingsSecurityViewModel SettingsSecurityViewModel { get; }

        public SettingsShortcutsViewModel SettingsShortcutsViewModel { get; }

        public SettingsPersonalizationViewModel SettingsPersonalizationViewModel { get; }

        public string Title => Strings.SettingsLabel;

        /// <inheritdoc />
        public MvxObservableCollection<SettingsSelectorType> SettingsList => new MvxObservableCollection<SettingsSelectorType>
        {
            new SettingsSelectorType
            {
                Name = Strings.CategoriesLabel,
                Description = Strings.CategoriesSettingsDescription,
                Type = SettingsType.Categories
            },
            new SettingsSelectorType
            {
                Name = Strings.BackupLabel,
                Description = Strings.BackupSettingsDescription,
                Type = SettingsType.Backup
            },
            new SettingsSelectorType
            {
                Name = Strings.AboutLabel,
                Description = Strings.AboutSettingsDescription,
                Type = SettingsType.About
            }
        };

        /// <inheritdoc />
        public MvxAsyncCommand<SettingsSelectorType> GoToSettingCommand => new MvxAsyncCommand<SettingsSelectorType>(GoToSettings);

        private async Task GoToSettings(SettingsSelectorType item)
        {
            switch (item.Type)
            {
                case SettingsType.Categories:
                    await navigationService.Navigate<CategoryListViewModel>();
                    break;

                case SettingsType.Backup:
                    await navigationService.Navigate<BackupViewModel>();
                    break;

                case SettingsType.About:
                    await navigationService.Navigate<AboutViewModel>();
                    break;
            }
        }
    }
}