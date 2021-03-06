﻿using MoneyFox.Ui.Shared.Commands;
using MoneyFox.Ui.Shared.ViewModels.Categories;

#nullable enable
namespace MoneyFox.Uwp.ViewModels.Categories
{
    public interface IModifyCategoryViewModel
    {
        /// <summary>
        /// Saves changes to a CategoryViewModel
        /// </summary>
        AsyncCommand SaveCommand { get; }

        /// <summary>
        /// Cancel the current operation
        /// </summary>
        AsyncCommand CancelCommand { get; }

        /// <summary>
        /// Selected category.
        /// </summary>
        CategoryViewModel? SelectedCategory { get; }
    }
}
