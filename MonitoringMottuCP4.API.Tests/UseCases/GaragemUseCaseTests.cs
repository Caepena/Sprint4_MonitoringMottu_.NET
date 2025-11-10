using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MonitoringMottu.Application.UseCases;
using MonitoringMottu.Domain.Entities;
using MonitoringMottu.Domain.Interfaces;
using MonitoringMottu.Domain.Pagination;
using Xunit;

namespace MonitoringMottuCP4.API.Tests.UseCases;

public class GaragemUseCaseTests
    {
        [Fact]
        public async Task GetPaginationAsync_Should_Call_Repository_With_Same_Params()
        {
            var page = 2;
            var pageSize = 25;

            var repo = new Mock<IGaragemRepository>(MockBehavior.Strict);
            
            var expected = new PageResult<Garagem>
            {
                Items = new List<Garagem>(),
                Page = page,
                PageSize = pageSize,
                Total = 0
            };

            repo.Setup(r => r.GetPaginationAsyncGaragem(page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var useCase = new GaragemUseCase(repo.Object);

            // act
            var result = await useCase.GetPaginationAsync(page, pageSize);

            // assert
            result.Should().BeSameAs(expected);
            repo.Verify(r => r.GetPaginationAsyncGaragem(page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Theory]
        [InlineData(1, 10)]
        [InlineData(3, 50)]
        [InlineData(5, 1)]
        public async Task GetPaginationAsync_Should_Return_Repository_Result(int page, int pageSize)
        {
            // arrange
            var repo = new Mock<IGaragemRepository>();

            var expected = new PageResult<Garagem>
            {
                Items = new List<Garagem>
                {

                    new Garagem("Rua das Flores, 123", 10, "Carlos Souza") 
                },
                Page = page,
                PageSize = pageSize,
                Total = 1
            };

            repo.Setup(r => r.GetPaginationAsyncGaragem(page, pageSize, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expected);

            var useCase = new GaragemUseCase(repo.Object);


            var result = await useCase.GetPaginationAsync(page, pageSize);
            
            result.Should().NotBeNull();
            result.Should().BeSameAs(expected);
            result.Items.Should().HaveCount(1);
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.Total.Should().Be(1);
        }
    }