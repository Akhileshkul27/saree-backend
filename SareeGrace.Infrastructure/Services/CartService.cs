using Microsoft.EntityFrameworkCore;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;
using SareeGrace.Domain.Entities;
using SareeGrace.Infrastructure.Data;

namespace SareeGrace.Infrastructure.Services;

public class CartService : ICartService
{
    private readonly AppDbContext _context;
    public CartService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<CartSummaryDto>> GetCartAsync(Guid userId)
    {
        var items = await _context.CartItems
            .Include(c => c.Product).ThenInclude(p => p.Images)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        var cartItems = items.Select(c => new CartItemDto
        {
            Id = c.Id,
            ProductId = c.ProductId,
            ProductName = c.Product.Name,
            ImageUrl = c.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
            BasePrice = c.Product.BasePrice,
            DiscountPercent = c.Product.DiscountPercent,
            SellingPrice = c.Product.SellingPrice,
            Quantity = c.Quantity,
            TotalPrice = c.Product.SellingPrice * c.Quantity,
            StockCount = c.Product.StockCount
        }).ToList();

        return ApiResponse<CartSummaryDto>.SuccessResponse(new CartSummaryDto
        {
            Items = cartItems,
            SubTotal = cartItems.Sum(i => i.TotalPrice),
            TotalItems = cartItems.Sum(i => i.Quantity)
        });
    }

    public async Task<ApiResponse<CartItemDto>> AddToCartAsync(Guid userId, AddToCartDto dto)
    {
        var product = await _context.Products.Include(p => p.Images).FirstOrDefaultAsync(p => p.Id == dto.ProductId);
        if (product == null) return ApiResponse<CartItemDto>.FailResponse("Product not found");
        if (product.StockCount < dto.Quantity) return ApiResponse<CartItemDto>.FailResponse("Insufficient stock");

        var existing = await _context.CartItems.FirstOrDefaultAsync(c => c.UserId == userId && c.ProductId == dto.ProductId);
        if (existing != null)
        {
            existing.Quantity += dto.Quantity;
            await _context.SaveChangesAsync();
        }
        else
        {
            existing = new CartItem { UserId = userId, ProductId = dto.ProductId, Quantity = dto.Quantity };
            await _context.CartItems.AddAsync(existing);
            await _context.SaveChangesAsync();
        }

        return ApiResponse<CartItemDto>.SuccessResponse(new CartItemDto
        {
            Id = existing.Id,
            ProductId = product.Id,
            ProductName = product.Name,
            ImageUrl = product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
            BasePrice = product.BasePrice,
            DiscountPercent = product.DiscountPercent,
            SellingPrice = product.SellingPrice,
            Quantity = existing.Quantity,
            TotalPrice = product.SellingPrice * existing.Quantity,
            StockCount = product.StockCount
        }, "Item added to cart");
    }

    public async Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(Guid userId, int itemId, UpdateCartItemDto dto)
    {
        var item = await _context.CartItems.Include(c => c.Product).ThenInclude(p => p.Images)
            .FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);
        if (item == null) return ApiResponse<CartItemDto>.FailResponse("Cart item not found");

        item.Quantity = dto.Quantity;
        await _context.SaveChangesAsync();

        return ApiResponse<CartItemDto>.SuccessResponse(new CartItemDto
        {
            Id = item.Id,
            ProductId = item.ProductId,
            ProductName = item.Product.Name,
            ImageUrl = item.Product.Images.FirstOrDefault(i => i.IsPrimary)?.ImageUrl,
            BasePrice = item.Product.BasePrice,
            DiscountPercent = item.Product.DiscountPercent,
            SellingPrice = item.Product.SellingPrice,
            Quantity = item.Quantity,
            TotalPrice = item.Product.SellingPrice * item.Quantity,
            StockCount = item.Product.StockCount
        });
    }

    public async Task<ApiResponse<bool>> RemoveFromCartAsync(Guid userId, int itemId)
    {
        var item = await _context.CartItems.FirstOrDefaultAsync(c => c.Id == itemId && c.UserId == userId);
        if (item == null) return ApiResponse<bool>.FailResponse("Cart item not found");

        _context.CartItems.Remove(item);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Item removed from cart");
    }

    public async Task<ApiResponse<bool>> ClearCartAsync(Guid userId)
    {
        var items = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();
        _context.CartItems.RemoveRange(items);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Cart cleared");
    }
}
