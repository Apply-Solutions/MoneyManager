﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using MoneyFox.Business.Manager;
using MoneyFox.Business.ViewModels;
using MoneyFox.Foundation;
using MoneyFox.Foundation.DataModels;
using MoneyFox.Foundation.Interfaces;
using MoneyFox.Foundation.Interfaces.Repositories;
using Moq;
using MvvmCross.Plugins.Messenger;
using Ploeh.AutoFixture;
using Xunit;
using XunitShouldExtension;

namespace MoneyFox.Business.Tests.ViewModels
{
    public class ModifyPaymentViewModelTest
    {
        [Theory]
        [InlineData(PaymentType.Income)]
        [InlineData(PaymentType.Expense)]
        public void Init_IncomeNotEditing_PropertiesSetupCorrectly(PaymentType type)
        {
            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel>());

            var paymentManager = new PaymentManager(new Mock<IPaymentRepository>().Object, 
                accountRepoMock.Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object);

            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupAllProperties();

            var viewmodel = new ModifyPaymentViewModel(new Mock<IPaymentRepository>().Object,
                accountRepoMock.Object,
                new Mock<IDialogService>().Object,
                paymentManager,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object);

            viewmodel.Init(type);

            //Execute and Assert
            viewmodel.SelectedPayment.ShouldNotBeNull();
            viewmodel.SelectedPayment.Type.ShouldBe(type);
            viewmodel.SelectedPayment.IsTransfer.ShouldBeFalse();
            viewmodel.SelectedPayment.IsRecurring.ShouldBeFalse();
        }

        [Fact]
        public void Init_TransferNotEditing_PropertiesSetupCorrectly() 
        {
            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(x => x.GetList(null))
                .Returns(new List<AccountViewModel> { new AccountViewModel { Id = 3 } });

            var paymentManager = new PaymentManager(new Mock<IPaymentRepository>().Object,
                accountRepoMock.Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object);

            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupAllProperties();

            var viewmodel = new ModifyPaymentViewModel(new Mock<IPaymentRepository>().Object,
                accountRepoMock.Object,
                new Mock<IDialogService>().Object,
                paymentManager,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object);

            //Execute and Assert
            viewmodel.Init(PaymentType.Transfer);
            viewmodel.SelectedPayment.Type.ShouldBe(PaymentType.Transfer);
            viewmodel.SelectedPayment.IsTransfer.ShouldBeTrue();
            viewmodel.SelectedPayment.IsRecurring.ShouldBeFalse();
        }

        [Fact]
        public void Save_UpdateTimeStamp()
        {
            var selectedPayment = new PaymentViewModel
            {
                ChargedAccountId = 3,
                ChargedAccount = new AccountViewModel {Id = 3, Name = "3"}
            };

            var localDateSetting = DateTime.MinValue;

            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupSet(x => x.LastDatabaseUpdate = It.IsAny<DateTime>())
                .Callback((DateTime x) => localDateSetting = x);

            var paymentRepoSetup = new Mock<IPaymentRepository>();
            paymentRepoSetup.Setup(x => x.GetList(null)).Returns(new List<PaymentViewModel>());
            paymentRepoSetup.Setup(x => x.FindById(It.IsAny<int>())).Returns(selectedPayment);
            paymentRepoSetup.Setup(x => x.Save(selectedPayment)).Returns(true);

            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(x => x.GetList(null))
                .Returns(new List<AccountViewModel> { new AccountViewModel { Id = 3, Name = "3" } });

            var dialogService = new Mock<IDialogService>().Object;

            var paymentManagerSetup = new Mock<IPaymentManager>();
            paymentManagerSetup.Setup(x => x.SavePayment(It.IsAny<PaymentViewModel>())).Returns(true);
            paymentManagerSetup.Setup(x => x.AddPaymentAmount(It.IsAny<PaymentViewModel>())).Returns(true);

            var viewmodel = new ModifyPaymentViewModel(paymentRepoSetup.Object,
                accountRepoMock.Object,
                dialogService, 
                paymentManagerSetup.Object,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object)
            {
                SelectedPayment = selectedPayment
            };
            viewmodel.SaveCommand.Execute();

            localDateSetting.ShouldBeGreaterThan(DateTime.Now.AddSeconds(-1));
            localDateSetting.ShouldBeLessThan(DateTime.Now.AddSeconds(1));
        }

        [Theory]
        [InlineData(PaymentType.Income)]
        [InlineData(PaymentType.Expense)]
        public void Init_IncomeEditing_PropertiesSetupCorrectly(PaymentType type)
        {
            var testEndDate = new DateTime(2099, 1, 31);

            var paymentRepoSetup = new Mock<IPaymentRepository>();
            paymentRepoSetup.Setup(x => x.FindById(It.IsAny<int>())).Returns(new PaymentViewModel
            {
                Type = type,
                IsRecurring = true,
                RecurringPayment = new RecurringPaymentViewModel
                {
                    EndDate = testEndDate
                }
            });

            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel>());

            var paymentManager = new PaymentManager(paymentRepoSetup.Object,
                accountRepoMock.Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object);

            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupAllProperties();

            var viewmodel = new ModifyPaymentViewModel(paymentRepoSetup.Object,
                accountRepoMock.Object,
                new Mock<IDialogService>().Object,
                paymentManager,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object);

            viewmodel.Init(type, 12);

            //Execute and Assert
            viewmodel.SelectedPayment.ShouldNotBeNull();
            viewmodel.SelectedPayment.Type.ShouldBe(type);
            viewmodel.SelectedPayment.IsTransfer.ShouldBeFalse();
            viewmodel.SelectedPayment.IsRecurring.ShouldBeTrue();
            viewmodel.SelectedPayment.RecurringPayment.EndDate.ShouldBe(testEndDate);
            viewmodel.SelectedPayment.RecurringPayment.IsEndless.ShouldBeFalse();
        }

        [Fact]
        public void Init_TransferEditing_PropertiesSetupCorrectly() {
            var testEndDate = new DateTime(2099, 1, 31);

            var paymentRepoSetup = new Mock<IPaymentRepository>();
            paymentRepoSetup.Setup(x => x.FindById(It.IsAny<int>())).Returns(new PaymentViewModel {
                Type = PaymentType.Transfer,
                IsRecurring = true,
                RecurringPayment = new RecurringPaymentViewModel {
                    EndDate = testEndDate
                }
            });

            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel>());

            var paymentManager = new PaymentManager(paymentRepoSetup.Object,
                            accountRepoMock.Object,
                            new Mock<IRecurringPaymentRepository>().Object,
                            new Mock<IDialogService>().Object);

            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupAllProperties();

            var viewmodel = new ModifyPaymentViewModel(paymentRepoSetup.Object,
                accountRepoMock.Object,
                new Mock<IDialogService>().Object,
                paymentManager,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object);

            viewmodel.Init(PaymentType.Income, 12);

            //Execute and Assert
            viewmodel.SelectedPayment.ShouldNotBeNull();
            viewmodel.SelectedPayment.Type.ShouldBe(PaymentType.Transfer);
            viewmodel.SelectedPayment.IsTransfer.ShouldBeTrue();
            viewmodel.SelectedPayment.IsRecurring.ShouldBeTrue();
            viewmodel.SelectedPayment.RecurringPayment.EndDate.ShouldBe(testEndDate);
            viewmodel.SelectedPayment.RecurringPayment.IsEndless.ShouldBeFalse();
        }

        [Fact]
        public void SelectedItemChangedCommand_UpdatesCorrectely()
        {
            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupAllProperties();

            var accountRepoMock = new Mock<IAccountRepository>();
            accountRepoMock.Setup(x => x.GetList(null)).Returns(new List<AccountViewModel>());

            var paymentManager = new PaymentManager(new Mock<IPaymentRepository>().Object,
                accountRepoMock.Object,
                new Mock<IRecurringPaymentRepository>().Object,
                new Mock<IDialogService>().Object);

            var viewmodel = new ModifyPaymentViewModel(new Mock<IPaymentRepository>().Object,
                accountRepoMock.Object,
                new Mock<IDialogService>().Object,
                paymentManager,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object);

            viewmodel.Init(PaymentType.Income);

            AccountViewModel test1 = new AccountViewModel();//target AccountViewModel
            AccountViewModel test2 = new AccountViewModel();//charge AccountViewModel
            viewmodel.TargetAccounts.Add(test1);
            viewmodel.ChargedAccounts.Add(test1);
            viewmodel.TargetAccounts.Add(test2);
            viewmodel.ChargedAccounts.Add(test2);

            viewmodel.SelectedPayment.TargetAccount = test1;
            viewmodel.SelectedPayment.ChargedAccount = test2;

            viewmodel.SelectedItemChangedCommand.Execute();

            viewmodel.ChargedAccounts.Contains(viewmodel.SelectedPayment.ChargedAccount).ShouldBeTrue();
            viewmodel.TargetAccounts.Contains(viewmodel.SelectedPayment.TargetAccount).ShouldBeTrue();
            viewmodel.ChargedAccounts.Contains(viewmodel.SelectedPayment.TargetAccount).ShouldBeFalse();
            viewmodel.TargetAccounts.Contains(viewmodel.SelectedPayment.ChargedAccount).ShouldBeFalse();
        }

        [Theory]
        [InlineData(PaymentRecurrence.Daily)]
        [InlineData(PaymentRecurrence.DailyWithoutWeekend)]
        [InlineData(PaymentRecurrence.Weekly)]
        [InlineData(PaymentRecurrence.Biweekly)]
        [InlineData(PaymentRecurrence.Monthly)]
        [InlineData(PaymentRecurrence.Yearly)]
        public void SaveCommand_Recurrence_RecurrenceSetCorrectly(PaymentRecurrence recurrence)
        {
            //setup
            var testPayment = new PaymentViewModel();

            var paymentRepoSetup = new Mock<IPaymentRepository>();
            var accountRepoMock = new Mock<IAccountRepository>();

            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupAllProperties();

            var paymentManagerMock = new Mock<IPaymentManager>();
            paymentManagerMock.Setup(x => x.SavePayment(It.IsAny<PaymentViewModel>())).Callback((PaymentViewModel payment) => testPayment = payment);

            var viewmodel = new ModifyPaymentViewModel(paymentRepoSetup.Object,
                accountRepoMock.Object,
                new Mock<IDialogService>().Object,
                paymentManagerMock.Object,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object);

            viewmodel.Init(PaymentType.Income);
            viewmodel.SelectedPayment.ChargedAccount = new AccountViewModel();
            viewmodel.SelectedPayment.IsRecurring = true;
            viewmodel.Recurrence = recurrence;

            // execute
            viewmodel.SaveCommand.Execute();

            //Assert
            testPayment.RecurringPayment.ShouldNotBeNull();
            testPayment.RecurringPayment.Recurrence.ShouldBe(recurrence);
        }

        [Theory]
        [InlineData("35", 35, "de-CH")]
        [InlineData("35.5", 35.5, "de-CH")]
        [InlineData("35,5", 35.5, "de-CH")]
        [InlineData("35.50", 35.5, "de-CH")]
        [InlineData("35,50", 35.5, "de-CH")]
        [InlineData("3,500.5", 3500.5, "de-CH")]
        [InlineData("3,500.50", 3500.5, "de-CH")]
        [InlineData("3.500,5", 3500.5, "de-CH")]
        [InlineData("3.500,50", 3500.5, "de-CH")]
        [InlineData("35", 35, "de-DE")]
        [InlineData("35,5", 35.5, "de-DE")]
        [InlineData("35,50", 35.5, "de-DE")]
        [InlineData("35.5", 35.5, "de-DE")]
        [InlineData("35.50", 35.5, "de-DE")]
        [InlineData("3,500.5", 3500.5, "de-DE")]
        [InlineData("3,500.50", 3500.5, "de-DE")]
        [InlineData("3.500,5", 3500.5, "de-DE")]
        [InlineData("3.500,50", 3500.5, "de-DE")]
        [InlineData("35", 35, "en-GB")]
        [InlineData("35,5", 35.5, "en-GB")]
        [InlineData("35,50", 35.5, "en-GB")]
        [InlineData("35.5", 35.5, "en-GB")]
        [InlineData("35.50", 35.5, "en-GB")]
        [InlineData("3,500.5", 3500.5, "en-GB")]
        [InlineData("3,500.50", 3500.5, "en-GB")]
        [InlineData("3.500,5", 3500.5, "en-GB")]
        [InlineData("3.500,50", 3500.5, "en-GB")]
        [InlineData("35", 35, "en-US")]
        [InlineData("35,5", 35.5, "en-US")]
        [InlineData("35,50", 35.5, "en-US")]
        [InlineData("35.5", 35.5, "en-US")]
        [InlineData("35.50", 35.5, "en-US")]
        [InlineData("3,500.5", 3500.5, "en-US")]
        [InlineData("3,500.50", 3500.5, "en-US")]
        [InlineData("3.500,5", 3500.5, "en-US")]
        [InlineData("3.500,50", 3500.5, "en-US")]
        [InlineData("35", 35, "it-IT")]
        [InlineData("35,5", 35.5, "it-IT")]
        [InlineData("35,50", 35.5, "it-IT")]
        [InlineData("35.5", 35.5, "it-IT")]
        [InlineData("35.50", 35.5, "it-IT")]
        [InlineData("3,500.5", 3500.5, "it-IT")]
        [InlineData("3,500.50", 3500.5, "it-IT")]
        [InlineData("3.500,5", 3500.5, "it-IT")]
        public void AmountString_CorrectConvertedAmount(string amount, double convertedAmount, string culture)
        {
            Thread.CurrentThread.CurrentCulture = new CultureInfo(culture, false);

            // Setup
            var paymentRepoSetup = new Mock<IPaymentRepository>();
            var accountRepoMock = new Mock<IAccountRepository>();

            var settingsManagerMock = new Mock<ISettingsManager>();
            settingsManagerMock.SetupAllProperties();

            var paymentManagerMock = new Mock<IPaymentManager>();

            var testPayment = new Fixture().Create<PaymentViewModel>();
            testPayment.Amount = 0;

            var viewmodel = new ModifyPaymentViewModel(paymentRepoSetup.Object,
                accountRepoMock.Object,
                new Mock<IDialogService>().Object,
                paymentManagerMock.Object,
                settingsManagerMock.Object,
                new Mock<IMvxMessenger>().Object,
                new Mock<IBackupManager>().Object)
            {
                SelectedPayment = testPayment,
            };

            // Execute
            viewmodel.AmountString = amount;

            // Assert
            viewmodel.AmountString.ShouldBe(convertedAmount.ToString("N", CultureInfo.CurrentCulture));
        }
    }
}