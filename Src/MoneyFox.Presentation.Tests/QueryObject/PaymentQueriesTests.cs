﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using MoneyFox.Presentation.QueryObject;
using MoneyFox.Presentation.ViewModels;
using Xunit;

namespace MoneyFox.Presentation.Tests.QueryObject
{
    [ExcludeFromCodeCoverage]
    public class PaymentQueriesTests
    {
        [Fact]
        public void AreNotCleared()
        {
            // Arrange
            IQueryable<PaymentViewModel> paymentQueryList = new List<PaymentViewModel>
                {
                    new PaymentViewModel {Id = 1, IsCleared = false},
                    new PaymentViewModel {Id = 2, IsCleared = true},
                    new PaymentViewModel {Id = 3, IsCleared = false}
                }
                .AsQueryable();

            // Act
            List<PaymentViewModel> resultList = paymentQueryList.AreNotCleared().ToList();

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.Equal(1, resultList[0].Id);
            Assert.Equal(3, resultList[1].Id);
        }

        [Fact]
        public void AreCleared()
        {
            // Arrange
            IQueryable<PaymentViewModel> paymentQueryList = new List<PaymentViewModel>
                {
                    new PaymentViewModel {Id = 1, IsCleared = false},
                    new PaymentViewModel {Id = 2, IsCleared = true},
                    new PaymentViewModel {Id = 3, IsCleared = false}
                }
                .AsQueryable();

            // Act
            List<PaymentViewModel> resultList = paymentQueryList.AreCleared().ToList();

            // Assert
            Assert.Single(resultList);
            Assert.Equal(2, resultList[0].Id);
        }

        [Fact]
        public void AreRecurring()
        {
            // Arrange
            IQueryable<PaymentViewModel> paymentQueryList = new List<PaymentViewModel>
                {
                    new PaymentViewModel {Id = 1, IsRecurring = false},
                    new PaymentViewModel {Id = 2, IsRecurring = true},
                    new PaymentViewModel {Id = 3, IsRecurring = false}
                }
                .AsQueryable();

            // Act
            List<PaymentViewModel> resultList = paymentQueryList.AreRecurring().ToList();

            // Assert
            Assert.Single(resultList);
            Assert.Equal(2, resultList[0].Id);
        }

        [Fact]
        public void HasDateSmallerEqualsThan()
        {
            // Arrange
            IQueryable<PaymentViewModel> paymentQueryList = new List<PaymentViewModel>
                {
                    new PaymentViewModel {Id = 1, Date = DateTime.Now},
                    new PaymentViewModel {Id = 2, Date = DateTime.Now.Date.AddDays(-1)},
                    new PaymentViewModel {Id = 3, Date = DateTime.Now.Date.AddDays(-2)}
                }
                .AsQueryable();

            // Act
            List<PaymentViewModel> resultList = paymentQueryList.HasDateSmallerEqualsThan(DateTime.Now.Date.AddDays(-1)).ToList();

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.Equal(2, resultList[0].Id);
            Assert.Equal(3, resultList[1].Id);
        }

        [Fact]
        public void HasAccountId()
        {
            // Arrange
            IQueryable<PaymentViewModel> paymentQueryList = new List<PaymentViewModel>
                {
                    new PaymentViewModel {Id = 1, ChargedAccount = new AccountViewModel {Id = 1}},
                    new PaymentViewModel {Id = 2, ChargedAccount = new AccountViewModel {Id = 3}, TargetAccount = new AccountViewModel {Id = 1}},
                    new PaymentViewModel {Id = 3, ChargedAccount = new AccountViewModel {Id = 2}}
                }
                .AsQueryable();

            // Act
            List<PaymentViewModel> resultList = paymentQueryList.HasAccountId(1).ToList();

            // Assert
            Assert.Equal(2, resultList.Count);
            Assert.Equal(1, resultList[0].Id);
            Assert.Equal(2, resultList[1].Id);
        }

        [Fact]
        public void HasChargedAccountId()
        {
            // Arrange
            IQueryable<PaymentViewModel> paymentQueryList = new List<PaymentViewModel>
                {
                    new PaymentViewModel {Id = 1, ChargedAccount = new AccountViewModel {Id = 1}},
                    new PaymentViewModel {Id = 2, ChargedAccount = new AccountViewModel {Id = 3}, TargetAccount = new AccountViewModel {Id = 1}},
                    new PaymentViewModel {Id = 3, ChargedAccount = new AccountViewModel {Id = 2}}
                }
                .AsQueryable();

            // Act
            List<PaymentViewModel> resultList = paymentQueryList.HasChargedAccountId(1).ToList();

            // Assert
            Assert.Single(resultList);
            Assert.Equal(1, resultList[0].Id);
        }

        [Fact]
        public void OrderDescendingByDate()
        {
            // Arrange
            IQueryable<PaymentViewModel> paymentQueryList = new List<PaymentViewModel>
                {
                    new PaymentViewModel {Id = 1, Date = DateTime.Now.AddDays(-3)},
                    new PaymentViewModel {Id = 2, Date = DateTime.Now.AddDays(1)},
                    new PaymentViewModel {Id = 3, Date = DateTime.Now.AddDays(-2)}
                }
                .AsQueryable();

            // Act
            List<PaymentViewModel> resultList = paymentQueryList.OrderDescendingByDate().ToList();

            // Assert
            Assert.Equal(3, resultList.Count);
            Assert.Equal(2, resultList[0].Id);
            Assert.Equal(3, resultList[1].Id);
            Assert.Equal(1, resultList[2].Id);
        }
    }
}
