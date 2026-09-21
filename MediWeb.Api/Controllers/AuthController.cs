using System.Security.Claims;
using MediWeb.Api.DTOs;
using MediWeb.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MySqlConnector;

namespace MediWeb.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    // POST: api/Auth/login -> Acceso público sin token
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);
        if (result is null)
        {
            return Unauthorized(new { mensaje = "Credenciales inválidas o usuario inactivo." });
        }

        return Ok(result);
    }

    // GET: api/Auth/me -> Requiere token válido
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            id = User.FindFirst(ClaimTypes.NameIdentifier)?.Value,
            name = User.Identity?.Name,
            email = User.FindFirst(ClaimTypes.Email)?.Value,
            role = User.FindFirst(ClaimTypes.Role)?.Value
        });
    }

    // GET: api/Auth/usuarios-activos -> Exclusivo para Administrador (Reto de ampliación)
    [Authorize(Roles = "administrador")]
    [HttpGet("usuarios-activos")]
    public async Task<IActionResult> UsuariosActivos([FromServices] IConfiguration configuration)
    {
        var lista = new List<object>();
        string connectionString = configuration.GetConnectionString("MediWebConnection")!;

        const string sql = """
            SELECT u.id_usuario, CONCAT(u.nombres, ' ', u.apellidos) AS nombre, 
                   u.correo_electronico AS email, LOWER(r.nombre) AS rol
            FROM Usuario u
            INNER JOIN Rol r ON u.id_rol = r.id_rol
            WHERE u.estado = 1
            ORDER BY u.apellidos;
        """;

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(new
            {
                id = reader.GetInt64("id_usuario"),
                nombre = reader.GetString("nombre"),
                email = reader.GetString("email"),
                rol = reader.GetString("rol")
            });
        }

        return Ok(lista);
    }
}