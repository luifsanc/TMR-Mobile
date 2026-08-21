namespace tmr_shared.DTOs.Auth;

public record UserResponse(
    int Id,
    string Email,
    string Name,
    DateTime CreatedAt,
    int? IdEmpleado,
    bool DebeCambiarPassword = false);
