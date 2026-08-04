namespace tmr_shared.DTOs.Auth;

public record AuthResponse(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    Guid TokenFamilyId,
    UserResponse User
);
