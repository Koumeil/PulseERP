using PulseERP.Application.Dtos.Token;
using PulseERP.Application.Dtos.User;

namespace PulseERP.Application.Dtos.Auth;

public record AuthResponse(UserInfo User, AccessTokenDto AccessToken, RefreshTokenDto RefreshToken);
