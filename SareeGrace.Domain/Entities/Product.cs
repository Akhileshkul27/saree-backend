namespace SareeGrace.Domain.Entities;

public class Product
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ShortDescription { get; set; }
    public string SKU { get; set; } = string.Empty;
    public decimal BasePrice { get; set; }
    public decimal DiscountPercent { get; set; } = 0;
    public string FabricType { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string? Pattern { get; set; }
    public string? Occasion { get; set; }
    public decimal Length { get; set; } = 5.5m;
    public decimal Width { get; set; } = 1.1m;
    public bool HasBlousePiece { get; set; } = false;
    public decimal? BlouseLength { get; set; }
    public string? WashCare { get; set; }
    public decimal? Weight { get; set; }
    public int StockCount { get; set; } = 0;
    public int CategoryId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public bool IsFeatured { get; set; } = false;
    public bool IsNewArrival { get; set; } = false;
    public bool IsSpecialOffer { get; set; } = false;
    public decimal AverageRating { get; set; } = 0;
    public int ReviewCount { get; set; } = 0;
    public DateTime ManufactureDate { get; set; }
    public DateTime? LastStockUpdate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Computed
    public decimal SellingPrice => BasePrice - (BasePrice * DiscountPercent / 100);

    // Navigation
    public Category Category { get; set; } = null!;
    public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
