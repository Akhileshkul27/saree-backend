using SareeGrace.Application.DTOs;

namespace SareeGrace.Application.Interfaces;

public interface ICartService
{
    Task<ApiResponse<CartSummaryDto>> GetCartAsync(Guid userId);
    Task<ApiResponse<CartItemDto>> AddToCartAsync(Guid userId, AddToCartDto dto);
    Task<ApiResponse<CartItemDto>> UpdateCartItemAsync(Guid userId, int itemId, UpdateCartItemDto dto);
    Task<ApiResponse<bool>> RemoveFromCartAsync(Guid userId, int itemId);
    Task<ApiResponse<bool>> ClearCartAsync(Guid userId);
}
