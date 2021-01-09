﻿using MoneyFox.Uwp.ViewModels.Settings;

#nullable enable
namespace MoneyFox.Uwp.Views.Settings
{
    public sealed partial class SettingsView
    {
        public override bool ShowHeader => false;

        private WindowsSettingsViewModel ViewModel => (WindowsSettingsViewModel)DataContext;

        public SettingsView()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.SettingsVm;
        }
    }
}
