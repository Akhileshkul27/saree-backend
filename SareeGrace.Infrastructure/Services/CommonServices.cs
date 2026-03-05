using Microsoft.EntityFrameworkCore;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;
using SareeGrace.Domain.Entities;
using SareeGrace.Infrastructure.Data;

namespace SareeGrace.Infrastructure.Services;

public class WishlistService : IWishlistService
{
    private readonly AppDbContext _context;
    public WishlistService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<WishlistItemDto>>> GetWishlistAsync(Guid userId)
    {
        var items = await _context.WishlistItems
            .Include(w => w.Product).ThenInclude(p => p.Images)
            .Where(w => w.UserId == userId)
            .OrderByDescending(w => w.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<WishlistItemDto>>.SuccessResponse(items.Select(w => new WishlistItemDto
        {
            Id = w.Id,
            ProductId = w.ProductId,
            ProductName = w.Product.Name,
            ImageUrl = w.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
            BasePrice = w.Product.BasePrice,
            SellingPrice = w.Product.SellingPrice,
            InStock = w.Product.StockCount > 0,
            AddedAt = w.CreatedAt
        }).ToList());
    }

    public async Task<ApiResponse<WishlistItemDto>> AddToWishlistAsync(Guid userId, Guid productId)
    {
        if (await _context.WishlistItems.AnyAsync(w => w.UserId == userId && w.ProductId == productId))
            return ApiResponse<WishlistItemDto>.FailResponse("Already in wishlist");

        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == productId);
        if (product == null) return ApiResponse<WishlistItemDto>.FailResponse("Product not found");

        var item = new WishlistItem { UserId = userId, ProductId = productId };
        await _context.WishlistItems.AddAsync(item);
        await _context.SaveChangesAsync();

        return ApiResponse<WishlistItemDto>.SuccessResponse(new WishlistItemDto
        {
            Id = item.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            ImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
            BasePrice = product.BasePrice,
            SellingPrice = product.SellingPrice,
            InStock = product.StockCount > 0,
            AddedAt = item.CreatedAt
        }, "Added to wishlist");
    }

    public async Task<ApiResponse<bool>> RemoveFromWishlistAsync(Guid userId, Guid productId)
    {
        var item = await _context.WishlistItems.FirstOrDefaultAsync(w => w.UserId == userId && w.ProductId == productId);
        if (item == null) return ApiResponse<bool>.FailResponse("Item not in wishlist");

        _context.WishlistItems.Remove(item);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Removed from wishlist");
    }
}

public class AddressService : IAddressService
{
    private readonly AppDbContext _context;
    public AddressService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<AddressDto>>> GetAddressesAsync(Guid userId)
    {
        var addresses = await _context.Addresses.Where(a => a.UserId == userId).ToListAsync();
        return ApiResponse<List<AddressDto>>.SuccessResponse(addresses.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<AddressDto>> AddAddressAsync(Guid userId, CreateAddressDto dto)
    {
        if (dto.IsDefault)
        {
            var existing = await _context.Addresses.Where(a => a.UserId == userId && a.IsDefault).ToListAsync();
            existing.ForEach(a => a.IsDefault = false);
        }

        var address = new Address
        {
            UserId = userId,
            FullName = dto.FullName,
            Phone = dto.Phone,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            Pincode = dto.Pincode,
            Country = dto.Country,
            IsDefault = dto.IsDefault,
            AddressType = dto.AddressType
        };

        await _context.Addresses.AddAsync(address);
        await _context.SaveChangesAsync();
        return ApiResponse<AddressDto>.SuccessResponse(MapToDto(address), "Address added");
    }

    public async Task<ApiResponse<AddressDto>> UpdateAddressAsync(Guid userId, int addressId, CreateAddressDto dto)
    {
        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        if (address == null) return ApiResponse<AddressDto>.FailResponse("Address not found");

        if (dto.IsDefault)
        {
            var existing = await _context.Addresses.Where(a => a.UserId == userId && a.IsDefault && a.Id != addressId).ToListAsync();
            existing.ForEach(a => a.IsDefault = false);
        }

        address.FullName = dto.FullName;
        address.Phone = dto.Phone;
        address.AddressLine1 = dto.AddressLine1;
        address.AddressLine2 = dto.AddressLine2;
        address.City = dto.City;
        address.State = dto.State;
        address.Pincode = dto.Pincode;
        address.Country = dto.Country;
        address.IsDefault = dto.IsDefault;
        address.AddressType = dto.AddressType;
        await _context.SaveChangesAsync();

        return ApiResponse<AddressDto>.SuccessResponse(MapToDto(address), "Address updated");
    }

    public async Task<ApiResponse<bool>> DeleteAddressAsync(Guid userId, int addressId)
    {
        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        if (address == null) return ApiResponse<bool>.FailResponse("Address not found");

        _context.Addresses.Remove(address);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Address deleted");
    }

    private static AddressDto MapToDto(Address a) => new()
    {
        Id = a.Id, FullName = a.FullName, Phone = a.Phone,
        AddressLine1 = a.AddressLine1, AddressLine2 = a.AddressLine2,
        City = a.City, State = a.State, Pincode = a.Pincode,
        Country = a.Country, IsDefault = a.IsDefault, AddressType = a.AddressType
    };
}

public class ReviewService : IReviewService
{
    private readonly AppDbContext _context;
    public ReviewService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<List<ReviewDto>>> GetProductReviewsAsync(Guid productId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<ReviewDto>>.SuccessResponse(reviews.Select(r => new ReviewDto
        {
            Id = r.Id, ProductId = r.ProductId,
            UserName = $"{r.User.FirstName} {r.User.LastName}".Trim(),
            Rating = r.Rating, Title = r.Title, Comment = r.Comment,
            IsVerifiedPurchase = r.IsVerifiedPurchase, CreatedAt = r.CreatedAt
        }).ToList());
    }

    public async Task<ApiResponse<ReviewDto>> AddReviewAsync(Guid userId, CreateReviewDto dto)
    {
        var hasPurchased = await _context.OrderItems.AnyAsync(oi =>
            oi.Order.UserId == userId && oi.ProductId == dto.ProductId);

        var review = new Review
        {
            ProductId = dto.ProductId,
            UserId = userId,
            Rating = Math.Clamp(dto.Rating, 1, 5),
            Title = dto.Title,
            Comment = dto.Comment,
            IsVerifiedPurchase = hasPurchased,
            IsApproved = true // Auto-approve for now
        };

        await _context.Reviews.AddAsync(review);

        // Update product average rating
        var product = await _context.Products.FindAsync(dto.ProductId);
        if (product != null)
        {
            var allRatings = await _context.Reviews.Where(r => r.ProductId == dto.ProductId && r.IsApproved).ToListAsync();
            allRatings.Add(review);
            product.AverageRating = (decimal)allRatings.Average(r => r.Rating);
            product.ReviewCount = allRatings.Count;
        }

        await _context.SaveChangesAsync();
        var user = await _context.Users.FindAsync(userId);

        return ApiResponse<ReviewDto>.SuccessResponse(new ReviewDto
        {
            Id = review.Id, ProductId = review.ProductId,
            UserName = $"{user?.FirstName} {user?.LastName}".Trim(),
            Rating = review.Rating, Title = review.Title, Comment = review.Comment,
            IsVerifiedPurchase = review.IsVerifiedPurchase, CreatedAt = review.CreatedAt
        }, "Review added");
    }

    public async Task<ApiResponse<bool>> ApproveReviewAsync(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null) return ApiResponse<bool>.FailResponse("Review not found");
        review.IsApproved = true;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true);
    }

    public async Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null) return ApiResponse<bool>.FailResponse("Review not found");
        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true);
    }
}
