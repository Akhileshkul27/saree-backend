using Microsoft.EntityFrameworkCore;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;
using SareeGrace.Domain.Entities;
using SareeGrace.Infrastructure.Data;
using System.Text.RegularExpressions;

namespace SareeGrace.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly AppDbContext _context;

    public ProductService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<PaginatedResult<ProductDto>>> GetProductsAsync(ProductFilterDto filter)
    {
        var query = _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .AsQueryable();

        // Public shop: active + non-deleted only.
        // Admin with IncludeInactive=true: show active AND soft-deleted (hard-deleted rows are already gone from DB).
        if (!filter.IncludeInactive)
            query = query.Where(p => p.IsActive && !p.IsDeleted);

        // Apply filters
        if (!string.IsNullOrEmpty(filter.Search))
            query = query.Where(p => p.Name.Contains(filter.Search) || p.Description!.Contains(filter.Search));

        if (filter.CategoryId.HasValue)
            query = query.Where(p => p.CategoryId == filter.CategoryId.Value);

        if (!string.IsNullOrEmpty(filter.FabricType))
            query = query.Where(p => p.FabricType == filter.FabricType);

        if (!string.IsNullOrEmpty(filter.Color))
            query = query.Where(p => p.Color == filter.Color);

        if (!string.IsNullOrEmpty(filter.Pattern))
            query = query.Where(p => p.Pattern == filter.Pattern);

        if (!string.IsNullOrEmpty(filter.Occasion))
            query = query.Where(p => p.Occasion == filter.Occasion);

        if (filter.MinPrice.HasValue)
            query = query.Where(p => p.BasePrice >= filter.MinPrice.Value);

        if (filter.MaxPrice.HasValue)
            query = query.Where(p => p.BasePrice <= filter.MaxPrice.Value);

        if (filter.IsSpecialOffer == true)
            query = query.Where(p => p.IsSpecialOffer);

        if (filter.IsNewArrival == true)
            query = query.Where(p => p.IsNewArrival);

        if (filter.InStock == true)
            query = query.Where(p => p.StockCount > 0);

        // Sort
        query = filter.SortBy?.ToLower() switch
        {
            "price_asc" => query.OrderBy(p => p.BasePrice),
            "price_desc" => query.OrderByDescending(p => p.BasePrice),
            "rating" => query.OrderByDescending(p => p.AverageRating),
            "popular" => query.OrderByDescending(p => p.ReviewCount),
            "discount" => query.OrderByDescending(p => p.DiscountPercent),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalItems = await query.CountAsync();
        var items = await query
            .Skip((filter.Page - 1) * filter.PageSize)
            .Take(filter.PageSize)
            .ToListAsync();

        var result = new PaginatedResult<ProductDto>
        {
            Items = items.Select(MapToDto).ToList(),
            Page = filter.Page,
            PageSize = filter.PageSize,
            TotalItems = totalItems
        };

        return ApiResponse<PaginatedResult<ProductDto>>.SuccessResponse(result);
    }

    public async Task<ApiResponse<ProductDetailDto>> GetProductByIdAsync(Guid id)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product == null)
            return ApiResponse<ProductDetailDto>.FailResponse("Product not found");

        return ApiResponse<ProductDetailDto>.SuccessResponse(MapToDetailDto(product));
    }

    public async Task<ApiResponse<ProductDetailDto>> GetProductBySlugAsync(string slug)
    {
        var product = await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(p => p.Slug == slug);

        if (product == null)
            return ApiResponse<ProductDetailDto>.FailResponse("Product not found");

        return ApiResponse<ProductDetailDto>.SuccessResponse(MapToDetailDto(product));
    }

    public async Task<ApiResponse<List<ProductDto>>> GetFeaturedProductsAsync(int count = 8)
    {
        var products = await _context.Products
            .Include(p => p.Category).Include(p => p.Images)
            .Where(p => p.IsActive && p.IsFeatured)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

        return ApiResponse<List<ProductDto>>.SuccessResponse(products.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<ProductDto>>> GetSpecialOffersAsync(int count = 12)
    {
        var products = await _context.Products
            .Include(p => p.Category).Include(p => p.Images)
            .Where(p => p.IsActive && p.IsSpecialOffer && p.StockCount > 0)
            .OrderByDescending(p => p.DiscountPercent)
            .Take(count)
            .ToListAsync();

        return ApiResponse<List<ProductDto>>.SuccessResponse(products.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<ProductDto>>> GetNewArrivalsAsync(int count = 12)
    {
        // Only return products the admin has explicitly marked as New Arrival
        var flagged = await _context.Products
            .Include(p => p.Category).Include(p => p.Images)
            .Where(p => p.IsActive && p.IsNewArrival)
            .OrderByDescending(p => p.CreatedAt)
            .Take(count)
            .ToListAsync();

        return ApiResponse<List<ProductDto>>.SuccessResponse(flagged.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<List<ProductDto>>> GetRelatedProductsAsync(Guid productId, int count = 6)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return ApiResponse<List<ProductDto>>.FailResponse("Product not found");

        var related = await _context.Products
            .Include(p => p.Category).Include(p => p.Images)
            .Where(p => p.IsActive && p.Id != productId &&
                   (p.CategoryId == product.CategoryId || p.FabricType == product.FabricType))
            .OrderByDescending(p => p.AverageRating)
            .Take(count)
            .ToListAsync();

        return ApiResponse<List<ProductDto>>.SuccessResponse(related.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<ProductDetailDto>> CreateProductAsync(CreateProductDto dto)
    {
        // ─ Name uniqueness check ─
        var nameExists = await _context.Products
            .AnyAsync(p => p.Name.ToLower() == dto.Name.Trim().ToLower());
        if (nameExists)
            return ApiResponse<ProductDetailDto>.FailResponse($"A product named '{dto.Name}' already exists");

        var product = new Product
        {
            Name = dto.Name.Trim(),
            Slug = GenerateSlug(dto.Name),
            Description = dto.Description,
            ShortDescription = dto.ShortDescription,
            SKU = GenerateSKU(dto.FabricType),
            BasePrice = dto.BasePrice,
            DiscountPercent = dto.DiscountPercent,
            FabricType = dto.FabricType,
            Color = dto.Color,
            Pattern = dto.Pattern,
            Occasion = dto.Occasion,
            Length = dto.Length,
            Width = dto.Width,
            HasBlousePiece = dto.HasBlousePiece,
            BlouseLength = dto.BlouseLength,
            WashCare = dto.WashCare,
            Weight = dto.Weight,
            StockCount = dto.StockCount,
            CategoryId = dto.CategoryId,
            IsFeatured = dto.IsFeatured,
            IsNewArrival = dto.IsNewArrival,
            ManufactureDate = dto.ManufactureDate
        };

        // Auto-apply special offer if > 2 years old
        ApplyAgingDiscount(product);

        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id);
    }

    public async Task<ApiResponse<ProductDetailDto>> UpdateProductAsync(Guid id, UpdateProductDto dto)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return ApiResponse<ProductDetailDto>.FailResponse("Product not found");

        product.Name = dto.Name.Trim();
        product.Slug = GenerateSlug(dto.Name);
        product.Description = dto.Description;
        product.ShortDescription = dto.ShortDescription;
        product.BasePrice = dto.BasePrice;
        product.DiscountPercent = dto.DiscountPercent;
        product.FabricType = dto.FabricType;
        product.Color = dto.Color;
        product.Pattern = dto.Pattern;
        product.Occasion = dto.Occasion;
        product.Length = dto.Length;
        product.Width = dto.Width;
        product.HasBlousePiece = dto.HasBlousePiece;
        product.BlouseLength = dto.BlouseLength;
        product.WashCare = dto.WashCare;
        product.Weight = dto.Weight;
        product.StockCount = dto.StockCount;
        product.CategoryId = dto.CategoryId;
        product.IsFeatured = dto.IsFeatured;
        product.IsNewArrival = dto.IsNewArrival;
        product.IsActive = dto.IsActive;
        product.IsDeleted = dto.IsDeleted;
        product.ManufactureDate = dto.ManufactureDate;
        product.UpdatedAt = DateTime.UtcNow;

        ApplyAgingDiscount(product);
        await _context.SaveChangesAsync();

        return await GetProductByIdAsync(product.Id);
    }

    public async Task<ApiResponse<bool>> CheckProductHasOrdersAsync(Guid id)
    {
        var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);
        return ApiResponse<bool>.SuccessResponse(hasOrders);
    }

    public async Task<ApiResponse<bool>> DeleteProductAsync(Guid id, bool permanent)
    {
        var product = await _context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (product == null)
            return ApiResponse<bool>.FailResponse("Product not found");

        var hasOrders = await _context.OrderItems.AnyAsync(oi => oi.ProductId == id);

        if (permanent)
        {
            if (hasOrders)
                return ApiResponse<bool>.FailResponse(
                    "Cannot permanently delete a product that has order history. Use deactivate instead.");

            // Hard delete — remove all dependent rows first
            _context.WishlistItems.RemoveRange(_context.WishlistItems.Where(w => w.ProductId == id));
            _context.CartItems.RemoveRange(_context.CartItems.Where(c => c.ProductId == id));
            _context.Reviews.RemoveRange(_context.Reviews.Where(r => r.ProductId == id));
            _context.ProductImages.RemoveRange(product.Images);
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Product permanently deleted");
        }
        else
        {
            // Soft delete
            product.IsActive  = false;
            product.IsDeleted = true;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return ApiResponse<bool>.SuccessResponse(true, "Product deactivated and hidden from shop");
        }
    }

    public async Task<ApiResponse<bool>> UpdateStockAsync(Guid id, int quantity)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
            return ApiResponse<bool>.FailResponse("Product not found");

        product.StockCount = quantity;
        product.LastStockUpdate = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Stock updated");
    }

    public async Task<ApiResponse<ProductImageDto>> AddProductImageAsync(Guid productId, string imageUrl, bool isPrimary = false)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null)
            return ApiResponse<ProductImageDto>.FailResponse("Product not found");

        if (isPrimary)
        {
            foreach (var img in product.Images)
                img.IsPrimary = false;
        }

        var image = new ProductImage
        {
            ProductId = productId,
            ImageUrl = imageUrl,
            IsPrimary = isPrimary || !product.Images.Any(),
            DisplayOrder = product.Images.Count
        };

        await _context.ProductImages.AddAsync(image);
        await _context.SaveChangesAsync();

        return ApiResponse<ProductImageDto>.SuccessResponse(new ProductImageDto
        {
            Id = image.Id,
            ImageUrl = image.ImageUrl,
            IsPrimary = image.IsPrimary,
            DisplayOrder = image.DisplayOrder
        });
    }

    public async Task<ApiResponse<bool>> DeleteProductImageAsync(int imageId)
    {
        var image = await _context.ProductImages.FindAsync(imageId);
        if (image == null)
            return ApiResponse<bool>.FailResponse("Image not found");

        _context.ProductImages.Remove(image);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Image deleted");
    }

    private static void ApplyAgingDiscount(Product product)
    {
        var ageYears = (DateTime.UtcNow - product.ManufactureDate).TotalDays / 365.25;
        if (ageYears >= 2 && product.StockCount > 0)
        {
            if (product.DiscountPercent < 10)
                product.DiscountPercent = 10;
            product.IsSpecialOffer = true;
        }
    }

    private static string GenerateSlug(string name)
    {
        var slug = name.ToLower().Trim();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = Regex.Replace(slug, @"-+", "-");
        return slug + "-" + Guid.NewGuid().ToString()[..6];
    }

    private static string GenerateSKU(string fabricType)
    {
        var prefix = fabricType.Length >= 3 ? fabricType[..3].ToUpper() : fabricType.ToUpper();
        return $"SG-{prefix}-{DateTime.UtcNow:yyMMdd}-{new Random().Next(1000, 9999)}";
    }

    private static ProductDto MapToDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        ShortDescription = p.ShortDescription,
        BasePrice = p.BasePrice,
        DiscountPercent = p.DiscountPercent,
        SellingPrice = p.SellingPrice,
        FabricType = p.FabricType,
        Color = p.Color,
        Pattern = p.Pattern,
        Occasion = p.Occasion,
        StockCount = p.StockCount,
        IsActive = p.IsActive,
        IsDeleted = p.IsDeleted,
        IsFeatured = p.IsFeatured,
        IsNewArrival = p.IsNewArrival,
        IsSpecialOffer = p.IsSpecialOffer,
        AverageRating = p.AverageRating,
        ReviewCount = p.ReviewCount,
        PrimaryImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
        CategoryName = p.Category?.Name ?? ""
    };

    private static ProductDetailDto MapToDetailDto(Product p) => new()
    {
        Id = p.Id,
        Name = p.Name,
        Slug = p.Slug,
        Description = p.Description,
        ShortDescription = p.ShortDescription,
        SKU = p.SKU,
        BasePrice = p.BasePrice,
        DiscountPercent = p.DiscountPercent,
        SellingPrice = p.SellingPrice,
        FabricType = p.FabricType,
        Color = p.Color,
        Pattern = p.Pattern,
        Occasion = p.Occasion,
        Length = p.Length,
        Width = p.Width,
        HasBlousePiece = p.HasBlousePiece,
        BlouseLength = p.BlouseLength,
        WashCare = p.WashCare,
        Weight = p.Weight,
        StockCount = p.StockCount,
        CategoryId = p.CategoryId,
        IsActive = p.IsActive,
        IsDeleted = p.IsDeleted,
        IsFeatured = p.IsFeatured,
        IsNewArrival = p.IsNewArrival,
        IsSpecialOffer = p.IsSpecialOffer,
        AverageRating = p.AverageRating,
        ReviewCount = p.ReviewCount,
        ManufactureDate = p.ManufactureDate,
        CategoryName = p.Category?.Name ?? "",
        PrimaryImageUrl = p.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl ?? p.Images.FirstOrDefault()?.ImageUrl,
        Images = p.Images.Select(i => new ProductImageDto
        {
            Id = i.Id,
            ImageUrl = i.ImageUrl,
            ThumbnailUrl = i.ThumbnailUrl,
            AltText = i.AltText,
            IsPrimary = i.IsPrimary,
            DisplayOrder = i.DisplayOrder
        }).ToList()
    };

    public async Task<ApiResponse<ProductFilterOptionsDto>> GetFilterOptionsAsync()
    {        var activeProducts = _context.Products.Where(p => p.IsActive);

        var fabricTypes = await activeProducts
            .Where(p => p.FabricType != null && p.FabricType != "")
            .Select(p => p.FabricType)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var colors = await activeProducts
            .Where(p => p.Color != null && p.Color != "")
            .Select(p => p.Color)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var occasions = await activeProducts
            .Where(p => p.Occasion != null && p.Occasion != "")
            .Select(p => p.Occasion!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        var patterns = await activeProducts
            .Where(p => p.Pattern != null && p.Pattern != "")
            .Select(p => p.Pattern!)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync();

        return ApiResponse<ProductFilterOptionsDto>.SuccessResponse(new ProductFilterOptionsDto
        {
            FabricTypes = fabricTypes,
            Colors = colors,
            Occasions = occasions,
            Patterns = patterns,
        });
    }

    public async Task<ApiResponse<BulkCreateResultDto>> BulkCreateProductsAsync(List<BulkProductRowDto> rows)
    {
        var result = new BulkCreateResultDto();
        // Load existing categories into a live lookup so newly created ones are reused
        var allCategories = await _context.Categories.ToListAsync();

        foreach (var row in rows)
        {
            // ─ Required field validation ─
            if (string.IsNullOrWhiteSpace(row.Name))
            {
                result.Errors.Add(new BulkRowErrorDto { Row = row.RowNumber, ProductName = row.Name, Error = "Name is required" });
                continue;
            }
            if (row.BasePrice <= 0)
            {
                result.Errors.Add(new BulkRowErrorDto { Row = row.RowNumber, ProductName = row.Name, Error = "BasePrice must be greater than 0" });
                continue;
            }
            if (row.StockCount < 0)
            {
                result.Errors.Add(new BulkRowErrorDto { Row = row.RowNumber, ProductName = row.Name, Error = "StockCount cannot be negative" });
                continue;
            }

            // ─ Category lookup — auto-create if not found ─
            var categoryName = row.CategoryName?.Trim() ?? string.Empty;
            var category = allCategories.FirstOrDefault(c =>
                string.Equals(c.Name, categoryName, StringComparison.OrdinalIgnoreCase));

            if (category == null)
            {
                if (string.IsNullOrWhiteSpace(categoryName))
                {
                    result.Errors.Add(new BulkRowErrorDto { Row = row.RowNumber, ProductName = row.Name, Error = "CategoryName is required" });
                    continue;
                }

                // Slugify: lowercase, replace spaces/specials with hyphens, collapse multiple hyphens
                var slug = System.Text.RegularExpressions.Regex
                    .Replace(categoryName.ToLowerInvariant(), @"[^a-z0-9]+", "-")
                    .Trim('-');

                // Ensure slug uniqueness
                var baseSlug = slug;
                var suffix = 1;
                while (allCategories.Any(c => c.Slug == slug))
                    slug = $"{baseSlug}-{suffix++}";

                category = new SareeGrace.Domain.Entities.Category
                {
                    Name = categoryName,
                    Slug = slug,
                    IsActive = true,
                    DisplayOrder = 0
                };
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();   // get the new Id immediately
                allCategories.Add(category);         // cache so subsequent rows reuse it
            }

            // ─ Parse manufacture date ─
            var manufactureDate = DateTime.UtcNow;
            if (!string.IsNullOrWhiteSpace(row.ManufactureDate) &&
                DateTime.TryParse(row.ManufactureDate, out var parsedDate))
                manufactureDate = parsedDate;

            // ─ Build shared product fields ─
            var basePrice    = row.BasePrice;
            var discountPct  = row.DiscountPercent;
            var fabricType   = row.FabricType ?? string.Empty;
            var color        = row.Color ?? string.Empty;
            var stockCount   = row.StockCount;
            var length       = row.Length > 0 ? row.Length : 5.5m;
            var width        = row.Width  > 0 ? row.Width  : 1.1m;

            // ─ Upsert: update if product with same name already exists (including soft-deleted) ─
            var existing = await _context.Products
                .FirstOrDefaultAsync(p => p.Name.ToLower() == row.Name.Trim().ToLower());

            if (existing != null)
            {
                var updateDto = new UpdateProductDto
                {
                    Name             = row.Name.Trim(),
                    ShortDescription = row.ShortDescription,
                    Description      = row.Description,
                    BasePrice        = basePrice,
                    DiscountPercent  = discountPct,
                    FabricType       = fabricType,
                    Color            = color,
                    Pattern          = row.Pattern,
                    Occasion         = row.Occasion,
                    Length           = length,
                    Width            = width,
                    HasBlousePiece   = row.HasBlousePiece,
                    BlouseLength     = row.BlouseLength,
                    WashCare         = row.WashCare,
                    Weight           = row.Weight,
                    StockCount       = stockCount,
                    CategoryId       = category.Id,
                    IsFeatured       = row.IsFeatured,
                    IsNewArrival     = row.IsNewArrival,
                    ManufactureDate  = manufactureDate,
                    IsActive         = true,        // always re-activate on Excel upsert
                    IsDeleted        = false,       // resurrect soft-deleted products
                };
                try
                {
                    var updateResult = await UpdateProductAsync(existing.Id, updateDto);
                    if (updateResult.Success)
                    {
                        if (row.Images.Count > 0)
                            result.ImagesAttached += await ApplyBulkImagesAsync(existing.Id, row.Images, replace: true);
                        result.UpdatedCount++;
                    }
                    else
                        result.Errors.Add(new BulkRowErrorDto { Row = row.RowNumber, ProductName = row.Name, Error = updateResult.Message });
                }
                catch (Exception ex)
                {
                    result.Errors.Add(new BulkRowErrorDto { Row = row.RowNumber, ProductName = row.Name, Error = ex.Message });
                }
                continue;
            }

            // ─ Insert new product ─
            var dto = new CreateProductDto
            {
                Name             = row.Name.Trim(),
                ShortDescription = row.ShortDescription,
                Description      = row.Description,
                BasePrice        = basePrice,
                DiscountPercent  = discountPct,
                FabricType       = fabricType,
                Color            = color,
                Pattern          = row.Pattern,
                Occasion         = row.Occasion,
                Length           = length,
                Width            = width,
                HasBlousePiece   = row.HasBlousePiece,
                BlouseLength     = row.BlouseLength,
                WashCare         = row.WashCare,
                Weight           = row.Weight,
                StockCount       = stockCount,
                CategoryId       = category.Id,
                IsFeatured       = row.IsFeatured,
                ManufactureDate  = manufactureDate,
            };

            try
            {
                // Bypass the name-uniqueness guard inside CreateProductAsync because
                // we already confirmed above that no product with this name exists.
                var product = new Product
                {
                    Name             = dto.Name,
                    Slug             = GenerateSlug(dto.Name),
                    Description      = dto.Description,
                    ShortDescription = dto.ShortDescription,
                    SKU              = GenerateSKU(dto.FabricType),
                    BasePrice        = dto.BasePrice,
                    DiscountPercent  = dto.DiscountPercent,
                    FabricType       = dto.FabricType,
                    Color            = dto.Color,
                    Pattern          = dto.Pattern,
                    Occasion         = dto.Occasion,
                    Length           = dto.Length,
                    Width            = dto.Width,
                    HasBlousePiece   = dto.HasBlousePiece,
                    BlouseLength     = dto.BlouseLength,
                    WashCare         = dto.WashCare,
                    Weight           = dto.Weight,
                    StockCount       = dto.StockCount,
                    CategoryId       = dto.CategoryId,
                    IsFeatured       = dto.IsFeatured,
                    IsNewArrival     = dto.IsNewArrival,
                    ManufactureDate  = dto.ManufactureDate,
                };
                ApplyAgingDiscount(product);
                await _context.Products.AddAsync(product);
                await _context.SaveChangesAsync();
                if (row.Images.Count > 0)
                    result.ImagesAttached += await ApplyBulkImagesAsync(product.Id, row.Images, replace: false);
                result.InsertedCount++;
            }
            catch (Exception ex)
            {
                result.Errors.Add(new BulkRowErrorDto { Row = row.RowNumber, ProductName = row.Name, Error = ex.Message });
            }
        }

        result.ErrorCount = result.Errors.Count;
        var parts = new List<string>();
        if (result.InsertedCount  > 0) parts.Add($"{result.InsertedCount} product(s) imported");
        if (result.UpdatedCount   > 0) parts.Add($"{result.UpdatedCount} product(s) updated");
        if (result.ImagesAttached > 0) parts.Add($"{result.ImagesAttached} image(s) attached");
        if (result.ErrorCount     > 0) parts.Add($"{result.ErrorCount} row(s) had errors");
        var message = parts.Any() ? string.Join(", ", parts) : "Nothing to import";
        return ApiResponse<BulkCreateResultDto>.SuccessResponse(result, message);
    }

    /// <summary>
    /// Applies up to 10 images from a bulk-import row to a product.
    /// <para>
    ///   <b>replace = true</b>: clears all existing product images first (used on upsert).  <br/>
    ///   <b>replace = false</b>: appends only URLs not already attached (used on insert).
    /// </para>
    /// Image at index 0 becomes the primary image; the rest are secondary.<br/>
    /// Empty values and duplicate URLs (case-insensitive) are silently skipped.
    /// </summary>
    /// <returns>Number of image rows actually inserted.</returns>
    private async Task<int> ApplyBulkImagesAsync(Guid productId, List<string> rawImages, bool replace)
    {
        const int MaxImages = 10;

        // Sanitize: trim, remove blanks, deduplicate (case-insensitive), cap at 10
        var distinctUrls = rawImages
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(MaxImages)
            .ToList();

        if (replace)
        {
            var existing = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .ToListAsync();

            if (existing.Count > 0)
                _context.ProductImages.RemoveRange(existing);

            if (distinctUrls.Count == 0)
            {
                if (existing.Count > 0) await _context.SaveChangesAsync();
                return 0;
            }
        }
        else
        {
            if (distinctUrls.Count == 0) return 0;

            // Append mode: skip URLs already attached to this product
            var existingUrls = await _context.ProductImages
                .Where(pi => pi.ProductId == productId)
                .Select(pi => pi.ImageUrl.ToLower())
                .ToListAsync();

            distinctUrls = distinctUrls
                .Where(u => !existingUrls.Contains(u.ToLower()))
                .ToList();

            if (distinctUrls.Count == 0) return 0;
        }

        // Build entities — index 0 is primary, each subsequent image is secondary
        var images = distinctUrls
            .Select((url, idx) => new ProductImage
            {
                ProductId    = productId,
                ImageUrl     = url,
                IsPrimary    = idx == 0,
                DisplayOrder = idx + 1,
            })
            .ToList();

        await _context.ProductImages.AddRangeAsync(images);
        await _context.SaveChangesAsync();
        return images.Count;
    }
}
