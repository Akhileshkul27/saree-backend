using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class OrdersController : BaseApiController
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>Create a new order from cart</summary>
    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto dto)
    {
        var result = await _orderService.CreateOrderAsync(GetUserId(), dto);
        return ApiResult(result);
    }

    /// <summary>Get current user's orders</summary>
    [HttpGet]
    public async Task<IActionResult> GetUserOrders()
    {
        var result = await _orderService.GetUserOrdersAsync(GetUserId());
        return ApiResult(result);
    }

    /// <summary>Get specific order details</summary>
    [HttpGet("{orderId:guid}")]
    public async Task<IActionResult> GetOrder(Guid orderId)
    {
        var result = await _orderService.GetOrderByIdAsync(GetUserId(), orderId);
        return ApiResult(result);
    }

    /// <summary>Cancel an order</summary>
    [HttpPost("{orderId:guid}/cancel")]
    public async Task<IActionResult> CancelOrder(Guid orderId)
    {
        var result = await _orderService.CancelOrderAsync(GetUserId(), orderId);
        return ApiResult(result);
    }
}
