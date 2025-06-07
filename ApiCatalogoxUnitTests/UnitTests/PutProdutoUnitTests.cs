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

public class PutProdutoUnitTests : IClassFixture<ProdutosUnitTestController>
{
    private readonly ProdutosController _controller;

    public PutProdutoUnitTests(ProdutosUnitTestController controller)
    {
        _controller = new ProdutosController(controller.repository, controller.mapper);
    }

    //testes de unidade para PUT

    [Fact]
    public async Task PutProdutoReturnOkResult()
    {
        //Arrange
        var prodId = 5;

        var updatedProdutoDto = new ProdutoDTO
        {
            ProdutoId = prodId,
            Nome = "Produto Atualizado - Testes",
            Descricao = "Minha descrição",
            ImagemUrl = "imagem1.jpg",
            CategoriaId = 2
        };

        //Act
        var result = await _controller.Put(prodId, updatedProdutoDto) as ActionResult<ProdutoDTO>;

        //Assert
        result.Should().NotBeNull(); //verifica se o resultado não é nulo
        result.Result.Should().BeOfType<OkObjectResult>(); //verifica se o resultado é do tipo OkObjectResult
    }

    [Fact]
    public async Task PutProdutoReturnBadRequest()
    {
        //Arrange
        var prodId = 100;

        var meuProduto = new ProdutoDTO
        {
            ProdutoId = 5,
            Nome = "Produto Atualizado - Testes",
            Descricao = "Minha descrição alterada",
            ImagemUrl = "imagem11.jpg",
            CategoriaId = 2
        };

        //Act
        var data = await _controller.Put(prodId, meuProduto);

        //Assert
        data.Result.Should().BeOfType<BadRequestResult>().Which.StatusCode.Should().Be(400);
    }
}
