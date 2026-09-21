using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace MediWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogosController : ControllerBase
{
    private readonly string _connectionString;

    public CatalogosController(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MediWebConnection")
            ?? throw new InvalidOperationException("No existe la cadena MediWebConnection.");
    }

    // GET: api/Catalogos/pacientes[cite: 6]
    [HttpGet("pacientes")]
    public async Task<IActionResult> Pacientes()
    {
        const string sql = @"
            SELECT p.id_paciente AS id, CONCAT(u.nombres, ' ', u.apellidos) AS nombre 
            FROM Paciente p 
            INNER JOIN Usuario u ON p.id_usuario = u.id_usuario 
            ORDER BY u.apellidos;";

        return Ok(await ConsultarAsync(sql));
    }

    // GET: api/Catalogos/medicos[cite: 6]
    [HttpGet("medicos")]
    public async Task<IActionResult> Medicos()
    {
        const string sql = @"
            SELECT m.id_medico AS id, CONCAT('Dr. ', u.nombres, ' ', u.apellidos, ' (', esp.nombre, ')') AS nombre 
            FROM Medico m 
            INNER JOIN Usuario u ON m.id_usuario = u.id_usuario 
            INNER JOIN Especialidad esp ON m.id_especialidad = esp.id_especialidad 
            ORDER BY u.apellidos;";

        return Ok(await ConsultarAsync(sql));
    }

    // GET: api/Catalogos/consultorios[cite: 6]
    [HttpGet("consultorios")]
    public async Task<IActionResult> Consultorios()
    {
        const string sql = "SELECT id_consultorio AS id, nombre FROM Consultorio ORDER BY nombre;";
        return Ok(await ConsultarAsync(sql));
    }

    // GET: api/Catalogos/estados[cite: 6]
    [HttpGet("estados")]
    public async Task<IActionResult> Estados()
    {
        const string sql = "SELECT id_estado AS id, nombre FROM Estado_Cita ORDER BY id_estado;";
        return Ok(await ConsultarAsync(sql));
    }

    private async Task<List<object>> ConsultarAsync(string sql)
    {
        var lista = new List<object>();

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(new
            {
                id = reader.GetInt32("id"),
                nombre = reader.GetString("nombre")
            });
        }

        return lista;
    }
}