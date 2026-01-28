using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WaqfGIS.Services;

namespace WaqfGIS.Web.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class GeoDataController : ControllerBase
{
    private readonly MosqueService _mosqueService;
    private readonly PropertyService _propertyService;

    public GeoDataController(MosqueService mosqueService, PropertyService propertyService)
    {
        _mosqueService = mosqueService;
        _propertyService = propertyService;
    }

    [HttpGet("mosques")]
    public async Task<IActionResult> GetMosques()
    {
        var data = await _mosqueService.GetForMapAsync();
        return Ok(data);
    }

    [HttpGet("properties")]
    public async Task<IActionResult> GetProperties()
    {
        var data = await _propertyService.GetForMapAsync();
        return Ok(data);
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll()
    {
        var mosques = await _mosqueService.GetForMapAsync();
        var properties = await _propertyService.GetForMapAsync();
        
        return Ok(new
        {
            Mosques = mosques,
            Properties = properties
        });
    }
}
