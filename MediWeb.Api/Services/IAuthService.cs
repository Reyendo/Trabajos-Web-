using MediWeb.Api.DTOs;

namespace MediWeb.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
}