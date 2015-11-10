﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using MoneyManager.Core.Helpers;
using MoneyManager.Core.Repositories;
using MoneyManager.Core.Tests.Mocks;
using MoneyManager.Foundation;
using MoneyManager.Foundation.Interfaces;
using MoneyManager.Foundation.Model;
using MoneyManager.TestFoundation;
using Moq;
using Xunit;

namespace MoneyManager.Core.Tests.Repositories
{
    public class TransactionRepositoryTests
    {
        [Fact]
        public void SaveWithouthAccount_NoAccount_InvalidDataException()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(),
                new RecurringTransactionDataAccessMock());

            var transaction = new FinancialTransaction
            {
                Amount = 20
            };

            Assert.Throws<InvalidDataException>(() => repository.Save(transaction));
        }

        [Theory]
        [InlineData(TransactionType.Income)]
        public void Save_DifferentTransactionTypes_CorrectlySaved(TransactionType type)
        {
            var transactionDataAccessMock = new TransactionDataAccessMock();
            var repository = new TransactionRepository(transactionDataAccessMock,
                new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                TargetAccount = null,
                Amount = 20,
                Type = (int) type
            };

            repository.Save(transaction);

            transactionDataAccessMock.FinancialTransactionTestList[0].ShouldBeSameAs(transaction);
            transactionDataAccessMock.FinancialTransactionTestList[0].ChargedAccount.ShouldBeSameAs(account);
            transactionDataAccessMock.FinancialTransactionTestList[0].TargetAccount.ShouldBeNull();
            transactionDataAccessMock.FinancialTransactionTestList[0].Type.ShouldBe((int) type);
        }

        [Fact]
        public void Save_TransferTransaction_CorrectlySaved()
        {
            var transactionDataAccessMock = new TransactionDataAccessMock();
            var repository = new TransactionRepository(transactionDataAccessMock,
                new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var targetAccount = new Account
            {
                Name = "targetAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                TargetAccount = targetAccount,
                Amount = 20,
                Type = (int) TransactionType.Transfer
            };

            repository.Save(transaction);

            transactionDataAccessMock.FinancialTransactionTestList[0].ShouldBeSameAs(transaction);
            transactionDataAccessMock.FinancialTransactionTestList[0].ChargedAccount.ShouldBeSameAs(account);
            transactionDataAccessMock.FinancialTransactionTestList[0].TargetAccount.ShouldBeSameAs(targetAccount);
            transactionDataAccessMock.FinancialTransactionTestList[0].Type.ShouldBe((int) TransactionType.Transfer);
        }

        [Fact]
        public void TransactionRepository_Delete()
        {
            var transactionDataAccessMock = new TransactionDataAccessMock();
            var repository = new TransactionRepository(transactionDataAccessMock,
                new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 20
            };

            repository.Save(transaction);
            transactionDataAccessMock.FinancialTransactionTestList[0].ShouldBeSameAs(transaction);

            repository.Delete(transaction);

            transactionDataAccessMock.FinancialTransactionTestList.Any().ShouldBeFalse();
            repository.Data.Any().ShouldBeFalse();
        }

        [Fact]
        public void TransactionRepository_AccessCache()
        {
            new TransactionRepository(new TransactionDataAccessMock(), new RecurringTransactionDataAccessMock()).Data
                .ShouldNotBeNull();
        }

        [Fact]
        public void TransactionRepository_AddMultipleToCache()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(),
                new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 20
            };

            var secondTransaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 60
            };

            repository.Save(transaction);
            repository.Save(secondTransaction);

            repository.Data.Count.ShouldBe(2);
            repository.Data[0].ShouldBeSameAs(transaction);
            repository.Data[1].ShouldBeSameAs(secondTransaction);
        }

        [Fact]
        public void AddItemToDataList_SaveAccount_IsAddedToData()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(),
                new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            var transaction = new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 20,
                Type = (int) TransactionType.Transfer
            };

            repository.Save(transaction);
            repository.Data.Contains(transaction).ShouldBeTrue();
        }

        [Fact]
        public void GetUnclearedTransactions_PastDate_PastTransactions()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(),
                new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            repository.Save(new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 55,
                Date = DateTime.Today.AddDays(-1),
                Note = "this is a note!!!",
                IsCleared = false
            }
                );

            var transactions = repository.GetUnclearedTransactions();

            transactions.Count().ShouldBe(1);
        }

        /// <summary>
        ///     This Test may fail if the date overlaps with the month transition.
        /// </summary>
        [Fact]
        public void GetUnclearedTransactions_FutureDate_PastTransactions()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(),
                new RecurringTransactionDataAccessMock());

            var account = new Account
            {
                Name = "TestAccount"
            };

            repository.Save(new FinancialTransaction
            {
                ChargedAccount = account,
                Amount = 55,
                Date = Utilities.GetEndOfMonth().AddDays(-1),
                Note = "this is a note!!!",
                IsCleared = false
            }
                );

            var transactions = repository.GetUnclearedTransactions();
            transactions.Count().ShouldBe(0);

            transactions = repository.GetUnclearedTransactions(Utilities.GetEndOfMonth());
            transactions.Count().ShouldBe(1);
        }

        [Fact]
        public void GetUnclearedTransactions_AccountNull()
        {
            var repository = new TransactionRepository(new TransactionDataAccessMock(),
                new RecurringTransactionDataAccessMock());

            repository.Data.Add(new FinancialTransaction
            {
                Amount = 55,
                Date = DateTime.Today.AddDays(-1),
                Note = "this is a note!!!",
                IsCleared = false
            }
                );

            var transactions = repository.GetUnclearedTransactions();
            transactions.Count().ShouldBe(1);
        }

        [Fact]
        public void Load_FinancialTransaction_DataInitialized()
        {
            var dataAccessSetup = new Mock<IDataAccess<FinancialTransaction>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<FinancialTransaction>
            {
                new FinancialTransaction {Id = 10},
                new FinancialTransaction {Id = 15}
            });

            var categoryRepository = new TransactionRepository(dataAccessSetup.Object, new Mock<IDataAccess<RecurringTransaction>>().Object);
            categoryRepository.Load();

            categoryRepository.Data.Any(x => x.Id == 10).ShouldBeTrue();
            categoryRepository.Data.Any(x => x.Id == 15).ShouldBeTrue();
        }

        [Fact]
        public void GetRelatedTransactions_Account_CorrectAccounts()
        {
            var dataAccessSetup = new Mock<IDataAccess<FinancialTransaction>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<FinancialTransaction>());

            var repo = new TransactionRepository(dataAccessSetup.Object, new Mock<IDataAccess<RecurringTransaction>>().Object);

            var account1 = new Account {Id = 1};
            var account3 = new Account {Id = 3};

            repo.Data = new ObservableCollection<FinancialTransaction>(new List<FinancialTransaction>
            {
                new FinancialTransaction {Id = 2, ChargedAccount = account1, ChargedAccountId = account1.Id},
                new FinancialTransaction {Id = 5, ChargedAccount = account3, ChargedAccountId = account3.Id}
            });

            var result = repo.GetRelatedTransactions(account1);

            result.Count().ShouldBe(1);
            result.First().Id.ShouldBe(2);
        }

        [Fact]
        public void LoadRecurringList_NoParameters_ListWithRecurringTrans()
        {
            var dataAccessSetup = new Mock<IDataAccess<FinancialTransaction>>();
            dataAccessSetup.Setup(x => x.LoadList(null)).Returns(new List<FinancialTransaction>());

            var repo = new TransactionRepository(dataAccessSetup.Object, new Mock<IDataAccess<RecurringTransaction>>().Object)
            {
                Data = new ObservableCollection<FinancialTransaction>(new List<FinancialTransaction>
                {
                    new FinancialTransaction {Id = 1, IsRecurring = true, RecurringTransaction = new RecurringTransaction {Id = 1, IsEndless = true}, ReccuringTransactionId = 1},
                    new FinancialTransaction {Id = 2, IsRecurring = false},
                    new FinancialTransaction
                    {
                        Id = 3,
                        IsRecurring = true,
                        RecurringTransaction = new RecurringTransaction {Id = 2, IsEndless = false, EndDate = DateTime.Today.AddDays(10)},
                        ReccuringTransactionId = 2
                    },
                    new FinancialTransaction
                    {
                        Id = 4,
                        IsRecurring = true,
                        RecurringTransaction = new RecurringTransaction {Id = 3, IsEndless = false, EndDate = DateTime.Today.AddDays(-10)},
                        ReccuringTransactionId = 3
                    }
                })
            };

            var result = repo.LoadRecurringList().ToList();

            result.Count.ShouldBe(2);
            result[0].Id.ShouldBe(1);
            result[1].Id.ShouldBe(3);
        }
    }
}