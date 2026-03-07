using Microsoft.EntityFrameworkCore;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;
using SareeGrace.Domain.Entities;
using SareeGrace.Infrastructure.Data;

namespace SareeGrace.Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly AppDbContext _context;
    public OrderService(AppDbContext context) => _context = context;

    public async Task<ApiResponse<OrderDto>> CreateOrderAsync(Guid userId, CreateOrderDto dto)
    {
        var cartItems = await _context.CartItems
            .Include(c => c.Product)
            .Where(c => c.UserId == userId)
            .ToListAsync();

        if (!cartItems.Any())
            return ApiResponse<OrderDto>.FailResponse("Cart is empty");

        var address = await _context.Addresses.FirstOrDefaultAsync(a => a.Id == dto.ShippingAddressId && a.UserId == userId);
        if (address == null)
            return ApiResponse<OrderDto>.FailResponse("Shipping address not found");

        // Check stock
        foreach (var item in cartItems)
        {
            if (item.Product.StockCount < item.Quantity)
                return ApiResponse<OrderDto>.FailResponse($"Insufficient stock for {item.Product.Name}");
        }

        decimal subTotal = cartItems.Sum(c => c.Product.SellingPrice * c.Quantity);
        decimal shippingCharge = subTotal >= 999 ? 0 : 79; // Free shipping over ₹999
        decimal taxAmount = Math.Round(subTotal * 0.05m, 2); // 5% GST

        var order = new Order
        {
            OrderNumber = $"SG-{DateTime.UtcNow:yyMMdd}-{new Random().Next(10000, 99999)}",
            UserId = userId,
            ShippingAddressId = dto.ShippingAddressId,
            SubTotal = subTotal,
            ShippingCharge = shippingCharge,
            TaxAmount = taxAmount,
            TotalAmount = subTotal + shippingCharge + taxAmount,
            PaymentMethod = dto.PaymentMethod,
            PaymentId = dto.RazorpayPaymentId,
            CouponCode = dto.CouponCode,
            Notes = dto.Notes,
            OrderStatus = "Confirmed",
            PaymentStatus = dto.PaymentMethod == "COD" ? "Pending" : "Paid"
        };

        // Create order items & deduct stock
        foreach (var cartItem in cartItems)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = cartItem.ProductId,
                ProductName = cartItem.Product.Name,
                Quantity = cartItem.Quantity,
                UnitPrice = cartItem.Product.BasePrice,
                DiscountPercent = cartItem.Product.DiscountPercent,
                TotalPrice = cartItem.Product.SellingPrice * cartItem.Quantity,
                ImageUrl = (await _context.ProductImages.FirstOrDefaultAsync(i => i.ProductId == cartItem.ProductId && i.IsPrimary))?.ImageUrl
            });

            cartItem.Product.StockCount -= cartItem.Quantity;
        }

        await _context.Orders.AddAsync(order);
        _context.CartItems.RemoveRange(cartItems); // Clear cart
        await _context.SaveChangesAsync();

        return await GetOrderByIdAsync(userId, order.Id);
    }

    public async Task<ApiResponse<List<OrderDto>>> GetUserOrdersAsync(Guid userId)
    {
        var orders = await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return ApiResponse<List<OrderDto>>.SuccessResponse(orders.Select(MapToDto).ToList());
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

        return order == null
            ? ApiResponse<OrderDto>.FailResponse("Order not found")
            : ApiResponse<OrderDto>.SuccessResponse(MapToDto(order));
    }

    public async Task<ApiResponse<OrderDto>> UpdateOrderStatusAsync(Guid orderId, UpdateOrderStatusDto dto)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return ApiResponse<OrderDto>.FailResponse("Order not found");

        order.OrderStatus = dto.Status;
        order.TrackingNumber = dto.TrackingNumber ?? order.TrackingNumber;
        order.CourierName = dto.CourierName ?? order.CourierName;
        order.UpdatedAt = DateTime.UtcNow;

        if (dto.Status == "Delivered")
            order.DeliveredAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<OrderDto>.SuccessResponse(MapToDto(order));
    }

    public async Task<ApiResponse<bool>> CancelOrderAsync(Guid userId, Guid orderId)
    {
        var order = await _context.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);
        if (order == null) return ApiResponse<bool>.FailResponse("Order not found");
        if (order.OrderStatus is "Shipped" or "Delivered")
            return ApiResponse<bool>.FailResponse("Cannot cancel shipped/delivered order");

        order.OrderStatus = "Cancelled";
        order.UpdatedAt = DateTime.UtcNow;

        // Restore stock
        foreach (var item in order.Items)
        {
            var product = await _context.Products.FindAsync(item.ProductId);
            if (product != null) product.StockCount += item.Quantity;
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.SuccessResponse(true, "Order cancelled");
    }

    public async Task<ApiResponse<PaginatedResult<OrderDto>>> GetAllOrdersAsync(int page = 1, int pageSize = 20, string? status = null)
    {
        var query = _context.Orders.Include(o => o.Items).AsQueryable();
        if (!string.IsNullOrEmpty(status))
            query = query.Where(o => o.OrderStatus == status);

        var total = await query.CountAsync();
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return ApiResponse<PaginatedResult<OrderDto>>.SuccessResponse(new PaginatedResult<OrderDto>
        {
            Items = orders.Select(MapToDto).ToList(),
            Page = page, PageSize = pageSize, TotalItems = total
        });
    }

    public async Task<ApiResponse<DashboardDto>> GetDashboardAsync()
    {
        var totalOrders = await _context.Orders.CountAsync();
        var totalRevenue = await _context.Orders.Where(o => o.PaymentStatus == "Paid").SumAsync(o => o.TotalAmount);
        var totalCustomers = await _context.Users.CountAsync(u => u.Role == "Customer");
        var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
        var lowStock = await _context.Products.CountAsync(p => p.IsActive && p.StockCount < 5);
        var pending = await _context.Orders.CountAsync(o => o.OrderStatus == "Pending" || o.OrderStatus == "Confirmed");

        var recentOrders = await _context.Orders.Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt).Take(5).ToListAsync();

        return ApiResponse<DashboardDto>.SuccessResponse(new DashboardDto
        {
            TotalRevenue = totalRevenue,
            TotalOrders = totalOrders,
            TotalCustomers = totalCustomers,
            TotalProducts = totalProducts,
            LowStockProducts = lowStock,
            PendingOrders = pending,
            RecentOrders = recentOrders.Select(MapToDto).ToList()
        });
    }

    private static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        OrderNumber = o.OrderNumber,
        OrderStatus = o.OrderStatus,
        SubTotal = o.SubTotal,
        DiscountAmount = o.DiscountAmount,
        TaxAmount = o.TaxAmount,
        ShippingCharge = o.ShippingCharge,
        TotalAmount = o.TotalAmount,
        PaymentMethod = o.PaymentMethod,
        PaymentStatus = o.PaymentStatus,
        TrackingNumber = o.TrackingNumber,
        CreatedAt = o.CreatedAt,
        EstimatedDelivery = o.EstimatedDelivery,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            ProductId = i.ProductId,
            ProductName = i.ProductName,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            DiscountPercent = i.DiscountPercent,
            TotalPrice = i.TotalPrice,
            ImageUrl = i.ImageUrl
        }).ToList()
    };
}
