using APICatalogo.Controllers;
using APICatalogo.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiCatalogoxUnitTests.UnitTests;

public class GetProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
{
    private readonly ProdutosController _controller;

    public GetProdutoUnitTests(ProdutosUnitTestController controller)
    {
        _controller = new ProdutosController(controller.repository, controller.mapper);
    }

    [Fact]
    public async Task GetProdutoByIdReturnOkResult()
    {
        //Arrange
        var prodId = 2;

        //Act
        var data = await _controller.Get(prodId);

        //Assert (xUnit)
        //var okResult = Assert.IsType<OkObjectResult>(data.Result);
        //Assert.Equal(200, okResult.StatusCode);

        //Assert (FluentAssertions)
        data.Result.Should().BeOfType<OkObjectResult>() //verifica se o resultado é do tipo OkObjectResult
                   .Which.StatusCode.Should().Be(200); //verifica se o status code é 200
    }

    [Fact]
    public async Task GetProdutoByIdReturnNotFound()
    {
        //Arrange
        var prodId = 999;

        //Act
        var data = await _controller.Get(prodId);

        //Assert
        data.Result.Should().BeOfType<NotFoundObjectResult>() //verifica se o resultado é do tipo NotFoundObjectResult
                   .Which.StatusCode.Should().Be(404); //verifica se o status code é 404
    }

    [Fact]
    public async Task GetProdutoByIdReturnBadRequest()
    {
        //Arrange
        int prodId = -1;

        //Act
        var data = await _controller.Get(prodId);

        //Assert
        data.Result.Should().BeOfType<BadRequestObjectResult>() //verifica se o resultado é do tipo BadRequestObjectResult
                   .Which.StatusCode.Should().Be(400); //verifica se o status code é 400
    }

    [Fact]
    public async Task GetProdutosReturnListOfProdutosDTO()
    {
        //Act
        var data = await _controller.Get();

        //Assert
        data.Result.Should().BeOfType<OkObjectResult>()
                   .Which.Value.Should().BeAssignableTo<IEnumerable<ProdutoDTO>>() //verifica se o valor do OkObjectResult é atribuível a IEnumerable<ProdutoDTO>
                   .And.NotBeNull(); 
    }

    [Fact]
    public async Task GetProdutosReturnBadRequestResult()
    {
        //Act
        var data = await _controller.Get();

        //Assert
        data.Result.Should().BeOfType<BadRequestResult>(); //verifica se o resultado é do tipo BadRequestResult
    }
}
