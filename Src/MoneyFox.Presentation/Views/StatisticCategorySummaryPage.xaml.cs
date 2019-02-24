﻿using MoneyFox.Foundation.Resources;
using MoneyFox.Presentation.Dialogs;
using MoneyFox.ServiceLayer.ViewModels;
using MvvmCross;
using Rg.Plugins.Popup.Extensions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MoneyFox.Presentation.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class StatisticCategorySummaryPage
	{
		public StatisticCategorySummaryPage ()
		{
            InitializeComponent ();
		    Title = Strings.CategorySummaryTitle;

		    var filterItem = new ToolbarItem
		    {
		        Command = new Command(OpenDialog),
		        Text = Strings.SelectDateLabel,
		        Priority = 0,
		        Order = ToolbarItemOrder.Primary
		    };

		    ToolbarItems.Add(filterItem);
        }

	    private async void OpenDialog()
	    {
	        if (Mvx.IoCProvider.CanResolve<SelectDateRangeDialogViewModel>())
	        {
	            await Navigation.PushPopupAsync(new DateSelectionPopup
	            {
	                BindingContext = Mvx.IoCProvider.Resolve<SelectDateRangeDialogViewModel>()
	            }).ConfigureAwait(true);
	        }
	    }
    }
}