﻿#nullable enable
namespace MoneyFox.Uwp.Views
{
    public sealed partial class AddAccountDialog
    {
        public AddAccountDialog()
        {
            InitializeComponent();
            DataContext = ViewModelLocator.AddAccountVm;
        }
    }
}
