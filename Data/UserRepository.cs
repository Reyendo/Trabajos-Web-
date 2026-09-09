using MySqlConnector;
using Practica02MediWebMVC.Models;

namespace Practica02MediWebMVC.Data;

public class UserRepository
{
    private readonly string _connectionString;

    public UserRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MediWebConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena MediWebConnection.");
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        // Unimos la tabla Usuario con Rol para obtener el nombre del rol
        const string sql = @"
            SELECT u.id_usuario, CONCAT(u.nombres, ' ', u.apellidos) AS name, 
                   u.correo_electronico, u.contrasena, r.nombre AS role, u.estado 
            FROM Usuario u
            INNER JOIN Rol r ON u.id_rol = r.id_rol
            WHERE u.correo_electronico = @email 
            LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@email", email.Trim()); // Previene inyección SQL

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new User
        {
            Id = reader.GetInt64("id_usuario"),
            Name = reader.GetString("name"),
            Email = reader.GetString("correo_electronico"),
            Password = reader.GetString("contrasena"),
            Role = reader.GetString("role"),
            Active = reader.GetBoolean("estado")
        };
    }

}