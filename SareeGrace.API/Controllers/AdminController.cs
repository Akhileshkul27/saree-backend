using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

/// <summary>
/// Admin-only controller for managing the store.
/// Admin can manage products, orders, reviews, categories, and images — all through this UI-driven API.
/// No code changes needed for day-to-day operations.
/// </summary>
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminController : BaseApiController
{
    private readonly IProductService _productService;
    private readonly IOrderService _orderService;
    private readonly IReviewService _reviewService;
    private readonly ICategoryService _categoryService;
    private readonly IImageService _imageService;

    public AdminController(
        IProductService productService,
        IOrderService orderService,
        IReviewService reviewService,
        ICategoryService categoryService,
        IImageService imageService)
    {
        _productService = productService;
        _orderService = orderService;
        _reviewService = reviewService;
        _categoryService = categoryService;
        _imageService = imageService;
    }

    // ──────────────────── Dashboard ────────────────────

    /// <summary>Get admin dashboard stats</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _orderService.GetDashboardAsync();
        return ApiResult(result);
    }

    // ──────────────────── Products ────────────────────

    /// <summary>Get all products (admin view with pagination)</summary>
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var result = await _productService.GetProductsAsync(filter);
        return ApiResult(result);
    }

    /// <summary>Create a new product</summary>
    [HttpPost("products")]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        var result = await _productService.CreateProductAsync(dto);
        return ApiResult(result);
    }

    /// <summary>Update an existing product</summary>
    [HttpPut("products/{id:guid}")]
    public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var result = await _productService.UpdateProductAsync(id, dto);
        return ApiResult(result);
    }

    /// <summary>Check if a product has order history</summary>
    [HttpGet("products/{id:guid}/has-orders")]
    public async Task<IActionResult> CheckProductHasOrders(Guid id)
    {
        var result = await _productService.CheckProductHasOrdersAsync(id);
        return ApiResult(result);
    }

    /// <summary>Delete a product — permanent=true for hard delete, false for soft delete</summary>
    [HttpDelete("products/{id:guid}")]
    public async Task<IActionResult> DeleteProduct(Guid id, [FromQuery] bool permanent = false)
    {
        var result = await _productService.DeleteProductAsync(id, permanent);
        return ApiResult(result);
    }

    /// <summary>Bulk import products from parsed Excel rows</summary>
    [HttpPost("products/bulk")]
    public async Task<IActionResult> BulkCreateProducts([FromBody] List<BulkProductRowDto> rows)
    {
        if (rows == null || rows.Count == 0)
            return BadRequest(ApiResponse<string>.FailResponse("No rows provided"));
        if (rows.Count > 500)
            return BadRequest(ApiResponse<string>.FailResponse("Maximum 500 rows per import"));
        var result = await _productService.BulkCreateProductsAsync(rows);
        return ApiResult(result);
    }

    /// <summary>Update product stock</summary>
    [HttpPatch("products/{id:guid}/stock")]
    public async Task<IActionResult> UpdateStock(Guid id, [FromBody] int quantity)
    {
        var result = await _productService.UpdateStockAsync(id, quantity);
        return ApiResult(result);
    }

    // ──────────────────── Product Images (Admin UI-driven) ────────────────────

    /// <summary>Upload product image via admin panel — admin only changes images through UI</summary>
    [HttpPost("products/{productId:guid}/images")]
    public async Task<IActionResult> UploadProductImage(Guid productId, IFormFile file, [FromQuery] bool isPrimary = false)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.FailResponse("No file uploaded"));

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest(ApiResponse<string>.FailResponse("Invalid image format. Allowed: jpg, jpeg, png, webp"));

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<string>.FailResponse("Image size must be under 5MB"));

        using var stream = file.OpenReadStream();
        var imageUrl = await _imageService.SaveImageAsync(stream, file.FileName, "products");
        var result = await _productService.AddProductImageAsync(productId, imageUrl, isPrimary);
        return ApiResult(result);
    }

    /// <summary>Delete a product image</summary>
    [HttpDelete("products/images/{imageId:int}")]
    public async Task<IActionResult> DeleteProductImage(int imageId)
    {
        var result = await _productService.DeleteProductImageAsync(imageId);
        return ApiResult(result);
    }

    /// <summary>Upload category image via admin panel</summary>
    [HttpPost("categories/{categoryId:int}/image")]
    public async Task<IActionResult> UploadCategoryImage(int categoryId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<string>.FailResponse("No file uploaded"));

        using var stream = file.OpenReadStream();
        var imageUrl = await _imageService.SaveImageAsync(stream, file.FileName, "categories");

        // Update category with new image URL
        var category = await _categoryService.GetCategoryByIdAsync(categoryId);
        if (!category.Success || category.Data == null)
            return BadRequest(ApiResponse<string>.FailResponse("Category not found"));

        var updateDto = new UpdateCategoryDto
        {
            Name = category.Data.Name,
            Description = category.Data.Description,
            ParentCategoryId = category.Data.ParentCategoryId,
            DisplayOrder = category.Data.DisplayOrder,
            IsActive = category.Data.IsActive,
            ImageUrl = imageUrl
        };
        var result = await _categoryService.UpdateCategoryAsync(categoryId, updateDto);
        return Ok(ApiResponse<string>.SuccessResponse(imageUrl, "Image uploaded"));
    }

    // ──────────────────── Orders ────────────────────

    /// <summary>Get all orders (admin view)</summary>
    [HttpGet("orders")]
    public async Task<IActionResult> GetAllOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        var result = await _orderService.GetAllOrdersAsync(page, pageSize, status);
        return ApiResult(result);
    }

    /// <summary>Update order status (Processing, Shipped, Delivered, etc.)</summary>
    [HttpPut("orders/{orderId:guid}/status")]
    public async Task<IActionResult> UpdateOrderStatus(Guid orderId, [FromBody] UpdateOrderStatusDto dto)
    {
        var result = await _orderService.UpdateOrderStatusAsync(orderId, dto);
        return ApiResult(result);
    }

    // ──────────────────── Categories ────────────────────

    /// <summary>Create a new category</summary>
    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        var result = await _categoryService.CreateCategoryAsync(dto);
        return ApiResult(result);
    }

    /// <summary>Update a category</summary>
    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto dto)
    {
        var result = await _categoryService.UpdateCategoryAsync(id, dto);
        return ApiResult(result);
    }

    /// <summary>Delete a category</summary>
    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var result = await _categoryService.DeleteCategoryAsync(id);
        return ApiResult(result);
    }

    // ──────────────────── Reviews ────────────────────

    /// <summary>Approve a pending review</summary>
    [HttpPut("reviews/{reviewId:int}/approve")]
    public async Task<IActionResult> ApproveReview(int reviewId)
    {
        var result = await _reviewService.ApproveReviewAsync(reviewId);
        return ApiResult(result);
    }

    /// <summary>Delete a review</summary>
    [HttpDelete("reviews/{reviewId:int}")]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        var result = await _reviewService.DeleteReviewAsync(reviewId);
        return ApiResult(result);
    }
}
