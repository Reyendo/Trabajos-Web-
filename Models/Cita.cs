using System.ComponentModel.DataAnnotations;

namespace Practica02MediWebMVC.Models;

public class Cita
{
    public int CitaId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un paciente.")]
    [Display(Name = "Paciente")]
    public int PacienteId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un médico.")]
    [Display(Name = "Médico asignado")]
    public int MedicoId { get; set; }

    [Required(ErrorMessage = "Debe seleccionar un consultorio.")]
    [Display(Name = "Consultorio")]
    public int ConsultorioId { get; set; }

    [Required(ErrorMessage = "Debe asignar un estado inicial.")]
    [Display(Name = "Estado de la cita")]
    public int EstadoId { get; set; }

    [Required(ErrorMessage = "La fecha y hora son obligatorias.")]
    [Display(Name = "Fecha y Hora")]
    public DateTime FechaHora { get; set; }

    [Required(ErrorMessage = "El motivo de la consulta es obligatorio.")]
    [StringLength(255)]
    public string Motivo { get; set; } = "";

    [Display(Name = "Notas Médicas")]
    public string? NotasMedicas { get; set; }

    // Propiedades auxiliares (Equivalentes a EspecieNombre, RazaNombre, etc.).
    public string? PacienteNombre { get; set; }
    public string? MedicoNombre { get; set; }
    public string? ConsultorioNombre { get; set; }
    public string? EstadoNombre { get; set; }
}