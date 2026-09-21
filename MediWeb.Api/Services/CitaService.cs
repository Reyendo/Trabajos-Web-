using MediWeb.Api.DTOs;
using MySqlConnector;

namespace MediWeb.Api.Services;

public class CitaService : ICitaService
{
    private readonly string _connectionString;

    public CitaService(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MediWebConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena MediWebConnection.");
    }

    public async Task<List<CitaDto>> GetAllAsync()
    {
        var lista = new List<CitaDto>();
        const string sql = """
            SELECT c.id_cita,
                   c.id_paciente, CONCAT(u_pac.nombres, ' ', u_pac.apellidos) AS paciente,
                   c.id_medico, CONCAT('Dr. ', u_med.nombres, ' ', u_med.apellidos) AS medico,
                   c.id_consultorio, cons.nombre AS consultorio,
                   c.id_estado, est.nombre AS estado,
                   c.fecha_hora, c.motivo, c.notas_medicas
            FROM Cita c
            INNER JOIN Paciente p ON c.id_paciente = p.id_paciente
            INNER JOIN Usuario u_pac ON p.id_usuario = u_pac.id_usuario
            INNER JOIN Medico m ON c.id_medico = m.id_medico
            INNER JOIN Usuario u_med ON m.id_usuario = u_med.id_usuario
            INNER JOIN Consultorio cons ON c.id_consultorio = cons.id_consultorio
            INNER JOIN Estado_Cita est ON c.id_estado = est.id_estado
            ORDER BY c.fecha_hora DESC;
        """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            lista.Add(MapCita(reader));
        }

        return lista;
    }

    public async Task<CitaDto?> GetByIdAsync(int id)
    {
        const string sql = """
            SELECT c.id_cita,
                   c.id_paciente, CONCAT(u_pac.nombres, ' ', u_pac.apellidos) AS paciente,
                   c.id_medico, CONCAT('Dr. ', u_med.nombres, ' ', u_med.apellidos) AS medico,
                   c.id_consultorio, cons.nombre AS consultorio,
                   c.id_estado, est.nombre AS estado,
                   c.fecha_hora, c.motivo, c.notas_medicas
            FROM Cita c
            INNER JOIN Paciente p ON c.id_paciente = p.id_paciente
            INNER JOIN Usuario u_pac ON p.id_usuario = u_pac.id_usuario
            INNER JOIN Medico m ON c.id_medico = m.id_medico
            INNER JOIN Usuario u_med ON m.id_usuario = u_med.id_usuario
            INNER JOIN Consultorio cons ON c.id_consultorio = cons.id_consultorio
            INNER JOIN Estado_Cita est ON c.id_estado = est.id_estado
            WHERE c.id_cita = @id
            LIMIT 1;
        """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync();
        return await reader.ReadAsync() ? MapCita(reader) : null;
    }

    public async Task<int> CreateAsync(CitaCreateDto dto)
    {
        const string sql = """
            INSERT INTO Cita 
            (id_paciente, id_medico, id_consultorio, id_estado, fecha_hora, motivo, notas_medicas)
            VALUES 
            (@paciente, @medico, @consultorio, @estado, @fechaHora, @motivo, @notas);
            SELECT LAST_INSERT_ID();
        """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, dto);

        return Convert.ToInt32(await command.ExecuteScalarAsync());
    }

    public async Task<bool> UpdateAsync(int id, CitaUpdateDto dto)
    {
        const string sql = """
            UPDATE Cita
            SET id_paciente = @paciente,
                id_medico = @medico,
                id_consultorio = @consultorio,
                id_estado = @estado,
                fecha_hora = @fechaHora,
                motivo = @motivo,
                notas_medicas = @notas
            WHERE id_cita = @id;
        """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, dto);
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        const string sql = "DELETE FROM Cita WHERE id_cita = @id;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);

        return await command.ExecuteNonQueryAsync() > 0;
    }

    private static void AddParameters(MySqlCommand command, CitaCreateDto dto)
    {
        command.Parameters.AddWithValue("@paciente", dto.PacienteId);
        command.Parameters.AddWithValue("@medico", dto.MedicoId);
        command.Parameters.AddWithValue("@consultorio", dto.ConsultorioId);
        command.Parameters.AddWithValue("@estado", dto.EstadoId);
        command.Parameters.AddWithValue("@fechaHora", dto.FechaHora);
        command.Parameters.AddWithValue("@motivo", dto.Motivo.Trim());
        command.Parameters.AddWithValue("@notas", (object?)dto.NotasMedicas ?? DBNull.Value);
    }

    private static CitaDto MapCita(MySqlDataReader reader)
    {
        return new CitaDto
        {
            CitaId = reader.GetInt32("id_cita"),
            PacienteId = reader.GetInt32("id_paciente"),
            Paciente = reader.GetString("paciente"),
            MedicoId = reader.GetInt32("id_medico"),
            Medico = reader.GetString("medico"),
            ConsultorioId = reader.GetInt32("id_consultorio"),
            Consultorio = reader.GetString("consultorio"),
            EstadoId = reader.GetInt32("id_estado"),
            Estado = reader.GetString("estado"),
            FechaHora = reader.GetDateTime("fecha_hora"),
            Motivo = reader.GetString("motivo"),
            NotasMedicas = reader.IsDBNull(reader.GetOrdinal("notas_medicas"))
                ? null
                : reader.GetString("notas_medicas")
        };
    }
    public async Task<List<CitaDto>> BuscarAsync(int? estadoId, string? criterio)
    {
        var lista = new List<CitaDto>();
        string sql = """
        SELECT c.id_cita,
               c.id_paciente, CONCAT(u_pac.nombres, ' ', u_pac.apellidos) AS paciente,
               c.id_medico, CONCAT('Dr. ', u_med.nombres, ' ', u_med.apellidos) AS medico,
               c.id_consultorio, cons.nombre AS consultorio,
               c.id_estado, est.nombre AS estado,
               c.fecha_hora, c.motivo, c.notas_medicas
        FROM Cita c
        INNER JOIN Paciente p ON c.id_paciente = p.id_paciente
        INNER JOIN Usuario u_pac ON p.id_usuario = u_pac.id_usuario
        INNER JOIN Medico m ON c.id_medico = m.id_medico
        INNER JOIN Usuario u_med ON m.id_usuario = u_med.id_usuario
        INNER JOIN Consultorio cons ON c.id_consultorio = cons.id_consultorio
        INNER JOIN Estado_Cita est ON c.id_estado = est.id_estado
        WHERE (@estadoId IS NULL OR c.id_estado = @estadoId)
          AND (@criterio IS NULL OR c.motivo LIKE CONCAT('%', @criterio, '%') 
                                 OR u_pac.nombres LIKE CONCAT('%', @criterio, '%')
                                 OR u_pac.apellidos LIKE CONCAT('%', @criterio, '%'))
        ORDER BY c.fecha_hora DESC;
    """;

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@estadoId", (object?)estadoId ?? DBNull.Value);
        command.Parameters.AddWithValue("@criterio", string.IsNullOrWhiteSpace(criterio) ? DBNull.Value : criterio.Trim());

        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(MapCita(reader));
        }
        return lista;
    }
}