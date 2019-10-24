﻿using System.Globalization;
using System.Windows.Input;
using MoneyFox.Application.Resources;
using MoneyFox.Presentation.Utilities;

namespace MoneyFox.Presentation.ViewModels.DesignTime
{
    public class DesignTimeSettingsPersonalizationViewModel : ISettingsPersonalizationViewModel
    {
        public DesignTimeSettingsPersonalizationViewModel()
        {
            Resources = new LocalizedResources(typeof(Strings), CultureInfo.CurrentUICulture);
        }

        public LocalizedResources Resources { get; }
        public string ElementTheme { get; }
        public ICommand SwitchThemeCommand { get; }
    }
}
