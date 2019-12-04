﻿using System.Diagnostics.CodeAnalysis;
using System.IO;
using MoneyFox.Application.Extensions;
using Xunit;

namespace MoneyFox.BusinessLogic.Tests.Extensions
{
    [ExcludeFromCodeCoverage]
    public class StreamExtensionTests
    {
        [Fact]
        public void ReadToEnd()
        {
            // Arrange
            var bytes = new byte[] {12, 22, 25, 23};
            var stream = new MemoryStream(bytes);

            // Act
            byte[] resultBytes = stream.ReadToEnd();
            stream.Close();

            // Assert
            Assert.Equal(bytes, resultBytes);
        }
    }
}
