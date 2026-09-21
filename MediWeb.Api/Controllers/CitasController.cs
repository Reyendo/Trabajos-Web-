using MediWeb.Api.DTOs;
using MediWeb.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace MediWeb.Api.Controllers;

[Authorize] // Bloquea el controlador completo a solicitudes sin token
[ApiController]
[Route("api/[controller]")]
public class CitasController : ControllerBase
{
    private readonly ICitaService _service;

    public CitasController(ICitaService service)
    {
        _service = service;
    }

    // GET: api/Citas -> Accesible por administrador, medico y recepcionista[cite: 11]
    [HttpGet]
    public async Task<ActionResult<List<CitaDto>>> GetAll()
    {
        return Ok(await _service.GetAllAsync());
    }

    // GET: api/Citas/{id} -> Accesible por administrador, medico y recepcionista[cite: 11]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<CitaDto>> GetById(int id)
    {
        var cita = await _service.GetByIdAsync(id);
        if (cita is null)
            return NotFound(new { mensaje = "Cita médica no encontrada." });

        return Ok(cita);
    }

    // GET: api/Citas/buscar -> Búsqueda con parámetros accesible por todos los roles autenticados[cite: 6, 11]
    [HttpGet("buscar")]
    public async Task<ActionResult<List<CitaDto>>> Buscar([FromQuery] int? estadoId, [FromQuery] string? criterio)
    {
        return Ok(await _service.BuscarAsync(estadoId, criterio));
    }

    // POST: api/Citas -> Solo administrador y recepcionista pueden agendar citas[cite: 3, 11]
    [Authorize(Roles = "administrador,recepcionista")]
    [HttpPost]
    public async Task<ActionResult<CitaDto>> Create(CitaCreateDto dto)
    {
        try
        {
            int id = await _service.CreateAsync(dto);
            var creado = await _service.GetByIdAsync(id);
            return CreatedAtAction(nameof(GetById), new { id }, creado);
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Ya existe un registro con esos datos únicos." });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return BadRequest(new { mensaje = "El paciente, médico, consultorio o estado especificado no existe." });
        }
    }

    // PUT: api/Citas/{id} -> Solo administrador y médico pueden editar o añadir notas[cite: 3, 11]
    [Authorize(Roles = "administrador,medico")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CitaUpdateDto dto)
    {
        try
        {
            bool actualizado = await _service.UpdateAsync(id, dto);
            if (!actualizado)
                return NotFound(new { mensaje = "Cita no encontrada para actualizar." });

            return NoContent();
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            return Conflict(new { mensaje = "Conflicto con un valor único ya existente." });
        }
        catch (MySqlException ex) when (ex.Number == 1452)
        {
            return BadRequest(new { mensaje = "Uno de los identificadores relacionados no es válido." });
        }
    }

    // DELETE: api/Citas/{id} -> Solo administrador puede dar de baja una cita[cite: 3, 11]
    [Authorize(Roles = "administrador")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            bool eliminado = await _service.DeleteAsync(id);
            return eliminado ? NoContent() : NotFound(new { mensaje = "Cita no encontrada para eliminar." });
        }
        catch (MySqlException ex) when (ex.Number == 1451)
        {
            return Conflict(new
            {
                mensaje = "No se puede eliminar la cita porque tiene pagos, diagnósticos u otros registros asociados."
            });
        }
    }
}