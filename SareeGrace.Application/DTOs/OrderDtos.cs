namespace SareeGrace.Application.DTOs;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string OrderStatus { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal ShippingCharge { get; set; }
    public decimal TotalAmount { get; set; }
    public string? PaymentMethod { get; set; }
    public string PaymentStatus { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TotalPrice { get; set; }
    public string? ImageUrl { get; set; }
}

public class CreateOrderDto
{
    public int ShippingAddressId { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
    public string? RazorpayPaymentId { get; set; }
    public string? RazorpayOrderId { get; set; }
}

public class CreateRazorpayOrderDto
{
    public decimal Amount { get; set; }
}

public class RazorpayOrderResponseDto
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public int Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string KeyId { get; set; } = string.Empty;
}

public class PaymentVerifyDto
{
    public string RazorpayOrderId { get; set; } = string.Empty;
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string RazorpaySignature { get; set; } = string.Empty;
    public int ShippingAddressId { get; set; }
    public string? CouponCode { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderStatusDto
{
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public string? CourierName { get; set; }
}
