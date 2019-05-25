﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;
using GenericServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Toolkit.Uwp.Notifications;
using MoneyFox.Foundation;
using MoneyFox.Foundation.Resources;
using MoneyFox.ServiceLayer.ViewModels;
using MoneyFox.Windows.Business;
using MoneyFox.Windows.Business.Tiles;

namespace MoneyFox.Uwp.Business.Tiles
{
    public class LiveTileManager
    {
        private const int NUMBER_OF_PAYMENTS = 8;
        private readonly ICrudServicesAsync crudService;

        private readonly ApplicationDataContainer localsettings = ApplicationData.Current.LocalSettings;

        public LiveTileManager(ICrudServicesAsync crudService)
        {
            this.crudService = crudService;
        }

        public async Task UpdatePrimaryLiveTile()
        {
            if (await LiveTileHelper.IsPinned())
            {
                var b = localsettings.Values["lastrun"];
                var lastRun = (string) b;

                List<string> displayLargeContent;
                List<string> displayContentMedium;
                string headerText = "";
                if (lastRun == "last")
                {
                    localsettings.Values["lastrun"] = "next";
                    headerText = Strings.LiveTileUpcommingPayments;
                    displayContentMedium = await GetPaymentsAsync(TileSizeOption.Medium, PaymentInformation.Next)
                        ;
                    displayLargeContent = await GetPaymentsAsync(TileSizeOption.Large, PaymentInformation.Next)
                        ;
                }
                else
                {
                    localsettings.Values["lastrun"] = "last";
                    headerText = Strings.LiveTilePastPayments;
                    displayContentMedium = await GetPaymentsAsync(TileSizeOption.Medium, PaymentInformation.Previous)
                        ;
                    displayLargeContent = await GetPaymentsAsync(TileSizeOption.Large, PaymentInformation.Previous)
                        ;
                }

                var content = new TileContent
                {
                    Visual = new TileVisual
                    {
                        TileMedium = GetTileBinding(headerText, displayContentMedium),
                        TileWide = GetTileBinding(headerText, displayLargeContent),
                        TileLarge = GetTileBinding(headerText, displayLargeContent)
                    }
                };

                var tn = new TileNotification(content.GetXml());
                TileUpdateManager.CreateTileUpdaterForApplication().Update(tn);
            }
        }

        public async Task UpdateSecondaryLiveTiles()
        {
            var tiles = await SecondaryTile.FindAllForPackageAsync();

           
                ;

            if (tiles == null) return;

            foreach (var item in tiles)
            {
                var acct = await crudService.ReadSingleAsync<AccountViewModel>(int.Parse(item.TileId))
                    ;
                List<string> displayContent = GetSecondarypayments(int.Parse(item.TileId));
               var content = new TileContent
                {
                    Visual = new TileVisual
                    {
                        TileSmall = new TileBinding
                        {
                            Content = new TileBindingContentAdaptive
                            {
                                Children =
                                {
                                    new AdaptiveGroup
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText
                                                    {
                                                        Text = acct.Name,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = LiveTileHelper.TruncateNumber(acct.CurrentBalance),
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        TileMedium = new TileBinding
                        {
                            Content = new TileBindingContentAdaptive
                            {
                                Children =
                                {
                                    new AdaptiveGroup
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText
                                                    {
                                                        Text = acct.Name,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileAccountBalance,
                                                            acct.CurrentBalance.ToString("C2",
                                                                CultureInfo.InvariantCulture)),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = Strings.ExpenseLabel,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileLastMonthsExpenses,
                                                            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(
                                                                DateTime.Now.AddMonths(-1).Month),
                                                            LiveTileHelper.TruncateNumber(
                                                                GetMonthExpenses(
                                                                    DateTime.Now.AddMonths(-1).Month, DateTime.Now.Year,
                                                                    acct))),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileCurrentMonthsExpenses,
                                                            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(
                                                                DateTime.Now.Month),
                                                            LiveTileHelper.TruncateNumber(
                                                                GetMonthExpenses(
                                                                    DateTime.Now.Month, DateTime.Now.Year, acct))),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        TileWide = new TileBinding
                        {
                            Content = new TileBindingContentAdaptive
                            {
                                Children =
                                {
                                    new AdaptiveGroup
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText
                                                    {
                                                        Text = acct.Name,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileAccountBalance,
                                                            acct.CurrentBalance.ToString("C2",
                                                                CultureInfo.InvariantCulture)),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = Strings.ExpenseLabel,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileLastMonthsExpenses,
                                                            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(
                                                                DateTime.Now.AddMonths(-1).Month),
                                                            GetMonthExpenses(
                                                                DateTime.Now.AddMonths(-1).Month, DateTime.Now.Year,
                                                                acct).ToString("C2", CultureInfo.InvariantCulture)),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileCurrentMonthsExpenses,
                                                            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(
                                                                DateTime.Now.Month),
                                                            GetMonthExpenses(
                                                                    DateTime.Now.Month, DateTime.Now.Year, acct)
                                                                .ToString("C2", CultureInfo.InvariantCulture)),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        },
                        TileLarge = new TileBinding
                        {
                            Content = new TileBindingContentAdaptive
                            {
                                Children =
                                {
                                    new AdaptiveGroup
                                    {
                                        Children =
                                        {
                                            new AdaptiveSubgroup
                                            {
                                                Children =
                                                {
                                                    new AdaptiveText
                                                    {
                                                        Text = acct.Name,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileAccountBalance,
                                                            acct.CurrentBalance.ToString("C2",
                                                                CultureInfo.InvariantCulture)),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = Strings.ExpenseLabel,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileLastMonthsExpenses,
                                                            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(
                                                                DateTime.Now.AddMonths(-1).Month),
                                                            GetMonthExpenses(
                                                                DateTime.Now.AddMonths(-1).Month, DateTime.Now.Year,
                                                                acct).ToString("C2", CultureInfo.InvariantCulture)),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = string.Format(CultureInfo.InvariantCulture,
                                                            Strings.LiveTileCurrentMonthsExpenses,
                                                            DateTimeFormatInfo.CurrentInfo.GetAbbreviatedMonthName(
                                                                DateTime.Now.Month),
                                                            GetMonthExpenses(
                                                                    DateTime.Now.Month, DateTime.Now.Year, acct)
                                                                .ToString("C2", CultureInfo.InvariantCulture)),
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = Strings.LiveTilePastPayments,
                                                        HintStyle = AdaptiveTextStyle.Caption
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = displayContent[0],
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = displayContent[1],
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = displayContent[2],
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = displayContent[3],
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = displayContent[4],
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    },
                                                    new AdaptiveText
                                                    {
                                                        Text = displayContent[5],
                                                        HintStyle = AdaptiveTextStyle.CaptionSubtle
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

                var tn = new TileNotification(content.GetXml());
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(item.TileId).Update(tn);
            }
        }

        public double GetMonthExpenses(int month, int year, AccountViewModel account)
        {
            var balance = 0.00;
            var allpayment = new List<LiveTilesPaymentInfo>();
            var payments = crudService.ReadManyNoTracked<PaymentViewModel>()
                .Where(x => x.ChargedAccountId == account.Id)
                .ToList();

            foreach (var item in payments)
                if (item.IsRecurring)
                {
                    if (item.Type != PaymentType.Income) allpayment.AddRange(GetRecurrence(item));
                }
                else if (item.Type != PaymentType.Income)
                {
                    CreateLiveTileInfos(item, allpayment, item.Date.Date);
                }

            var tiles = allpayment
                .Where(x => x.Date.Date.Month == month && x.Date.Date.Year == year)
                .ToList();

            foreach (var item in tiles) balance += item.Amount;

            allpayment.Clear();
            return balance;
        }

        private TileBinding GetTileBinding(string headerText, List<string> displayContentMedium)
        {
            return new TileBinding
            {
                Content = new TileBindingContentAdaptive
                {
                    Children =
                    {
                        new AdaptiveGroup
                        {
                            Children =
                            {
                                new AdaptiveSubgroup
                                {
                                    Children =
                                    {
                                        new AdaptiveText
                                        {
                                            Text = headerText,
                                            HintStyle = AdaptiveTextStyle.Caption
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[0],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[1],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[2],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[3],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[4],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[5],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[6],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        },
                                        new AdaptiveText
                                        {
                                            Text = displayContentMedium[7],
                                            HintStyle = AdaptiveTextStyle.CaptionSubtle
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }


        private List<string> GetSecondarypayments(int accountid)
        {
            var allPayments = new List<PaymentViewModel>();
            var allPayment = new List<LiveTilesPaymentInfo>();
            List<LiveTilesPaymentInfo> payments;
            allPayments.AddRange(crudService.ReadManyNoTracked<PaymentViewModel>()
                   .Where(x => x.ChargedAccountId == accountid)
                   .ToList());
            try
            {
                allPayments.AddRange(crudService.ReadManyNoTracked<PaymentViewModel>()
                    .Where(x => x.TargetAccountId == accountid)
                    .ToList());
            }
            catch (Exception e)
            {
                Console.WriteLine(e);

            }
            foreach (var item in allPayments)
            {
                if (item.IsRecurring)
                {
                    allPayment.AddRange(GetRecurrence(item));
                }
                else
                {
                    var tileInfo = new LiveTilesPaymentInfo
                    {
                        Chargeaccountname = item.ChargedAccount.Name,
                        Amount = item.Amount,
                        Date = item.Date.Date,
                        Type = item.Type
                    };
                    allPayment.Add(tileInfo);
                }
            }
                payments = allPayment.OrderByDescending(x => x.Date.Date)
                    .ThenBy(x => x.Date.Date <= DateTime.Today.Date)
                    .Take(NUMBER_OF_PAYMENTS)
                    .ToList();

            var returnList = payments.Select(x => LiveTileHelper.GetTileText(TileSizeOption.Large, x)).ToList();

            for (var i = returnList.Count; i < NUMBER_OF_PAYMENTS - 1; i++) returnList.Add(string.Empty);

            return returnList;
        }

        private async Task<List<string>> GetPaymentsAsync(TileSizeOption tileSize,
            PaymentInformation paymentInformation)
        {
            var acct = await crudService.ReadManyNoTracked<AccountViewModel>()
                .ToListAsync()
                ;
            var allPayments = new List<PaymentViewModel>();
            var allPayment = new List<LiveTilesPaymentInfo>();

            foreach (var item in acct)
            {
                allPayments.AddRange(crudService.ReadManyNoTracked<PaymentViewModel>()
                    .Where(x => x.ChargedAccountId == item.Id)
                    .ToList());

                // We have to catch here, since otherwise an Exception is thrown when no payments are there.
                try
                {
                    allPayments.AddRange(crudService.ReadManyNoTracked<PaymentViewModel>()
                        .Where(x => x.TargetAccountId == item.Id)
                        .ToList());
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            }

            foreach (var item in allPayments)
                if (item.IsRecurring)
                {
                    allPayment.AddRange(GetRecurrence(item));
                }
                else
                {
                    var tileInfo = new LiveTilesPaymentInfo
                    {
                        Chargeaccountname = item.ChargedAccount.Name,
                        Amount = item.Amount,
                        Date = item.Date.Date,
                        Type = item.Type
                    };
                    allPayment.Add(tileInfo);
                }

            List<LiveTilesPaymentInfo> payments;

            if (paymentInformation == PaymentInformation.Previous)
            {
                payments = allPayment.OrderByDescending(x => x.Date.Date)
                    .ThenBy(x => x.Date.Date <= DateTime.Today.Date)
                    .Take(NUMBER_OF_PAYMENTS)
                    .ToList();
            }
            else
            {
                payments = allPayment.OrderBy(x => x.Date.Date)
                    .ThenBy(x => x.Date.Date >= DateTime.Today.Date)
                    .Take(NUMBER_OF_PAYMENTS)
                    .ToList();
            }

            var returnList = payments.Select(x => LiveTileHelper.GetTileText(tileSize, x)).ToList();

            for (var i = returnList.Count; i < NUMBER_OF_PAYMENTS - 1; i++) returnList.Add(string.Empty);

            allPayments.Clear();
            return returnList;
        }

        private List<LiveTilesPaymentInfo> GetRecurrence(PaymentViewModel payment)
        {
            var allPayment = new List<LiveTilesPaymentInfo>();

            if (payment.RecurringPayment.IsEndless)
            {
                var startDate = payment.RecurringPayment.StartDate;
                while (DateTime.Compare(DateTime.Now, startDate) <= 0)
                    startDate = CreateLiveTileInfos(payment, allPayment, startDate);
            }
            else
            {
                var startDate = payment.RecurringPayment.StartDate;
                var endDate = payment.RecurringPayment.EndDate.Value;
                while (DateTime.Compare(startDate, endDate) <= 0)
                    startDate = CreateLiveTileInfos(payment, allPayment, startDate);
            }

            return allPayment;
        }

        private DateTime CreateLiveTileInfos(PaymentViewModel payment, List<LiveTilesPaymentInfo> allPayment,
            DateTime startDate)
        {
            var liveTilesPaymentInfo = new LiveTilesPaymentInfo
            {
                Date = startDate,
                Amount = payment.RecurringPayment?.Amount ?? payment.Amount,
                Chargeaccountname = payment.RecurringPayment == null
                    ? payment.ChargedAccount.Name
                    : payment.RecurringPayment?.ChargedAccount.Name,
                Type = payment.RecurringPayment?.Type ?? payment.Type
            };
            allPayment.Add(liveTilesPaymentInfo);
            return LiveTileHelper.AddDateByRecurrence(payment, startDate);
        }
    }
}