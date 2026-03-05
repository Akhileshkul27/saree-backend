using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
public class CategoriesController : BaseApiController
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Get all categories</summary>
    [HttpGet]
    public async Task<IActionResult> GetAllCategories()
    {
        var result = await _categoryService.GetAllCategoriesAsync();
        return ApiResult(result);
    }

    /// <summary>Get category by ID</summary>
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetCategory(int id)
    {
        var result = await _categoryService.GetCategoryByIdAsync(id);
        return ApiResult(result);
    }

    /// <summary>Get category by slug</summary>
    [HttpGet("slug/{slug}")]
    public async Task<IActionResult> GetCategoryBySlug(string slug)
    {
        var result = await _categoryService.GetCategoryBySlugAsync(slug);
        return ApiResult(result);
    }
}
