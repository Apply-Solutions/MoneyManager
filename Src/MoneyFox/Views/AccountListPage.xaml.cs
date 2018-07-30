﻿using System;
using MoneyFox.Business.ViewModels;
using MoneyFox.Foundation.Resources;
using MvvmCross.Forms.Presenters.Attributes;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MoneyFox.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	[MvxTabbedPagePresentation(WrapInNavigationPage = false, Title = "Accounts", Icon = "ic_accounts_black")]
    public partial class AccountListPage
	{
		public AccountListPage ()
		{
			InitializeComponent ();
		    AccountsList.ItemTapped += (sender, args) =>
		    {
		        AccountsList.SelectedItem = null;
		        ViewModel.OpenOverviewCommand.Execute(args.Item);
		    };
		    Title = Strings.AccountsTitle;
		}

	    private async void AddItem_Clicked(object sender, EventArgs e)
	    {
	        var action = await DisplayActionSheet(Strings.AddTitle, Strings.CancelLabel, null, Strings.AddAccountLabel, Strings.AddExpenseLabel, Strings.AddIncomeLabel, Strings.AddTransferLabel);

	        if (action == Strings.AddAccountLabel)
	        {
	            await ViewModel.ViewActionViewModel.GoToAddAccountCommand.ExecuteAsync();
	        }
            else if (action == Strings.AddExpenseLabel)
	        {
	            await ViewModel.ViewActionViewModel.GoToAddExpenseCommand.ExecuteAsync();
	        }
            else if (action == Strings.AddIncomeLabel)
	        {
	            await ViewModel.ViewActionViewModel.GoToAddIncomeCommand.ExecuteAsync();
	        }
            else if (action == Strings.AddTransferLabel)
	        {
	            await ViewModel.ViewActionViewModel.GoToAddTransferCommand.ExecuteAsync();
	        }
        }

	    private void EditAccount(object sender, EventArgs e)
	    {
            if (!(sender is MenuItem menuItem)) return;

            ViewModel.EditAccountCommand.ExecuteAsync(menuItem.CommandParameter as AccountViewModel);
	    }

	    private void DeleteAccount(object sender, EventArgs e)
	    {
            if (!(sender is MenuItem menuItem)) return;

            ViewModel.DeleteAccountCommand.ExecuteAsync(menuItem.CommandParameter as AccountViewModel);
	    }
    }
}