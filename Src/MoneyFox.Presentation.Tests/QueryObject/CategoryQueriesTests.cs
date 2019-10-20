﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using MoneyFox.Presentation.QueryObject;
using MoneyFox.Presentation.ViewModels;
using Should;
using Xunit;

namespace MoneyFox.Presentation.Tests.QueryObject
{
    [ExcludeFromCodeCoverage]
    public class CategoryQueriesTests
    {
        [Theory]
        [InlineData("Foo3", true)]
        [InlineData("Foo5", false)]
        [InlineData("Foo", false)]
        [InlineData("abc", false)]
        public async Task AnyWithName(string searchName, bool expectedResult)
        {
            // Arrange
            IQueryable<CategoryViewModel> categoryQueryList = new List<CategoryViewModel>
                                                              {
                                                                  new CategoryViewModel {Name = "Foo2"},
                                                                  new CategoryViewModel {Name = "Foo3"},
                                                                  new CategoryViewModel {Name = "Foo1"}
                                                              }
                                                              .AsQueryable()
                                                              .BuildMock()
                                                              .Object;

            // Act
            bool result = await categoryQueryList.AnyWithNameAsync(searchName);

            // Assert
            result.ShouldEqual(expectedResult);
        }

        [Theory]
        [InlineData("Foo3", 1)]
        [InlineData("Foo5", 0)]
        [InlineData("Foo", 3)]
        [InlineData("abc", 0)]
        public void WhereNameContains(string searchName, int expectedCount)
        {
            // Arrange
            IQueryable<CategoryViewModel> categoryQueryList = new List<CategoryViewModel>
                                                              {
                                                                  new CategoryViewModel {Name = "Foo2"},
                                                                  new CategoryViewModel {Name = "Foo3"},
                                                                  new CategoryViewModel {Name = "Foo1"}
                                                              }
                                                              .AsQueryable()
                                                              .BuildMock()
                                                              .Object;

            // Act
            IQueryable<CategoryViewModel> result = categoryQueryList.WhereNameContains(searchName);

            // Assert
            result.Count().ShouldEqual(expectedCount);
        }
    }
}
