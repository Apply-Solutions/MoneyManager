﻿using System;
using System.Diagnostics.CodeAnalysis;
using MoneyFox.Domain.Exceptions;
using MoneyFox.Presentation.Groups;
using MoneyFox.Presentation.ViewModels;
using Xunit;

namespace MoneyFox.Presentation.Tests.Groups
{
    [ExcludeFromCodeCoverage]
    public class DateListGroupCollectionTests
    {
        [Fact]
        public void CreateGroups_Null_ArgumentNullExceptionThrown()
        {
            // Arrange
            // Act / Assert
            Assert.Throws<GroupListParameterNullException>(() =>
                                                               DateListGroupCollection<PaymentViewModel>.CreateGroups(null, s => "",
                                                                                                                      s => DateTime.Now));
        }
    }
}
