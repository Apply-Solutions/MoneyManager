using FluentAssertions;
using MoneyFox.Domain.Entities;
using MoneyFox.Domain.Exceptions;
using System;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace MoneyFox.Domain.Tests.Entities
{
    [ExcludeFromCodeCoverage]
    public class RecurringPaymentTests
    {
        [Fact]
        public void Ctor_ChargedAccountNull_ArgumentNullException()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<ArgumentNullException>(() => new RecurringPayment(DateTime.Now,
                                                                            123,
                                                                            PaymentType.Expense,
                                                                            PaymentRecurrence.Daily,
                                                                            null,
                                                                            "note"));
        }

        [Fact]
        public void Ctor_DefaultValuesSet()
        {
            // Arrange
            // Act
            var recurringPayment = new RecurringPayment(DateTime.Now,
                                                        123,
                                                        PaymentType.Expense,
                                                        PaymentRecurrence.Daily,
                                                        new Account("Foo"),
                                                        "note");

            // Assert
            recurringPayment.LastRecurrenceCreated.Should().BeAfter(DateTime.Now.AddSeconds(-1));
            recurringPayment.ModificationDate.Should().BeAfter(DateTime.Now.AddSeconds(-1));
            recurringPayment.CreationTime.Should().BeAfter(DateTime.Now.AddSeconds(-1));
        }

        [Fact]
        public void Ctor_Params_ValuesAssigned()
        {
            // Arrange
            DateTime startDate = DateTime.Now;
            const int amount = 123;
            const PaymentType type = PaymentType.Expense;
            const PaymentRecurrence recurrence = PaymentRecurrence.Daily;
            var account = new Account("foo");
            const string note = "asdf";

            // Act
            var recurringPayment = new RecurringPayment(startDate,
                                                        amount,
                                                        type,
                                                        recurrence,
                                                        account,
                                                        note);

            // Assert
            recurringPayment.StartDate.Should().Be(startDate);
            recurringPayment.IsEndless.Should().BeTrue();
            recurringPayment.Amount.Should().Be(amount);
            recurringPayment.Type.Should().Be(type);
            recurringPayment.Recurrence.Should().Be(recurrence);
            recurringPayment.ChargedAccount.Should().Be(account);
            recurringPayment.Note.Should().Be(note);
        }

        [Fact]
        public void Ctor_EndDateNull_IsEndlessTrue()
        {
            // Arrange
            // Act
            var recurringPayment = new RecurringPayment(DateTime.Now,
                                                        123,
                                                        PaymentType.Expense,
                                                        PaymentRecurrence.Daily,
                                                        new Account("Foo"),
                                                        "note");

            // Assert
            recurringPayment.IsEndless.Should().BeTrue();
        }

        [Fact]
        public void Ctor_EndDateSet_IsEndlessFalse()
        {
            // Arrange
            // Act
            var recurringPayment = new RecurringPayment(DateTime.Now,
                                                        123,
                                                        PaymentType.Expense,
                                                        PaymentRecurrence.Daily,
                                                        new Account("Foo"),
                                                        "note",
                                                        DateTime.Today);

            // Assert
            recurringPayment.IsEndless.Should().BeFalse();
            recurringPayment.EndDate.Should().Be(DateTime.Today);
        }

        [Fact]
        public void ShouldThrowAnExceptionWhenDateInvalid()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<InvalidEndDateException>(() => new RecurringPayment(DateTime.Now,
                                                                              123,
                                                                              PaymentType.Expense,
                                                                              PaymentRecurrence.Daily,
                                                                              new Account("Foo"),
                                                                              "note",
                                                                              DateTime.Today.AddDays(-1)));
        }

        [Fact]
        public void ShouldNotThrowExceptionWhenIsEndlessWithNullDateOnCtor()
        {
            // Arrange
            // Act
            var payment = new RecurringPayment(DateTime.Now,
                                 123,
                                 PaymentType.Expense,
                                 PaymentRecurrence.Daily,
                                 new Account("Foo"),
                                 "note",
                                 null);

            // Assert
            payment.Should().NotBeNull();
        }

        [Fact]
        public void ShouldNotThrowExceptionWhenIsEndlessWithNullOnUpdate()
        {
            // Arrange
            var payment = new RecurringPayment(DateTime.Now,
                                 123,
                                 PaymentType.Expense,
                                 PaymentRecurrence.Daily,
                                 new Account("Foo"),
                                 "note");
            // Act
            payment.UpdateRecurringPayment(111, PaymentRecurrence.Daily, payment.ChargedAccount, endDate: null);

            // Assert
            payment.Should().NotBeNull();
        }

        [Fact]
        public void ShouldNotThrowExceptionWhenIsEndlessWithMinDateOnUpdate()
        {
            // Arrange
            var payment = new RecurringPayment(DateTime.Now,
                                 123,
                                 PaymentType.Expense,
                                 PaymentRecurrence.Daily,
                                 new Account("Foo"),
                                 "note");
            // Act
            payment.UpdateRecurringPayment(111, PaymentRecurrence.Daily, payment.ChargedAccount, endDate: DateTime.MinValue);

            // Assert
            payment.Should().NotBeNull();
        }

        [Fact]
        public void UpdateRecurringPayment_ValuesAssigned()
        {
            // Arrange
            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            const int amount = 123;
            const PaymentType type = PaymentType.Income;
            const PaymentRecurrence recurrence = PaymentRecurrence.Daily;
            var account = new Account("foo");
            const string note = "asdf";

            var recurringPayment = new RecurringPayment(startDate,
                                                        65,
                                                        type,
                                                        PaymentRecurrence.Monthly,
                                                        new Account("1111"),
                                                        "foo");

            // Act
            recurringPayment.UpdateRecurringPayment(amount, recurrence, account, note, endDate);

            // Assert
            recurringPayment.StartDate.Should().Be(startDate);
            recurringPayment.EndDate.Should().Be(endDate);
            recurringPayment.Amount.Should().Be(amount);
            recurringPayment.Type.Should().Be(type);
            recurringPayment.Recurrence.Should().Be(recurrence);
            recurringPayment.ChargedAccount.Should().Be(account);
            recurringPayment.Note.Should().Be(note);
        }

        [Fact]
        public void UpdateRecurringPayment_CreationTimeSame()
        {
            // Arrange
            DateTime startDate = DateTime.Now.AddDays(-1);
            DateTime endDate = DateTime.Now;
            const int amount = 123;
            const PaymentType type = PaymentType.Income;
            const PaymentRecurrence recurrence = PaymentRecurrence.Daily;
            var account = new Account("foo");
            const string note = "asdf";

            var recurringPayment = new RecurringPayment(startDate,
                                                        65,
                                                        type,
                                                        PaymentRecurrence.Monthly,
                                                        new Account("1111"),
                                                        "foo");

            DateTime creationTime = recurringPayment.CreationTime;

            // Act
            recurringPayment.UpdateRecurringPayment(amount, recurrence, account, note, endDate);

            // Assert
            recurringPayment.CreationTime.Should().Be(creationTime);
        }

        [Fact]
        public void UpdateRecurringPayment_EndDateNull_IsEndless()
        {
            // Arrange
            var recurringPayment = new RecurringPayment(DateTime.Now.AddDays(-1),
                                                        65,
                                                        PaymentType.Income,
                                                        PaymentRecurrence.Monthly,
                                                        new Account("1111"),
                                                        "foo");

            // Act
            recurringPayment.UpdateRecurringPayment(123, PaymentRecurrence.Daily, new Account("123"));

            // Assert
            recurringPayment.IsEndless.Should().BeTrue();
        }

        [Fact]
        public void UpdateRecurringPayment_EndDateSet_IsEndlessFalse()
        {
            // Arrange
            var recurringPayment = new RecurringPayment(DateTime.Now.AddDays(-1),
                                                        65,
                                                        PaymentType.Income,
                                                        PaymentRecurrence.Monthly,
                                                        new Account("1111"),
                                                        "foo");

            // Act
            recurringPayment.UpdateRecurringPayment(123, PaymentRecurrence.Daily, new Account("123"), string.Empty, DateTime.Now);

            // Assert
            recurringPayment.IsEndless.Should().BeFalse();
        }

        [Fact]
        public void SetLastRecurrenceCreatedDateUpdated()
        {
            // Arrange
            DateTime startDate = DateTime.Now;
            const int amount = 123;
            const PaymentType type = PaymentType.Expense;
            const PaymentRecurrence recurrence = PaymentRecurrence.Daily;
            var account = new Account("foo");
            const string note = "asdf";

            var recurringPayment = new RecurringPayment(startDate,
                                                        amount,
                                                        type,
                                                        recurrence,
                                                        account,
                                                        note);
            // Act
            recurringPayment.SetLastRecurrenceCreatedDate();

            // Assert
            recurringPayment.LastRecurrenceCreated.Should().BeAfter(DateTime.Now.AddSeconds(-1));
            recurringPayment.ModificationDate.Should().BeAfter(DateTime.Now.AddSeconds(-1));
        }
    }
}
