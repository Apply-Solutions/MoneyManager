﻿using System;
using System.Linq;
using MoneyFox.Shared.Helpers;
using MoneyFox.Shared.Interfaces;
using MoneyFox.Shared.Model;
using MoneyFox.Shared.Resources;
using MvvmCross.Core.ViewModels;
using PropertyChanged;

namespace MoneyFox.Shared.ViewModels
{
    /// <summary>
    ///     View Model for creating and editing Categories without dialog
    /// </summary>
    [ImplementPropertyChanged]
    public class ModifyCategoryViewModel : BaseViewModel, IDisposable
    {
        private readonly IDialogService dialogService;
        private readonly IUnitOfWork unitOfWork;

        /// <summary>
        ///     Create a new instance of the view model.
        /// </summary>
        /// <param name="categoryRepository">Instance of the category repository to access the db.</param>
        public ModifyCategoryViewModel(IUnitOfWork unitOfWork, IDialogService dialogService)
        {
            this.unitOfWork = unitOfWork;
            this.dialogService = dialogService;
        }

        /// <summary>
        ///     Saves changes to a category if in edit mode <see cref="IsEdit" />  or creates
        ///     a new category.
        /// </summary>
        public MvxCommand SaveCommand => new MvxCommand(SaveCategory);

        /// <summary>
        ///     Cancel the current operation
        /// </summary>
        public MvxCommand CancelCommand => new MvxCommand(Cancel);

        /// <summary>
        ///     Delete the selected category from the database
        /// </summary>
        public MvxCommand DeleteCommand => new MvxCommand(DeleteCategory);

        /// <summary>
        ///     The currently selected category
        /// </summary>
        public Category SelectedCategory { get; set; }

        /// <summary>
        ///     Indicates if the selected category shall be edited or a new one created.
        /// </summary>
        public bool IsEdit { get; set; }

        /// <summary>
        ///     Returns the Title based on whether a category is being created or edited
        /// </summary>
        public string Title => IsEdit
            ? string.Format(Strings.EditCategoryTitle, SelectedCategory.Name)
            : Strings.AddCategoryTitle;

        public void Dispose()
        {
            unitOfWork.Dispose();
        }

        /// <summary>
        ///     Initialize the ViewModel
        /// </summary>
        /// <param name="isEdit">Indicates if a category is being edited or created</param>
        public void Init(bool isEdit)
        {
            IsEdit = isEdit;

            if (!IsEdit)
            {
                SelectedCategory = new Category();
            }
        }

        /// <summary>
        ///     Initialize the ViewModel
        /// </summary>
        /// <param name="isEdit">Indicates if a category is being edited or created</param>
        /// <param name="selectedCategoryId">If we are editing a category this is its Id</param>
        public void Init(bool isEdit, int selectedCategoryId)
        {
            IsEdit = isEdit;
            SelectedCategory = selectedCategoryId != 0
                ? unitOfWork.CategoryRepository.Data.First(x => x.Id == selectedCategoryId)
                : new Category();
        }

        private async void SaveCategory()
        {
            if (string.IsNullOrEmpty(SelectedCategory.Name))
            {
                await dialogService.ShowMessage(Strings.MandatoryFieldEmptyTitle, Strings.NameRequiredMessage);
                return;
            }

            if (!IsEdit && unitOfWork.CategoryRepository.Data.Any(
                a => string.Equals(a.Name, SelectedCategory.Name, StringComparison.CurrentCultureIgnoreCase)))
            {
                await dialogService.ShowMessage(Strings.ErrorMessageSave, Strings.DuplicateCategoryMessage);
                return;
            }

            if (unitOfWork.CategoryRepository.Save(SelectedCategory))
            {
                SettingsHelper.LastDatabaseUpdate = DateTime.Now;
                Close(this);
            }
        }

        private void DeleteCategory()
        {
            if (unitOfWork.CategoryRepository.Delete(SelectedCategory))
                SettingsHelper.LastDatabaseUpdate = DateTime.Now;
            Close(this);
        }

        private void Cancel()
        {
            // TODO: revert changes
            Close(this);
        }
    }
}