using MySqlConnector;
using Practica02MediWebMVC.Models;

namespace Practica02MediWebMVC.Data;

public class CitaRepository
{
    private readonly string _connectionString;

    public CitaRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("MediWebConnection")
            ?? throw new InvalidOperationException("No se encontró la cadena MediWebConnection.");
    }

    public async Task<List<Cita>> GetAllAsync()
    {
        // JOIN para obtener nombres en lugar de IDs, igual que con las especies y razas.
        const string sql = @"
            SELECT c.id_cita, c.id_paciente,
                   u_pac.nombres AS paciente_nombres, u_pac.apellidos AS paciente_apellidos,
                   c.id_medico, u_med.nombres AS medico_nombres, u_med.apellidos AS medico_apellidos,
                   c.id_consultorio, cons.nombre AS consultorio_nombre,
                   c.id_estado, est.nombre AS estado_nombre,
                   c.fecha_hora, c.motivo, c.notas_medicas
            FROM Cita c
            INNER JOIN Paciente p ON c.id_paciente = p.id_paciente
            INNER JOIN Usuario u_pac ON p.id_usuario = u_pac.id_usuario
            INNER JOIN Medico m ON c.id_medico = m.id_medico
            INNER JOIN Usuario u_med ON m.id_usuario = u_med.id_usuario
            INNER JOIN Consultorio cons ON c.id_consultorio = cons.id_consultorio
            INNER JOIN Estado_Cita est ON c.id_estado = est.id_estado
            ORDER BY c.fecha_hora DESC;";

        var lista = new List<Cita>();
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

    public async Task<Cita?> GetByIdAsync(int id)
    {
        const string sql = @"
            SELECT c.id_cita, c.id_paciente,
                   u_pac.nombres AS paciente_nombres, u_pac.apellidos AS paciente_apellidos,
                   c.id_medico, u_med.nombres AS medico_nombres, u_med.apellidos AS medico_apellidos,
                   c.id_consultorio, cons.nombre AS consultorio_nombre,
                   c.id_estado, est.nombre AS estado_nombre,
                   c.fecha_hora, c.motivo, c.notas_medicas
            FROM Cita c
            INNER JOIN Paciente p ON c.id_paciente = p.id_paciente
            INNER JOIN Usuario u_pac ON p.id_usuario = u_pac.id_usuario
            INNER JOIN Medico m ON c.id_medico = m.id_medico
            INNER JOIN Usuario u_med ON m.id_usuario = u_med.id_usuario
            INNER JOIN Consultorio cons ON c.id_consultorio = cons.id_consultorio
            INNER JOIN Estado_Cita est ON c.id_estado = est.id_estado
            WHERE c.id_cita = @id LIMIT 1;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        command.Parameters.AddWithValue("@id", id);
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? MapCita(reader) : null;
    }

    public async Task<int> CreateAsync(Cita cita)
    {
        const string sql = @"
            INSERT INTO Cita (id_paciente, id_medico, id_consultorio, id_estado, fecha_hora, motivo, notas_medicas)
            VALUES (@paciente, @medico, @consultorio, @estado, @fechaHora, @motivo, @notas);";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, cita);
        await command.ExecuteNonQueryAsync();
        return (int)command.LastInsertedId;
    }

    public async Task<bool> UpdateAsync(Cita cita)
    {
        const string sql = @"
            UPDATE Cita 
            SET id_paciente = @paciente, id_medico = @medico, id_consultorio = @consultorio, 
                id_estado = @estado, fecha_hora = @fechaHora, motivo = @motivo, notas_medicas = @notas
            WHERE id_cita = @id;";

        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        AddParameters(command, cita);
        command.Parameters.AddWithValue("@id", cita.CitaId);
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

    // Métodos para catálogos.
    public async Task<List<CatalogoItem>> GetPacientesAsync()
        => await GetCatalogAsync("SELECT p.id_paciente AS id, CONCAT(u.nombres, ' ', u.apellidos) AS nombre FROM Paciente p INNER JOIN Usuario u ON p.id_usuario = u.id_usuario;");

    public async Task<List<CatalogoItem>> GetMedicosAsync()
        => await GetCatalogAsync("SELECT m.id_medico AS id, CONCAT('Dr. ', u.nombres, ' ', u.apellidos) AS nombre FROM Medico m INNER JOIN Usuario u ON m.id_usuario = u.id_usuario;");

    public async Task<List<CatalogoItem>> GetConsultoriosAsync()
        => await GetCatalogAsync("SELECT id_consultorio AS id, nombre FROM Consultorio;");

    public async Task<List<CatalogoItem>> GetEstadosAsync()
        => await GetCatalogAsync("SELECT id_estado AS id, nombre FROM Estado_Cita;");

    private async Task<List<CatalogoItem>> GetCatalogAsync(string sql)
    {
        var lista = new List<CatalogoItem>();
        await using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();
        await using var command = new MySqlCommand(sql, connection);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            lista.Add(new CatalogoItem { Id = reader.GetInt32("id"), Nombre = reader.GetString("nombre") });
        }
        return lista;
    }

    private static void AddParameters(MySqlCommand command, Cita cita)
    {
        command.Parameters.AddWithValue("@paciente", cita.PacienteId);
        command.Parameters.AddWithValue("@medico", cita.MedicoId);
        command.Parameters.AddWithValue("@consultorio", cita.ConsultorioId);
        command.Parameters.AddWithValue("@estado", cita.EstadoId);
        command.Parameters.AddWithValue("@fechaHora", cita.FechaHora);
        command.Parameters.AddWithValue("@motivo", cita.Motivo.Trim());
        command.Parameters.AddWithValue("@notas", (object?)cita.NotasMedicas ?? DBNull.Value);
    }

    private static Cita MapCita(MySqlDataReader reader)
    {
        return new Cita
        {
            CitaId = reader.GetInt32("id_cita"),
            PacienteId = reader.GetInt32("id_paciente"),
            PacienteNombre = $"{reader.GetString("paciente_nombres")} {reader.GetString("paciente_apellidos")}",
            MedicoId = reader.GetInt32("id_medico"),
            MedicoNombre = $"{reader.GetString("medico_nombres")} {reader.GetString("medico_apellidos")}",
            ConsultorioId = reader.GetInt32("id_consultorio"),
            ConsultorioNombre = reader.GetString("consultorio_nombre"),
            EstadoId = reader.GetInt32("id_estado"),
            EstadoNombre = reader.GetString("estado_nombre"),
            FechaHora = reader.GetDateTime("fecha_hora"),
            Motivo = reader.GetString("motivo"),
            NotasMedicas = reader.IsDBNull(reader.GetOrdinal("notas_medicas")) ? null : reader.GetString("notas_medicas")
        };
    }
}