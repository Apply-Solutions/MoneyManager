﻿using MoneyFox.Presentation.ViewModels;
using Rg.Plugins.Popup.Extensions;
using System;
using Xamarin.Forms.Xaml;

namespace MoneyFox.Presentation.Dialogs
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DateSelectionPopup
    {
        public DateSelectionPopup()
        {
            InitializeComponent();
        }

        private async void Button_OnClicked(object sender, EventArgs e)
        {
            await Navigation.PopPopupAsync();
            (BindingContext as SelectDateRangeDialogViewModel)?.DoneCommand.Execute(null);
        }
    }
}
