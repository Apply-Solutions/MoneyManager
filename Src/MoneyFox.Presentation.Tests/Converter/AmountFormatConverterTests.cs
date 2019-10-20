﻿using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using MoneyFox.Presentation.Converter;
using Should;
using Xunit;

namespace MoneyFox.Presentation.Tests.Converter
{
    [ExcludeFromCodeCoverage]
    public class AmountFormatConverterTests
    {
        [Theory] // Currencies: 
        [InlineData("fr-Fr")] // France
        [InlineData("de-DE")] // Germany
        [InlineData("de-CH")] // Switzerland
        [InlineData("en-US")] // United States
        [InlineData("en-GB")] // United Kingdom
        [InlineData("it-IT")] // Italian
        public void Convert_NegativeAndDifferentCurrency_FloatAmount_ValidString(string cultureID)
        {
            var testCulture = new CultureInfo(cultureID, false);
            decimal amount = -88.23m;
            var positiveAmount = 88.23m;
            new AmountFormatConverter().Convert(amount, null, null, testCulture)
                                       .ShouldEqual("-" + testCulture.NumberFormat.CurrencySymbol + positiveAmount.ToString(testCulture));
        }
    }
}
