﻿using GalaSoft.MvvmLight.Command;
using MoneyFox.Application;
using MoneyFox.Application.Common.Facades;
using MoneyFox.Services;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyFox.ViewModels.Settings
{
    public class SettingsViewModel : BaseViewModel
    {
        private readonly ISettingsFacade settingsFacade;
        private readonly IDialogService dialogService;

        public SettingsViewModel(ISettingsFacade settingsFacade,
                                 IDialogService dialogService)
        {
            this.settingsFacade = settingsFacade;
            this.dialogService = dialogService;

            AvailableCultures = new ObservableCollection<CultureInfo>();
        }

        private CultureInfo selectedCulture = CultureHelper.CurrentCulture;

        public CultureInfo SelectedCulture
        {
            get => selectedCulture;
            set
            {
                if(selectedCulture == value) return;
                selectedCulture = value;
                settingsFacade.DefaultCulture = selectedCulture.Name;
                CultureHelper.CurrentCulture = selectedCulture;
                RaisePropertyChanged();
            }
        }

        public ObservableCollection<CultureInfo> AvailableCultures { get; }

        public RelayCommand LoadAvailableCulturesCommand => new RelayCommand(async() => await LoadAvailableCulturesAsync());

        private async Task LoadAvailableCulturesAsync()
        {
            await dialogService.ShowLoadingDialogAsync();

            CultureInfo.GetCultures(CultureTypes.AllCultures).OrderBy(x => x.Name).ToList().ForEach(AvailableCultures.Add);
            SelectedCulture = AvailableCultures.First(x => x.Name == settingsFacade.DefaultCulture);

            await dialogService.HideLoadingDialogAsync();
        }
    }
}