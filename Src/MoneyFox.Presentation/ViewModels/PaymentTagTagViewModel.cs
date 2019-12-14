﻿using MoneyFox.Domain.Entities;

namespace MoneyFox.Presentation.ViewModels
{
    public class PaymentTagTagViewModel : BaseViewModel, IMapFrom<PaymentTag>
    {
        private int id;
        private string name;

        public int Id
        {
            get => id;
            set
            {
                if (id == value) return;
                id = value;
                RaisePropertyChanged();
            }
        }

        public string Name
        {
            get => name;
            set
            {
                if (name == value) return;
                name = value;
                RaisePropertyChanged();
            }
        }
    }
}
