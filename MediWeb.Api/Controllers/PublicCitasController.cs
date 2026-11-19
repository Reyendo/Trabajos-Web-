using MediWeb.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MediWeb.Api.Controllers;

[ApiController]
[Route("api/public/citas")]
public class PublicCitasController : ControllerBase
{
    private readonly ICitaService _service;

    public PublicCitasController(ICitaService service)
    {
        _service = service;
    }

    // GET público para aprender consumo en React sin JWT
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var citas = await _service.GetAllAsync();
        return Ok(citas);
    }
}