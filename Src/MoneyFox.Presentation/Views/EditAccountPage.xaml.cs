﻿using MoneyFox.Foundation.Resources;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MoneyFox.Presentation.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EditAccountPage
	{
		public EditAccountPage ()
		{
			InitializeComponent ();

            ToolbarItems.Add(new ToolbarItem
            {
                //Command = new Command(() => ViewModel.SaveCommand.Execute()),
                Text = Strings.SaveAccountLabel,
                Priority = 0,
                Order = ToolbarItemOrder.Primary,
                Icon = "ic_save.png"
            });

            ToolbarItems.Add(new ToolbarItem
            {
                //Command = new Command(() => ViewModel.DeleteCommand.Execute()),
                Text = Strings.DeleteLabel,
                Priority = 1,
                Order = ToolbarItemOrder.Secondary
            });
        }
	}
}