using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
public class ReviewsController : BaseApiController
{
    private readonly IReviewService _reviewService;

    public ReviewsController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    /// <summary>Get reviews for a product</summary>
    [HttpGet("product/{productId:guid}")]
    public async Task<IActionResult> GetProductReviews(Guid productId)
    {
        var result = await _reviewService.GetProductReviewsAsync(productId);
        return ApiResult(result);
    }

    /// <summary>Add a review (authenticated users only)</summary>
    [Authorize]
    [HttpPost]
    public async Task<IActionResult> AddReview([FromBody] CreateReviewDto dto)
    {
        var result = await _reviewService.AddReviewAsync(GetUserId(), dto);
        return ApiResult(result);
    }
}
