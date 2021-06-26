using AutoMapper;
using FluentAssertions;
using MediatR;
using MoneyFox.Application.Categories.Queries.GetCategoryBySearchTerm;
using MoneyFox.Application.Common.Interfaces;
using MoneyFox.Domain.Entities;
using MoneyFox.Ui.Shared.ViewModels.Categories;
using MoneyFox.ViewModels.Categories;
using NSubstitute;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace MoneyFox.Tests.ViewModels
{
    [ExcludeFromCodeCoverage]
    public class CategoryListViewModelTests
    {
        private readonly IMediator mediator;
        private readonly IMapper mapper;
        private readonly IDialogService dialogService;

        public CategoryListViewModelTests()
        {
            mediator = Substitute.For<IMediator>();
            mapper = Substitute.For<IMapper>();
            dialogService = Substitute.For<IDialogService>();
        }

        [Fact]
        public void ListNotNullOnCtor()
        {
            // Arrange
            // Act
            var viewModel = new CategoryListViewModel(mediator, mapper, dialogService);

            // Assert
            viewModel.Categories.Should().NotBeNull();
        }

        [Fact]
        public async Task ItemLoadedInInit()
        {
            // Arrange
            mapper.Map<List<CategoryViewModel>>(Arg.Any<List<Category>>()).Returns(new List<CategoryViewModel>
            {
                new CategoryViewModel { Name = "asdf" }
            });
            var viewModel = new CategoryListViewModel(mediator, mapper, dialogService);

            // Act
            await viewModel.InitializeAsync();

            // Assert
            await mediator.Received(1).Send(Arg.Any<GetCategoryBySearchTermQuery>(), default);
            mapper.Received(1).Map<List<CategoryViewModel>>(Arg.Any<List<Category>>());
        }
    }
}
