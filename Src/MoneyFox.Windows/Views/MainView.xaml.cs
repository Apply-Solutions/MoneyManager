﻿using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MoneyFox.Business.ViewModels;
using MoneyFox.Foundation.Resources;

namespace MoneyFox.Windows.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainView
    {
        public MainView()
        {
            this.InitializeComponent();

            CoreApplicationViewTitleBar titleBar = CoreApplication.GetCurrentView().TitleBar;
            titleBar.LayoutMetricsChanged += TitleBar_LayoutMetricsChanged;
        }

        public Frame MainFrame => ContentFrame;

        private void TitleBar_LayoutMetricsChanged(CoreApplicationViewTitleBar sender, object args)
        {
            AppTitle.Margin = new Thickness(CoreApplication.GetCurrentView().TitleBar.SystemOverlayLeftInset + 12, 8, 0, 0);
        }

        private async void NavView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                await ((MainViewModel) ViewModel).ShowSettingsCommand.ExecuteAsync();
            } 
            else
            {
                if (args.InvokedItem == null) return;

                if (args.InvokedItem.Equals(Strings.AccountsTitle))
                {
                    await ((MainViewModel)ViewModel).ShowAccountListCommand.ExecuteAsync();
                }
                else if(args.InvokedItem.Equals(Strings.CategoriesTitle))
                {
                    await ((MainViewModel)ViewModel).ShowCategoryListCommand.ExecuteAsync();
                }
                else if(args.InvokedItem.Equals(Strings.StatisticsTitle))
                {
                    await ((MainViewModel)ViewModel).ShowStatisticSelectorCommand.ExecuteAsync();
                }
                else if(args.InvokedItem.Equals(Strings.BackupTitle))
                {
                    await ((MainViewModel)ViewModel).ShowBackupViewCommand.ExecuteAsync();
                } 
                else if(args.InvokedItem.Equals(Strings.SettingsTitle))
                {
                    await ((MainViewModel)ViewModel).ShowSettingsCommand.ExecuteAsync();
                } 
                else if(args.InvokedItem.Equals(Strings.AboutTitle))
                {
                    await ((MainViewModel)ViewModel).ShowAboutCommand.ExecuteAsync();
                }
            }
        }

        /// <summary>
        ///     Adjusts the view for login.
        /// </summary>
        public void SetLoginView()
        {
            NavView.OpenPaneLength = 0;
        }

        /// <summary>
        ///     Adjusts the view for the general usage.
        /// </summary>
        public void SetLoggedInView()
        {
            NavView.OpenPaneLength = 200;
        }
    }
}
