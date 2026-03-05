using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class CartController : BaseApiController
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    /// <summary>Get current user's cart</summary>
    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var result = await _cartService.GetCartAsync(GetUserId());
        return ApiResult(result);
    }

    /// <summary>Add item to cart</summary>
    [HttpPost]
    public async Task<IActionResult> AddToCart([FromBody] AddToCartDto dto)
    {
        var result = await _cartService.AddToCartAsync(GetUserId(), dto);
        return ApiResult(result);
    }

    /// <summary>Update cart item quantity</summary>
    [HttpPut("{itemId:int}")]
    public async Task<IActionResult> UpdateCartItem(int itemId, [FromBody] UpdateCartItemDto dto)
    {
        var result = await _cartService.UpdateCartItemAsync(GetUserId(), itemId, dto);
        return ApiResult(result);
    }

    /// <summary>Remove item from cart</summary>
    [HttpDelete("{itemId:int}")]
    public async Task<IActionResult> RemoveFromCart(int itemId)
    {
        var result = await _cartService.RemoveFromCartAsync(GetUserId(), itemId);
        return ApiResult(result);
    }

    /// <summary>Clear entire cart</summary>
    [HttpDelete("clear")]
    public async Task<IActionResult> ClearCart()
    {
        var result = await _cartService.ClearCartAsync(GetUserId());
        return ApiResult(result);
    }
}
