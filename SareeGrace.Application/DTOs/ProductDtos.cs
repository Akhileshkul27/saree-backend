namespace SareeGrace.Application.DTOs;

public class ProductDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal SellingPrice { get; set; }
    public string FabricType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Pattern { get; set; }
    public string? Occasion { get; set; }
    public int StockCount { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; }
    public bool IsSpecialOffer { get; set; }
    public decimal AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public string? PrimaryImageUrl { get; set; }
    public string CategoryName { get; set; } = string.Empty;
}

public class ProductDetailDto : ProductDto
{
    public string? Description { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal Length { get; set; }
    public decimal Width { get; set; }
    public bool HasBlousePiece { get; set; }
    public decimal? BlouseLength { get; set; }
    public string? WashCare { get; set; }
    public decimal? Weight { get; set; }
    public DateTime ManufactureDate { get; set; }
    public int CategoryId { get; set; }
    public List<ProductImageDto> Images { get; set; } = new();
}

public class ProductImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string? ThumbnailUrl { get; set; }
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateProductDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public string FabricType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Pattern { get; set; }
    public string? Occasion { get; set; }
    public decimal Length { get; set; } = 5.5m;
    public decimal Width { get; set; } = 1.1m;
    public bool HasBlousePiece { get; set; }
    public decimal? BlouseLength { get; set; }
    public string? WashCare { get; set; }
    public decimal? Weight { get; set; }
    public int StockCount { get; set; }
    public int CategoryId { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsNewArrival { get; set; } = false;
    public DateTime ManufactureDate { get; set; }
}

public class UpdateProductDto : CreateProductDto
{
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
}

// ── Bulk Import ──
public class BulkProductRowDto
{
    public int RowNumber { get; set; }
    public string Name { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public string? ShortDescription { get; set; }
    public string? Description { get; set; }
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public string? FabricType { get; set; }
    public string? Color { get; set; }
    public string? Pattern { get; set; }
    public string? Occasion { get; set; }
    public decimal Length { get; set; } = 5.5m;
    public decimal Width { get; set; } = 1.1m;
    public bool HasBlousePiece { get; set; } = false;
    public decimal? BlouseLength { get; set; }
    public string? WashCare { get; set; }
    public decimal? Weight { get; set; }
    public int StockCount { get; set; }
    public bool IsFeatured { get; set; } = false;
    public bool IsNewArrival { get; set; } = false;
    public string? ManufactureDate { get; set; }
    /// <summary>
    /// Up to 10 image URLs for this product. Index 0 = primary image.
    /// Populated by the Excel import parser; empty list means no images.
    /// </summary>
    public List<string> Images { get; set; } = new();
}

public class BulkRowErrorDto
{
    public int Row { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Error { get; set; } = string.Empty;
}

public class BulkCreateResultDto
{
    public int InsertedCount { get; set; }
    public int UpdatedCount { get; set; }
    public int ImagesAttached { get; set; }
    public int ErrorCount { get; set; }
    public List<BulkRowErrorDto> Errors { get; set; } = new();
}

public class ProductFilterDto
{
    public string? Search { get; set; }
    public int? CategoryId { get; set; }
    public string? FabricType { get; set; }
    public string? Color { get; set; }
    public string? Pattern { get; set; }
    public string? Occasion { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public bool? IsSpecialOffer { get; set; }
    public bool? IsNewArrival { get; set; }
    public bool? InStock { get; set; }
    public string SortBy { get; set; } = "newest";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    /// <summary>When true, includes inactive/soft-deleted products (admin use only)</summary>
    public bool IncludeInactive { get; set; } = false;
}

public class ProductFilterOptionsDto
{
    public List<string> FabricTypes { get; set; } = new();
    public List<string> Colors { get; set; } = new();
    public List<string> Occasions { get; set; } = new();
    public List<string> Patterns { get; set; } = new();
}
