using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Moq;
using MonitoringMottu.API.Controllers;
using MonitoringMottu.API.Models;
using MonitoringMottu.Domain.Entities;
using MonitoringMottu.Domain.Interfaces;
using MonitoringMottu.Domain.Pagination;
using Xunit;

namespace MonitoringMottuCP4.API.Test.Controllers
{
    public class MotoControllerTest
    {
        private readonly Mock<IRepository<Moto>> _repoMock;
        private readonly Mock<IMotoRepository> _repoPagedMock;
        private readonly LinkGenerator _linkGen;
        private readonly MotoController _controller;

        public MotoControllerTest()
        {
            _repoMock = new Mock<IRepository<Moto>>();
            _repoPagedMock = new Mock<IMotoRepository>();
            _linkGen = new Mock<LinkGenerator>().Object;

            _controller = new MotoController(_repoMock.Object, _linkGen, _repoPagedMock.Object);
        }

        [Fact]
        public async Task GetMotos_ReturnsOk_WithList()
        {
            // Arrange
            var expectedList = new List<Moto>
            {
                new Moto("CG 160", "Honda", "Vermelha", "ABC1D23", Guid.NewGuid())
            };

            _repoMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(expectedList);

            // Act
            var result = await _controller.GetMotos() as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            var value = result.Value as IEnumerable<Moto>;
            value.Should().NotBeNull();
            value!.Should().HaveCount(1);
            _repoMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetMoto_ReturnsOk_WhenFound()
        {
            // Arrange
            var moto = new Moto("Fan 160", "Honda", "Preta", "XYZ1A23", Guid.NewGuid());
            _repoMock.Setup(r => r.GetByIdAsync(moto.Id))
                .ReturnsAsync(moto);

            // Act
            var result = await _controller.GetMoto(moto.Id) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.Value.Should().Be(moto);
            result.StatusCode.Should().Be(StatusCodes.Status200OK);
            _repoMock.Verify(r => r.GetByIdAsync(moto.Id), Times.Once);
        }

        [Fact]
        public async Task GetMoto_ReturnsNotFound_WhenNull()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Moto?)null);

            // Act
            var result = await _controller.GetMoto(id) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task PostMoto_ReturnsCreated_WhenValid()
        {
            // Arrange
            var input = new MotoInputModel
            {
                Modelo = "Fazer 250",
                Marca = "Yamaha",
                Cor = "Azul",
                Placa = "DEF4G56",
                GaragemId = Guid.NewGuid()
            };

            _repoMock.Setup(r => r.AddAsync(It.IsAny<Moto>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.PostMoto(input) as CreatedAtActionResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status201Created);
            result.Value.Should().BeOfType<Moto>();
            _repoMock.Verify(r => r.AddAsync(It.IsAny<Moto>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PutMoto_ReturnsNoContent_WhenUpdated()
        {
            // Arrange
            var id = Guid.NewGuid();
            var existing = new Moto("CG 160", "Honda", "Vermelha", "ABC1D23", Guid.NewGuid());

            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(existing);
            _repoMock.Setup(r => r.UpdateAsync(It.IsAny<Moto>())).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            var input = new MotoInputModel
            {
                Modelo = "Fan 160",
                Marca = "Honda",
                Cor = "Preta",
                Placa = "XYZ1A23",
                GaragemId = Guid.NewGuid()
            };

            // Act
            var result = await _controller.PutMoto(id, input) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            _repoMock.Verify(r => r.UpdateAsync(It.IsAny<Moto>()), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task PutMoto_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Moto?)null);

            var input = new MotoInputModel
            {
                Modelo = "Fan 160",
                Marca = "Honda",
                Cor = "Preta",
                Placa = "XYZ1A23",
                GaragemId = Guid.NewGuid()
            };

            // Act
            var result = await _controller.PutMoto(id, input) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteMoto_ReturnsNoContent_WhenDeleted()
        {
            // Arrange
            var id = Guid.NewGuid();
            var moto = new Moto("Fan 160", "Honda", "Preta", "XYZ1A23", Guid.NewGuid());

            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(moto);
            _repoMock.Setup(r => r.DeleteAsync(id)).Returns(Task.CompletedTask);
            _repoMock.Setup(r => r.SaveChangesAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.DeleteMoto(id) as NoContentResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status204NoContent);
            _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
            _repoMock.Verify(r => r.SaveChangesAsync(), Times.Once);
        }

        [Fact]
        public async Task DeleteMoto_ReturnsNotFound_WhenNotExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((Moto?)null);

            // Act
            var result = await _controller.DeleteMoto(id) as NotFoundObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status404NotFound);
            _repoMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task GetPaged_ReturnsOk_WithValidData()
        {
            // Arrange
            var moto = new Moto("CG 160", "Honda", "Vermelha", "ABC1D23", Guid.NewGuid());
            var pageResult = new PageResult<Moto>
            {
                Items = new List<Moto> { moto },
                Page = 1,
                PageSize = 10,
                Total = 1
            };

            _repoPagedMock.Setup(r => r.GetPaginationAsyncMoto(1, 10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pageResult);
            
            var httpContext = new DefaultHttpContext();
            httpContext.Request.Scheme = "http";
            httpContext.Request.Host = new HostString("localhost", 5000);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };

            // Act
            var result = await _controller.GetPaged(1, 10) as OkObjectResult;

            // Assert
            result.Should().NotBeNull();
            result!.StatusCode.Should().Be(StatusCodes.Status200OK);
            _repoPagedMock.Verify(r => r.GetPaginationAsyncMoto(1, 10, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
