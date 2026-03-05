using SareeGrace.Application.DTOs;

namespace SareeGrace.Application.Interfaces;

public interface IOrderService
{
    Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderDto dto);
    Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(Guid userId);
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid userId, Guid orderId);
    Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto);
    Task<ApiResponse<bool>> CancelOrderAsync(Guid userId, Guid orderId);
    Task<ApiResponse<PaginatedResult<OrderDto>>> GetAllOrdersAsync(int page = 1, int pageSize = 20, string? status = null);
    Task<ApiResponse<DashboardDto>> GetDashboardAsync();
}
