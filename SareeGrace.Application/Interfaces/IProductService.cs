using SareeGrace.Application.DTOs;

namespace SareeGrace.Application.Interfaces;

public interface IProductService
{
    Task<ApiResponse<PaginatedResult<ProductDto>>> GetProductsAsync(ProductFilterDto filter);
    Task<ApiResponse<ProductDetailDto>> GetProductByIdAsync(Guid id);
    Task<ApiResponse<ProductDetailDto>> GetProductBySlugAsync(string slug);
    Task<ApiResponse<List<ProductDto>>> GetFeaturedProductsAsync(int count = 8);
    Task<ApiResponse<List<ProductDto>>> GetSpecialOffersAsync(int count = 12);
    Task<ApiResponse<List<ProductDto>>> GetNewArrivalsAsync(int count = 12);
    Task<ApiResponse<List<ProductDto>>> GetRelatedProductsAsync(Guid productId, int count = 6);
    Task<ApiResponse<ProductDetailDto>> CreateProductAsync(CreateProductDto dto);
    Task<ApiResponse<ProductDetailDto>> UpdateProductAsync(Guid id, UpdateProductDto dto);
    Task<ApiResponse<bool>> DeleteProductAsync(Guid id, bool permanent);
    Task<ApiResponse<bool>> CheckProductHasOrdersAsync(Guid id);
    Task<ApiResponse<bool>> UpdateStockAsync(Guid id, int quantity);
    Task<ApiResponse<ProductImageDto>> AddProductImageAsync(Guid productId, string imageUrl, bool isPrimary = false);
    Task<ApiResponse<bool>> DeleteProductImageAsync(int imageId);
    Task<ApiResponse<ProductFilterOptionsDto>> GetFilterOptionsAsync();
    Task<ApiResponse<BulkCreateResultDto>> BulkCreateProductsAsync(List<BulkProductRowDto> rows);
}
