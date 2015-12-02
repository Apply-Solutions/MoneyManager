﻿using System;
using System.ServiceModel.Channels;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Cirrious.CrossCore;
using MoneyManager.Core.ViewModels;
using MoneyManager.Foundation.Model;
using MoneyManager.Windows.Dialogs;

namespace MoneyManager.Windows.Controls
{
    public partial class CategoryListUserControl
    {
        public CategoryListUserControl()
        {
            InitializeComponent();

            DataContext = Mvx.Resolve<CategoryListViewModel>();
        }

        private void CategoryListRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void CategoryListHolding(object sender, HoldingRoutedEventArgs e)
        {
            var senderElement = sender as FrameworkElement;
            var flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private async void EditCategory(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var category = element.DataContext as Category;
            if (category == null)
            {
                return;
            }

            var dialog = new CategoryDialog(category);
            await dialog.ShowAsync();
        }

        private void DeleteCategory(object sender, RoutedEventArgs e)
        {
            var element = (FrameworkElement) sender;
            var category = element.DataContext as Category;
            if (category == null)
            {
                return;
            }

            ((CategoryListViewModel) DataContext).DeleteCategoryCommand.Execute(category);
        }

        protected override void OnKeyDown(KeyRoutedEventArgs e)
        {
            if(e.Key == VirtualKey.Enter)
            {
                ((CategoryListViewModel)DataContext).DoneCommand.Execute();
            }

            base.OnKeyDown(e);
        }
    }
}