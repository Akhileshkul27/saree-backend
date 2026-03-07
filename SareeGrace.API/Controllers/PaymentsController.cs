using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Razorpay.Api;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class PaymentsController : BaseApiController
{
    private readonly IOrderService _orderService;
    private readonly IConfiguration _config;

    public PaymentsController(IOrderService orderService, IConfiguration config)
    {
        _orderService = orderService;
        _config = config;
    }

    /// <summary>Create a Razorpay order to initiate the payment flow</summary>
    [HttpPost("create-razorpay-order")]
    public IActionResult CreateRazorpayOrder([FromBody] CreateRazorpayOrderDto dto)
    {
        var keyId = _config["Razorpay:KeyId"]!;
        var keySecret = _config["Razorpay:KeySecret"]!;

        var client = new RazorpayClient(keyId, keySecret);

        var options = new Dictionary<string, object>
        {
            { "amount", (int)(dto.Amount * 100) }, // Razorpay expects amount in paisa
            { "currency", "INR" },
            { "receipt", $"rcpt_{Guid.NewGuid():N}" },
            { "payment_capture", 1 }
        };

        var razorpayOrder = client.Order.Create(options);

        var response = new RazorpayOrderResponseDto
        {
            RazorpayOrderId = razorpayOrder["id"].ToString()!,
            Amount = (int)(dto.Amount * 100),
            Currency = "INR",
            KeyId = keyId
        };

        return Ok(ApiResponse<RazorpayOrderResponseDto>.SuccessResponse(response));
    }

    /// <summary>Verify Razorpay payment signature and create the DB order</summary>
    [HttpPost("verify")]
    public async Task<IActionResult> VerifyPayment([FromBody] PaymentVerifyDto dto)
    {
        var keySecret = _config["Razorpay:KeySecret"]!;

        // Verify HMAC SHA256 signature as per Razorpay docs
        var payload = $"{dto.RazorpayOrderId}|{dto.RazorpayPaymentId}";
        var key = Encoding.UTF8.GetBytes(keySecret);
        using var hmac = new HMACSHA256(key);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        var generatedSignature = Convert.ToHexString(hash).ToLower();

        if (generatedSignature != dto.RazorpaySignature)
            return BadRequest(ApiResponse<string>.FailResponse("Payment verification failed. Invalid signature."));

        // Payment is verified — now create the actual order in the DB
        var createOrderDto = new CreateOrderDto
        {
            ShippingAddressId = dto.ShippingAddressId,
            PaymentMethod = "Razorpay",
            CouponCode = dto.CouponCode,
            Notes = dto.Notes,
            RazorpayPaymentId = dto.RazorpayPaymentId,
            RazorpayOrderId = dto.RazorpayOrderId,
        };

        var result = await _orderService.CreateOrderAsync(GetUserId(), createOrderDto);
        return ApiResult(result);
    }
}
