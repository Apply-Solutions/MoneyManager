﻿using FluentAssertions;
using MoneyFox.Application.Accounts.Queries.GetTotalEndOfMonthBalance;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Application.Tests.Infrastructure;
using MoneyFox.Domain;
using MoneyFox.Domain.Entities;
using MoneyFox.Persistence;
using NSubstitute;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace MoneyFox.Application.Tests.Accounts.Queries.GetTotalEndOfMonthBalance
{
    [ExcludeFromCodeCoverage]
    public class GetTotalEndOfMonthBalanceQueryTests : IDisposable
    {
        private readonly EfCoreContext context;
        private readonly IContextAdapter contextAdapterMock;

        public GetTotalEndOfMonthBalanceQueryTests()
        {
            context = InMemoryEfCoreContextFactory.Create();

            contextAdapterMock = Substitute.For<IContextAdapter>();
            contextAdapterMock.Context.Returns(context);
        }

        public void Dispose()
        {
            SystemDateHelper.Today = DateTime.Today;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            InMemoryEfCoreContextFactory.Destroy(context);
        }

        [Fact]
        public async Task GetIncludedAccountBalanceSummary_CorrectSum()
        {
            // Arrange
            SystemDateHelper.Today = new DateTime(2020, 09, 05);
            var accountIncluded = new Account("test", 100);
            var payment = new Payment(new DateTime(2020, 09, 25), 50, PaymentType.Expense, accountIncluded);

            await context.AddAsync(accountIncluded);
            await context.AddAsync(payment);
            await context.SaveChangesAsync();

            // Act
            decimal result = await new GetTotalEndOfMonthBalanceQuery.Handler(contextAdapterMock).Handle(
                new GetTotalEndOfMonthBalanceQuery(), default);

            // Assert
            result.Should().Be(50);
        }
    }
}
