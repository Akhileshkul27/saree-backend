using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SareeGrace.Application.DTOs;
using SareeGrace.Application.Interfaces;

namespace SareeGrace.API.Controllers;

[Route("api/[controller]")]
[Authorize]
public class AddressesController : BaseApiController
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    /// <summary>Get all addresses for current user</summary>
    [HttpGet]
    public async Task<IActionResult> GetAddresses()
    {
        var result = await _addressService.GetAddressesAsync(GetUserId());
        return ApiResult(result);
    }

    /// <summary>Add a new address</summary>
    [HttpPost]
    public async Task<IActionResult> AddAddress([FromBody] CreateAddressDto dto)
    {
        var result = await _addressService.AddAddressAsync(GetUserId(), dto);
        return ApiResult(result);
    }

    /// <summary>Update an address</summary>
    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAddress(int id, [FromBody] CreateAddressDto dto)
    {
        var result = await _addressService.UpdateAddressAsync(GetUserId(), id, dto);
        return ApiResult(result);
    }

    /// <summary>Delete an address</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteAddress(int id)
    {
        var result = await _addressService.DeleteAddressAsync(GetUserId(), id);
        return ApiResult(result);
    }
}
