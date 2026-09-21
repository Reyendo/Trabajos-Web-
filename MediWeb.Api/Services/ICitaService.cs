using MediWeb.Api.DTOs;

namespace MediWeb.Api.Services;

public interface ICitaService
{
    Task<List<CitaDto>> GetAllAsync();
    Task<CitaDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CitaCreateDto dto);
    Task<bool> UpdateAsync(int id, CitaUpdateDto dto);
    Task<bool> DeleteAsync(int id);
    Task<List<CitaDto>> BuscarAsync(int? estadoId, string? criterio);
}
