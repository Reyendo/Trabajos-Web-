using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using MediWeb.Api.DTOs;
using MediWeb.Api.Models;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;

namespace MediWeb.Api.Services;

public class AuthService : IAuthService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = configuration.GetConnectionString("MediWebConnection")
            ?? throw new InvalidOperationException("No existe la cadena MediWebConnection.");
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await FindByEmailAsync(dto.Email);

        // Si no existe o está inhabilitado, denegamos el acceso
        if (user is null || !user.Active)
            return null;

        // Validación criptográfica con BCrypt sin exponer texto plano
        bool passwordOk = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
        if (!passwordOk)
            return null;

        return CreateToken(user);
    }

    private async Task<AuthUser?> FindByEmailAsync(string email)
    {
        const string sql = """
            SELECT u.id_usuario, CONCAT(u.nombres, ' ', u.apellidos) AS name,
                   u.correo_electronico, u.contrasena, LOWER(r.nombre) AS role, u.estado
            FROM Usuario u
            INNER JOIN Rol r ON u.id_rol = r.id_rol
            WHERE u.correo_electronico = @email
            LIMIT 1;
        """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email.Trim()); // Consulta parametrizada

        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
            return null;

        return new AuthUser
        {
            Id = reader.GetInt64("id_usuario"),
            Name = reader.GetString("name"),
            Email = reader.GetString("correo_electronico"),
            PasswordHash = reader.GetString("contrasena"),
            Role = reader.GetString("role"),
            Active = reader.GetBoolean("estado")
        };
    }

    private LoginResponseDto CreateToken(AuthUser user)
    {
        string key = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key no configurado en appsettings.json.");
        string issuer = _configuration["Jwt:Issuer"] ?? "MediWeb.Api";
        string audience = _configuration["Jwt:Audience"] ?? "MediWeb.Client";
        int expireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "5");

        DateTime expires = DateTime.UtcNow.AddMinutes(expireMinutes);

        // Estructuración de Claims en el Payload del token
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role) // Permite la autorización por [Authorize(Roles = "...")]
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        string token = new JwtSecurityTokenHandler().WriteToken(jwt);

        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expires,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role
        };
    }
}