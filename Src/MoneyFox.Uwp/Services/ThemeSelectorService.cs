﻿using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml;
using MoneyFox.BusinessLogic.Adapters;
using MoneyFox.Foundation;
using MoneyFox.Presentation.Facades;

namespace MoneyFox.Uwp.Services
{
    public static class ThemeSelectorService
    {
        private const string SETTINGS_KEY = "AppBackgroundRequestedTheme";

        public static ElementTheme Theme { get; set; } = ElementTheme.Default;

        public static void Initialize()
        {
            Theme = LoadThemeFromSettingsAsync();
        }

        public static async Task SetThemeAsync(ElementTheme theme)
        {
            Theme = theme;

            await SetRequestedThemeAsync();
            SaveThemeInSettings(Theme);
        }

        public static async Task SetRequestedThemeAsync()
        {
            foreach (var view in CoreApplication.Views)
            {
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    if (Window.Current.Content is FrameworkElement frameworkElement)
                    {
                        frameworkElement.RequestedTheme = Theme;
                    }
                });
            }
        }

        private static ElementTheme LoadThemeFromSettingsAsync()
        {
            ElementTheme cacheTheme = ElementTheme.Default;

            string themeName = new SettingsAdapter().GetValue(SETTINGS_KEY, string.Empty);

            if (!string.IsNullOrEmpty(themeName))
            {
                Enum.TryParse(themeName, out cacheTheme);
            }

            return cacheTheme;
        }

        private static void SaveThemeInSettings(ElementTheme theme)
        {
            var settingsAdapter = new SettingsAdapter();
            settingsAdapter.AddOrUpdate(SETTINGS_KEY, theme.ToString());
            var settingsFacade = new SettingsFacade(settingsAdapter);

            settingsFacade.Theme = Theme == ElementTheme.Dark ? AppTheme.Dark : AppTheme.Light;
        }
    }
}
