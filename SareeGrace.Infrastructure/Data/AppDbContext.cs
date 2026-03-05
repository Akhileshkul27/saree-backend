using Microsoft.EntityFrameworkCore;
using SareeGrace.Domain.Entities;

namespace SareeGrace.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<WishlistItem> WishlistItems => Set<WishlistItem>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Coupon> Coupons => Set<Coupon>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ── User ──
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(256).IsRequired();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.Role).HasMaxLength(20).HasDefaultValue("Customer");
        });

        // ── Category ──
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.Property(e => e.Name).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.HasOne(e => e.ParentCategory)
                  .WithMany(e => e.SubCategories)
                  .HasForeignKey(e => e.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Product ──
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Slug).IsUnique();
            entity.HasIndex(e => e.SKU).IsUnique();
            entity.HasIndex(e => e.CategoryId);
            entity.HasIndex(e => e.FabricType);
            entity.HasIndex(e => e.Color);
            entity.HasIndex(e => e.BasePrice);
            entity.HasIndex(e => e.ManufactureDate);
            entity.HasIndex(e => new { e.IsActive, e.IsFeatured });
            entity.Property(e => e.Name).HasMaxLength(300).IsRequired();
            entity.Property(e => e.Slug).HasMaxLength(300).IsRequired();
            entity.Property(e => e.SKU).HasMaxLength(50).IsRequired();
            entity.Property(e => e.BasePrice).HasPrecision(10, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.Length).HasPrecision(5, 2);
            entity.Property(e => e.Width).HasPrecision(5, 2);
            entity.Property(e => e.BlouseLength).HasPrecision(5, 2);
            entity.Property(e => e.Weight).HasPrecision(7, 2);
            entity.Property(e => e.AverageRating).HasPrecision(3, 2);
            entity.Property(e => e.FabricType).HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(50);
            entity.Property(e => e.Pattern).HasMaxLength(50);
            entity.Property(e => e.Occasion).HasMaxLength(50);
            entity.Ignore(e => e.SellingPrice); // Computed in app, not in DB
            entity.HasOne(e => e.Category)
                  .WithMany(e => e.Products)
                  .HasForeignKey(e => e.CategoryId);
        });

        // ── ProductImage ──
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ImageUrl).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.Images)
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // ── Address ──
        modelBuilder.Entity<Address>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FullName).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Phone).HasMaxLength(15).IsRequired();
            entity.Property(e => e.AddressLine1).HasMaxLength(200).IsRequired();
            entity.Property(e => e.City).HasMaxLength(100).IsRequired();
            entity.Property(e => e.State).HasMaxLength(100).IsRequired();
            entity.Property(e => e.Pincode).HasMaxLength(10).IsRequired();
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Addresses)
                  .HasForeignKey(e => e.UserId);
        });

        // ── Order ──
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderNumber).IsUnique();
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.OrderStatus);
            entity.Property(e => e.OrderNumber).HasMaxLength(20).IsRequired();
            entity.Property(e => e.SubTotal).HasPrecision(10, 2);
            entity.Property(e => e.DiscountAmount).HasPrecision(10, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
            entity.Property(e => e.ShippingCharge).HasPrecision(10, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
            entity.Property(e => e.OrderStatus).HasMaxLength(30);
            entity.Property(e => e.PaymentStatus).HasMaxLength(30);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Orders)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(e => e.ShippingAddress)
                  .WithMany()
                  .HasForeignKey(e => e.ShippingAddressId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── OrderItem ──
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.OrderId);
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.Property(e => e.DiscountPercent).HasPrecision(5, 2);
            entity.Property(e => e.TotalPrice).HasPrecision(10, 2);
            entity.Property(e => e.ProductName).HasMaxLength(300);
            entity.HasOne(e => e.Order)
                  .WithMany(e => e.Items)
                  .HasForeignKey(e => e.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── CartItem ──
        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(e => e.CartItems)
                  .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId);
        });

        // ── WishlistItem ──
        modelBuilder.Entity<WishlistItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.UserId, e.ProductId }).IsUnique();
            entity.HasOne(e => e.User)
                  .WithMany(e => e.WishlistItems)
                  .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Product)
                  .WithMany()
                  .HasForeignKey(e => e.ProductId);
        });

        // ── Review ──
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.ProductId);
            entity.HasOne(e => e.Product)
                  .WithMany(e => e.Reviews)
                  .HasForeignKey(e => e.ProductId);
            entity.HasOne(e => e.User)
                  .WithMany(e => e.Reviews)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ── Coupon ──
        modelBuilder.Entity<Coupon>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Code).IsUnique();
            entity.Property(e => e.Code).HasMaxLength(50).IsRequired();
            entity.Property(e => e.DiscountValue).HasPrecision(10, 2);
            entity.Property(e => e.MinOrderAmount).HasPrecision(10, 2);
            entity.Property(e => e.MaxDiscountAmount).HasPrecision(10, 2);
        });

        // ── RefreshToken ──
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token);
            entity.Property(e => e.Token).HasMaxLength(500).IsRequired();
            entity.HasOne(e => e.User)
                  .WithMany(e => e.RefreshTokens)
                  .HasForeignKey(e => e.UserId);
        });

        // ── Seed Default Categories ──
        modelBuilder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Silk Sarees", Slug = "silk-sarees", Description = "Luxurious silk sarees", DisplayOrder = 1 },
            new Category { Id = 2, Name = "Banarasi Sarees", Slug = "banarasi-sarees", Description = "Traditional Banarasi weaves", DisplayOrder = 2 },
            new Category { Id = 3, Name = "Cotton Sarees", Slug = "cotton-sarees", Description = "Comfortable cotton sarees", DisplayOrder = 3 },
            new Category { Id = 4, Name = "Chiffon Sarees", Slug = "chiffon-sarees", Description = "Elegant chiffon sarees", DisplayOrder = 4 },
            new Category { Id = 5, Name = "Georgette Sarees", Slug = "georgette-sarees", Description = "Beautiful georgette sarees", DisplayOrder = 5 },
            new Category { Id = 6, Name = "Linen Sarees", Slug = "linen-sarees", Description = "Premium linen sarees", DisplayOrder = 6 },
            new Category { Id = 7, Name = "Bridal Sarees", Slug = "bridal-sarees", Description = "Stunning bridal collections", DisplayOrder = 7 },
            new Category { Id = 8, Name = "Designer Sarees", Slug = "designer-sarees", Description = "Exclusive designer sarees", DisplayOrder = 8 },
            new Category { Id = 9, Name = "Party Wear", Slug = "party-wear", Description = "Party wear sarees", DisplayOrder = 9 },
            new Category { Id = 10, Name = "Casual Sarees", Slug = "casual-sarees", Description = "Everyday casual sarees", DisplayOrder = 10 }
        );

        // ── Seed Admin User ──
        modelBuilder.Entity<User>().HasData(new User
        {
            Id = Guid.Parse("a1b2c3d4-e5f6-7890-abcd-ef1234567890"),
            Email = "admin@sareegrace.com",
            PasswordHash = "$2a$11$TqJnJiIjAJdVaD3IXkQ4veqoIxOJHqQXgJDxGjIgZdl.JjFtfKMKy", // Admin@123
            FirstName = "Super",
            LastName = "Admin",
            Phone = "9999999999",
            Role = "Admin",
            IsActive = true,
            EmailVerified = true,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        });
    }
}
