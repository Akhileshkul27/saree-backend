using SareeGrace.Application.DTOs;

namespace SareeGrace.Application.Interfaces;

public interface IWishlistService
{
    Task<ApiResponse<List<WishlistItemDto>>> GetWishlistAsync(Guid userId);
    Task<ApiResponse<WishlistItemDto>> AddToWishlistAsync(Guid userId, Guid productId);
    Task<ApiResponse<bool>> RemoveFromWishlistAsync(Guid userId, Guid productId);
}

public interface IAddressService
{
    Task<ApiResponse<List<AddressDto>>> GetAddressesAsync(Guid userId);
    Task<ApiResponse<AddressDto>> AddAddressAsync(Guid userId, CreateAddressDto dto);
    Task<ApiResponse<AddressDto>> UpdateAddressAsync(Guid userId, int addressId, CreateAddressDto dto);
    Task<ApiResponse<bool>> DeleteAddressAsync(Guid userId, int addressId);
}

public interface IReviewService
{
    Task<ApiResponse<List<ReviewDto>>> GetProductReviewsAsync(Guid productId);
    Task<ApiResponse<ReviewDto>> AddReviewAsync(Guid userId, CreateReviewDto dto);
    Task<ApiResponse<bool>> ApproveReviewAsync(int reviewId);
    Task<ApiResponse<bool>> DeleteReviewAsync(int reviewId);
}

public interface IImageService
{
    Task<string> SaveImageAsync(Stream imageStream, string fileName, string folder = "products");
    bool DeleteImage(string imagePath);
}
