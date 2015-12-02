﻿using Windows.System;
using Windows.UI.Xaml.Input;
using Cirrious.CrossCore;
using MoneyManager.Core.ViewModels.Dialogs;
using MoneyManager.Foundation.Model;

namespace MoneyManager.Windows.Dialogs
{
    public sealed partial class CategoryDialog
    {
        public CategoryDialog(Category category = null)
        {
            InitializeComponent();

            DataContext = Mvx.Resolve<CategoryDialogViewModel>();

            if (category != null)
            {
                ((CategoryDialogViewModel) DataContext).IsEdit = true;
                ((CategoryDialogViewModel) DataContext).Selected = category;
            }
        }

        private void TextBox_OnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                ((CategoryDialogViewModel)DataContext).DoneCommand.Execute();
                Hide();
            }
        }
    }
}