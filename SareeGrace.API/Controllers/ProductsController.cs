using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
public class ProductsController : BaseApiController
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    /// <summary>Get paginated products with filters</summary>
    [HttpGet]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetProductsAsync(filter);
        return ApiResult(result);
    }

    /// <summary>Get product by ID</summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProduct(Guid id)
    {
        var result = await _productService.GetProductByIdAsync(id);
        return ApiResult(result);
    }

    /// <summary>Get product by slug (SEO-friendly URL)</summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetProductBySlug(string slug)
    {
        var result = await _productService.GetProductBySlugAsync(slug);
        return ApiResult(result);
    }

    /// <summary>Get featured products for homepage</summary>
    [HttpGet("featured")]
    public async Task<IActionResult> GetFeaturedProducts([FromQuery] int count = 8)
    {
        var result = await _productService.GetFeaturedProductsAsync(count);
        return ApiResult(result);
    }

    /// <summary>Get special offer products</summary>
    [HttpGet("special-offers")]
    public async Task<IActionResult> GetSpecialOffers([FromQuery] int count = 12)
    {
        var result = await _productService.GetSpecialOffersAsync(count);
        return ApiResult(result);
    }

    /// <summary>Get new arrivals</summary>
    [HttpGet("new-arrivals")]
    public async Task<IActionResult> GetNewArrivals([FromQuery] int count = 12)
    {
        var result = await _productService.GetNewArrivalsAsync(count);
        return ApiResult(result);
    }

    /// <summary>Get related products</summary>
    [HttpGet("{id:guid}/related")]
    public async Task<IActionResult> GetRelatedProducts(Guid id, [FromQuery] int count = 6)
    {
        var result = await _productService.GetRelatedProductsAsync(id, count);
        return ApiResult(result);
    }

    /// <summary>Get distinct fabric types, colors, occasions and patterns from active products</summary>
    [HttpGet("filter-options")]
    public async Task<IActionResult> GetFilterOptions()
    {
        var result = await _productService.GetFilterOptionsAsync();
        return ApiResult(result);
    }
}
