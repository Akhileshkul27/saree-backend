using Microsoft.EntityFrameworkCore;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;
using SareeGrace.Domain.Entities;
using SareeGrace.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace SareeGrace.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly AppDbContext _context;

    public CategoryService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<CategoryDto>>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .Include(c => c.SubCategories)
            .Include(c => c.Products)
            .Where(c => c.IsActive && c.ParentCategoryId == null)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync();

        return ApiResponse<List<CategoryDto>>.SuccessResponse(categories.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryByIdAsync(int id)
    {
        var cat = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Id == id);
        return cat == null
            ? ApiResponse<CategoryDto>.FailResponse("Category not found")
            : ApiResponse<CategoryDto>.SuccessResponse(MapToDto(cat));
    }

    public async Task<ApiResponse<CategoryDto>> GetCategoryBySlugAsync(string slug)
    {
        var cat = await _context.Categories.Include(c => c.Products).FirstOrDefaultAsync(c => c.Slug == slug);
        return cat == null
            ? ApiResponse<CategoryDto>.FailResponse("Category not found")
            : ApiResponse<CategoryDto>.SuccessResponse(MapToDto(cat));
    }

    public async Task<ApiResponse<CategoryDto>> CreateCategoryAsync(CreateCategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name.Trim(),
            Slug = GenerateSlug(dto.Name),
            Description = dto.Description,
            ParentCategoryId = dto.ParentCategoryId,
            DisplayOrder = dto.DisplayOrder
        };

        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return ApiResponse<CategoryDto>.SuccessResponse(MapToDto(category), "Category created");
    }

    public async Task<ApiResponse<CategoryDto>> UpdateCategoryAsync(int id, UpdateCategoryDto dto)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat == null) return ApiResponse<CategoryDto>.FailResponse("Category not found");

        cat.Name = dto.Name.Trim();
        cat.Slug = GenerateSlug(dto.Name);
        cat.Description = dto.Description;
        cat.ParentCategoryId = dto.ParentCategoryId;
        cat.DisplayOrder = dto.DisplayOrder;
        cat.IsActive = dto.IsActive;
        if (dto.ImageUrl != null) cat.ImageUrl = dto.ImageUrl;
        await _context.SaveChangesAsync();

        return ApiResponse<CategoryDto>.SuccessResponse(MapToDto(cat), "Category updated");
    }

    public async Task<ApiResponse<bool>> DeleteCategoryAsync(int id)
    {
        var cat = await _context.Categories.FindAsync(id);
        if (cat == null) return ApiResponse<bool>.FailResponse("Category not found");

        cat.IsActive = false;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Category deactivated");
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        return slug;
    }

    private static CategoryDto MapToDto(Category c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Slug = c.Slug,
        Description = c.Description,
        ImageUrl = c.ImageUrl,
        ParentCategoryId = c.ParentCategoryId,
        DisplayOrder = c.DisplayOrder,
        IsActive = c.IsActive,
        ProductCount = c.Products?.Count(p => p.IsActive) ?? 0,
        SubCategories = c.SubCategories?.Where(s => s.IsActive).Select(MapToDto).ToList() ?? new()
    };
}
