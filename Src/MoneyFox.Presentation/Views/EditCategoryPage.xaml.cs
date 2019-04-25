﻿using System.Reactive.Disposables;
using MoneyFox.Foundation.Resources;
using ReactiveUI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MoneyFox.Presentation.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class EditCategoryPage 
	{
		public EditCategoryPage ()
		{
			InitializeComponent ();

            this.WhenActivated(disposables => {
                Title = ViewModel.Title;

                ToolbarItems.Add(new ToolbarItem {
                    Command = new Command(() => ViewModel.SaveCommand.Execute()),
                    Text = Strings.SaveCategoryLabel,
                    Priority = 0,
                    Order = ToolbarItemOrder.Primary,
                    Icon = "ic_save.png"
                });

                ToolbarItems.Add(new ToolbarItem {
                    Command = new Command(() => ViewModel.DeleteCommand.Execute()),
                    Text = Strings.DeleteLabel,
                    Priority = 1,
                    Order = ToolbarItemOrder.Secondary
                });


                this.OneWayBind(ViewModel, vm => vm.DeleteCommand, v => v.DeleteCategoryButton.Command).DisposeWith(disposables);
                this.OneWayBind(ViewModel, vm => vm.Resources["DeleteLabel"],
                                v => v.DeleteCategoryButton.Text).DisposeWith(disposables);
            });
        }
	}
}