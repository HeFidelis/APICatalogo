﻿using APICatalogo.Context;
using APICatalogo.DTOs;
using APICatalogo.Filters;
using APICatalogo.Models;
using APICatalogo.Pagination;
using APICatalogo.Repositories;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;

namespace APICatalogo.Controllers;

[EnableRateLimiting("fixedwindow")]
[EnableCors("OrigensComAcessoPermitido")]
[Route("[controller]")]
[ApiController]
[Produces("application/json")]
//[ApiExplorerSettings(IgnoreApi = true)]
[ApiConventionType(typeof(DefaultApiConventions))]
public class CategoriasController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly ILogger<CategoriasController> _logger;
    private readonly IMapper _mapper;

    private readonly IMemoryCache _cache;
    private const string CacheCategoriasKey = "CacheCategorias";

    public CategoriasController(ILogger<CategoriasController> logger, IUnitOfWork uof, IMapper mapper, IMemoryCache cache)
    {
        _logger = logger;
        _uof = uof;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    /// Obtém uma lista de objetos CategoriadDTO
    /// </summary>
    /// <returns>Uma lista de objetos CategoriaDTO</returns>
    //[Authorize]
    [DisableRateLimiting]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    //[ProducesDefaultResponseType]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get()
    {
        if (!_cache.TryGetValue(CacheCategoriasKey, out IEnumerable<Categoria>? categorias))
        {

            categorias = await _uof.CategoriaRepository.GetAllAsync();

            if (categorias is null || !categorias.Any())
            {
                _logger.LogWarning("Não existem categorias...");
                return NotFound("Não existem categorias...");

            }
            SetCache(CacheCategoriasKey, categorias);
        }
        return Ok(categorias);
    }

    [HttpGet("pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> Get([FromQuery]
                                   CategoriasParameters categoriasParameters)
    {
        var categorias = await _uof.CategoriaRepository.GetCategoriasAsync(categoriasParameters);
        return ObterCategorias(categorias);
    }

    [HttpGet("filter/nome/pagination")]
    public async Task<ActionResult<IEnumerable<CategoriaDTO>>> GetCategoriasFiltradas([FromQuery]
                                                                          CategoriasFiltroNome categoriasFiltro)
    {
        var categoriasFiltradas = await _uof.CategoriaRepository.GetCategoriasFiltroNomeAsync(categoriasFiltro);
        return ObterCategorias(categoriasFiltradas);
    }

    private ActionResult<IEnumerable<CategoriaDTO>> ObterCategorias(IPagedList<Categoria> categorias)
    {
        var metadata = new
        {
            categorias.Count,
            categorias.PageSize,
            categorias.PageCount,
            categorias.TotalItemCount,
            categorias.HasNextPage,
            categorias.HasPreviousPage
        };

        Response.Headers.Append("X-Pagination", JsonConvert.SerializeObject(metadata));

        var categoriasDto = _mapper.Map<IEnumerable<CategoriaDTO>>(categorias);

        return Ok(categoriasDto);
    }

    /// <summary>
    /// Obtém uma Categoria pelo seu Id
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Objetos CategoriaDTO</returns>
    [DisableCors]
    [HttpGet("{id:int}", Name = "ObterCategoria")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoriaDTO>> Get(int id)
    {
        var cacheKey = GetCategoriaCacheKey(id);

        if (!_cache.TryGetValue(cacheKey, out Categoria? categoria))
        {
            categoria = await _uof.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

            if (categoria is null)
            {
                _logger.LogWarning($"Categoria com id = {id} não encontrada...");
                return NotFound($"Categoria com id = {id} não encontrada...");
            }
            SetCache(cacheKey, categoria);
        }
        return Ok(categoria);
    }

    /// <summary>
    /// Inclui uma nova Categoria
    /// </summary>
    /// <remarks>
    /// Exemplo de request:
    /// 
    ///     POST /categorias
    ///     {
    ///         "categoriaId": 1,
    ///         "nome": "categoria1",
    ///         "imagemUrl": "https://example.com/imagem1.jpg"
    ///     }
    /// </remarks>
    /// <param name="categoriaDto">objeto CategoriaDTO</param>
    /// <returns>O objeto CategoriaDTO incluída</returns>
    /// <remarks>Retorna um objeto CategoriaDTO incluído</remarks>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CategoriaDTO>> Post(CategoriaDTO categoriaDto)
    {
        if (categoriaDto is null)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        var categoria = _mapper.Map<Categoria>(categoriaDto);

        var categoriaCriada = _uof.CategoriaRepository.Create(categoria);
        await _uof.CommitAsync();

        InvalidateCacheAfterChange(categoriaCriada.CategoriaId, categoriaDto);

        return new CreatedAtRouteResult("ObterCategoria", 
            new { id = categoriaCriada.CategoriaId },
            categoriaCriada);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesDefaultResponseType]
    //[ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
    public async Task<ActionResult<CategoriaDTO>> Put(int id, CategoriaDTO categoriaDto)
    {
        if (id <= 0 || categoriaDto is null || id != categoriaDto?.CategoriaId)
        {
            _logger.LogWarning($"Dados inválidos...");
            return BadRequest("Dados inválidos");
        }

        var categoria = _mapper.Map<Categoria>(categoriaDto);

        var categoriaAtualizada = _uof.CategoriaRepository.Update(categoria);
        await _uof.CommitAsync();

        var categoriaAtualizadaDto = _mapper.Map<CategoriaDTO>(categoriaAtualizada);

        InvalidateCacheAfterChange(id, categoriaAtualizadaDto);

        return Ok(categoriaAtualizadaDto);
    }

    [Authorize(Policy ="AdminOnly")]
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<CategoriaDTO>> Delete(int id)
    {
        var categoria = await _uof.CategoriaRepository.GetAsync(c => c.CategoriaId == id);

        if (categoria == null)
        {
            _logger.LogWarning($"Categoria com id = {id} não encontrada...");
            return NotFound($"Categoria com id = {id} não encontrada...");
        }

        var categoriaExcluida = _uof.CategoriaRepository.Delete(categoria);
        await _uof.CommitAsync();

        var categoriaExcluidaDto = _mapper.Map<CategoriaDTO>(categoriaExcluida);

        InvalidateCacheAfterChange(id);

        return Ok(categoriaExcluidaDto);
    }

    private string GetCategoriaCacheKey(int id) => $"CacheCategoria_{id}";

    private void SetCache<T>(string key, T data)
    {
        var cacheOptions = new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30),
            SlidingExpiration = TimeSpan.FromSeconds(15),
            Priority = CacheItemPriority.High
        };
        _cache.Set(key, data, cacheOptions);
    }

    private void InvalidateCacheAfterChange(int id, CategoriaDTO? categoria = null)
    {
        _cache.Remove(CacheCategoriasKey);
        _cache.Remove(GetCategoriaCacheKey(id));

        if (categoria != null)
        {
            SetCache(GetCategoriaCacheKey(id), categoria);
        }
    }
}