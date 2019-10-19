﻿using System;
using GalaSoft.MvvmLight.Command;
using MoneyFox.BusinessLogic.Extensions;
using MoneyFox.Presentation.Messages;

namespace MoneyFox.Presentation.ViewModels
{
    public interface ISelectDateRangeDialogViewModel : IBaseViewModel
    {
        DateTime StartDate { get; set; }
        DateTime EndDate { get; set; }

        RelayCommand DoneCommand { get; set; }
    }

    public class SelectDateRangeDialogViewModel : BaseViewModel
    {
        private DateTime startDate;
        private DateTime endDate;

        public SelectDateRangeDialogViewModel()
        {
            StartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            EndDate = DateTime.Today.GetLastDayOfMonth();
        }

        /// <summary>
        ///     Start Date for the custom date range
        /// </summary>
        public DateTime StartDate
        {
            get => startDate;
            set
            {
                startDate = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     End Date for the custom date range
        /// </summary>
        public DateTime EndDate
        {
            get => endDate;
            set
            {
                endDate = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        ///     Selects the dates and notifies observer via the MessageHub
        /// </summary>
        public RelayCommand DoneCommand => new RelayCommand(Done);

        private void Done()
        {
            MessengerInstance.Send(new DateSelectedMessage(this, StartDate, EndDate));
        }
    }
}
