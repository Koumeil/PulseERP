using PulseERP.Domain.Dtos.Auth.Token;
using PulseERP.Domain.Dtos.Users;

namespace PulseERP.Domain.Dtos.Auth;

public record AuthResponse(UserInfo User, AccessTokenDto AccessToken, RefreshTokenDto RefreshToken);
