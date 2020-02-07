﻿using System;
using System.Globalization;
using MoneyFox.Application;
using MoneyFox.Presentation.ViewModels;
using Xamarin.Forms;

namespace MoneyFox.Presentation.Converter
{
    public class AccountNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(value is AccountViewModel account) ? "" : $"{account.Name} ({account.CurrentBalance.ToString("C", CultureHelper.CurrentCulture)})";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
