using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using MonitoringMottu.Application.UseCases;
using MonitoringMottu.Domain.Entities;
using MonitoringMottu.Domain.Interfaces;
using MonitoringMottu.Domain.Pagination;
using Xunit;

namespace MonitoringMottuCP4.API.Tests.UseCases;

public class MotoUseCaseTests
{
        [Fact]
    public async Task GetPaginationAsync_Should_Call_Repository_With_Same_Params()
    {
        // arrange
        var page = 2;
        var pageSize = 20;

        var repo = new Mock<IMotoRepository>(MockBehavior.Strict);

        var expected = new PageResult<Moto>
        {
            Items = new List<Moto>(),
            Page = page,
            PageSize = pageSize,
            Total = 0
        };

        repo.Setup(r => r.GetPaginationAsyncMoto(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new MotoUseCase(repo.Object);

        // act
        var result = await useCase.GetPaginationAsync(page, pageSize);

        // assert
        result.Should().BeSameAs(expected);
        repo.Verify(r => r.GetPaginationAsyncMoto(page, pageSize, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1, 10)]
    [InlineData(3, 5)]
    public async Task GetPaginationAsync_Should_Return_Repository_Result(int page, int pageSize)
    {
        // arrange
        var repo = new Mock<IMotoRepository>();

        var moto = new Moto(
            modelo: "CG 160",
            marca: "Honda",
            cor: "Vermelha",
            placa: "ABC1D23",
            garagemId: Guid.NewGuid()
        );

        var expected = new PageResult<Moto>
        {
            Items = new List<Moto> { moto },
            Page = page,
            PageSize = pageSize,
            Total = 1
        };

        repo.Setup(r => r.GetPaginationAsyncMoto(page, pageSize, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var useCase = new MotoUseCase(repo.Object);

        // act
        var result = await useCase.GetPaginationAsync(page, pageSize);

        // assert
        result.Should().NotBeNull();
        result.Should().BeSameAs(expected);
        result.Items.Should().HaveCount(1);
        result.Page.Should().Be(page);
        result.PageSize.Should().Be(pageSize);
        result.Total.Should().Be(1);
    }
}