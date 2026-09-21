using System.ComponentModel.DataAnnotations;

namespace MediWeb.Api.DTOs;

public class CitaCreateDto
{
    [Required(ErrorMessage = "El paciente es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Identificador de paciente no válido.")]
    public int PacienteId { get; set; }

    [Required(ErrorMessage = "El médico asignado es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Identificador de médico no válido.")]
    public int MedicoId { get; set; }

    [Required(ErrorMessage = "El consultorio es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Identificador de consultorio no válido.")]
    public int ConsultorioId { get; set; }

    [Required(ErrorMessage = "El estado de la cita es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "Identificador de estado no válido.")]
    public int EstadoId { get; set; }

    [Required(ErrorMessage = "La fecha y hora de la cita son obligatorias.")]
    public DateTime FechaHora { get; set; }

    [Required(ErrorMessage = "El motivo de la consulta es obligatorio.")]
    [StringLength(255, ErrorMessage = "El motivo no puede exceder los 255 caracteres.")]
    public string Motivo { get; set; } = "";

    public string? NotasMedicas { get; set; }
}