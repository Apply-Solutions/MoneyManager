﻿using MoneyFox.Foundation.Resources;

namespace MoneyFox.Business.ViewModels
{
    public interface IModifyCategoryGroupViewModel : IBaseViewModel
    {
        bool IsEdit { get; }
        string Title { get;  }

        CategoryGroupViewModel SelectedGroup { get;  }
    }

    public class ModifyCategoryGroupViewModel : BaseViewModel, IModifyCategoryGroupViewModel
    {
        public bool IsEdit { get; set; }
        public string Title => IsEdit ? Strings.EditCategoryGroupTitle : Strings.AddCategoryGroupTitle;
        public CategoryGroupViewModel SelectedGroup { get; }
    }
}
