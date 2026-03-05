namespace SareeGrace.Domain.Entities;

public class Order
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string OrderNumber { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public int ShippingAddressId { get; set; }
    public string OrderStatus { get; set; } = "Pending";
    public decimal SubTotal { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    public decimal ShippingCharge { get; set; } = 0;
    public decimal TotalAmount { get; set; }
    public string? CouponCode { get; set; }
    public string? PaymentMethod { get; set; }
    public string? PaymentId { get; set; }
    public string PaymentStatus { get; set; } = "Pending";
    public string? TrackingNumber { get; set; }
    public string? CourierName { get; set; }
    public DateTime? EstimatedDelivery { get; set; }
    public DateTime? DeliveredAt { get; set; }
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public User User { get; set; } = null!;
    public Address ShippingAddress { get; set; } = null!;
    public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
}
