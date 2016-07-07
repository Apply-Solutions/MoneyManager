﻿using System;
using MvvmCross.Plugins.Messenger;

namespace MoneyFox.Shared.Messages
{
    public class DateSelectedMessage : MvxMessage
    {
        public DateSelectedMessage(object sender, DateTime startDate, DateTime endDate)
            : base(sender)
        {
            StartDate = startDate;
            EndDate = endDate;
        }

        /// <summary>
        ///     The selected start date
        /// </summary>
        public DateTime StartDate { get; }

        /// <summary>
        ///     The selected end date
        /// </summary>
        public DateTime EndDate { get; }
    }
}