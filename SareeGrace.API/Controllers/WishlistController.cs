using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class WishlistController : BaseApiController
{
    private readonly IWishlistService _wishlistService;

    public WishlistController(IWishlistService wishlistService)
    {
        _wishlistService = wishlistService;
    }

    /// <summary>Get user's wishlist</summary>
    [HttpGet]
    public async Task<IActionResult> GetWishlist()
    {
        var result = await _wishlistService.GetWishlistAsync(GetUserId());
        return ApiResult(result);
    }

    /// <summary>Add product to wishlist</summary>
    [HttpPost("{productId:guid}")]
    public async Task<IActionResult> AddToWishlist(Guid productId)
    {
        var result = await _wishlistService.AddToWishlistAsync(GetUserId(), productId);
        return ApiResult(result);
    }

    /// <summary>Remove product from wishlist</summary>
    [HttpDelete("{productId:guid}")]
    public async Task<IActionResult> RemoveFromWishlist(Guid productId)
    {
        var result = await _wishlistService.RemoveFromWishlistAsync(GetUserId(), productId);
        return ApiResult(result);
    }
}
