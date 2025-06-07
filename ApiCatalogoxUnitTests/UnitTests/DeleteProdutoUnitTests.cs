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

public class DeleteProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
{
    private readonly ProdutosController _controller;

    public DeleteProdutoUnitTests(ProdutosUnitTestController controller)
    {
        _controller = new ProdutosController(controller.repository, controller.mapper);
    }

    //testes para o Delete 

    [Fact]
    public async Task DeleteProdutoByIdReturnOkResult()
    {
        //Arrange
        var prodId = 2;

        //Act
        var result = await _controller.Delete(prodId) as ActionResult<ProdutoDTO>;

        //Assert
        result.Should().NotBeNull(); //verifica se o resultado não é nulo
        result.Result.Should().BeOfType<OkObjectResult>(); //verifica se o resultado é do tipo OkObjectResult
    }

    [Fact]
    public async Task DeleteProdutoByIdReturnNotFound()
    {
        //Arrange
        var prodId = 999;

        //Act
        var result = await _controller.Delete(prodId) as ActionResult<ProdutoDTO>;

        //Assert
        result.Should().NotBeNull(); //verifica se o resultado não é nulo
        result.Result.Should().BeOfType<NotFoundObjectResult>(); //verifica se o resultado é do tipo NotFoundObjectResult
    }
}
