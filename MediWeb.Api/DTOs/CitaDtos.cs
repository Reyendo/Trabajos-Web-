namespace MediWeb.Api.DTOs;

public class CitaDto
{
    public int CitaId { get; set; }
    public int PacienteId { get; set; }
    public string Paciente { get; set; } = "";
    public int MedicoId { get; set; }
    public string Medico { get; set; } = "";
    public int ConsultorioId { get; set; }
    public string Consultorio { get; set; } = "";
    public int EstadoId { get; set; }
    public string Estado { get; set; } = "";
    public DateTime FechaHora { get; set; }
    public string Motivo { get; set; } = "";
    public string? NotasMedicas { get; set; }
}