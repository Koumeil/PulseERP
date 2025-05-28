using PulseERP.Shared.Dtos.Auth.Token;
using PulseERP.Shared.Dtos.Users;

namespace PulseERP.Shared.Dtos.Auth;

public record AuthResponse(
    UserInfo User,
    AccessTokenDto accessTokenDto,
    RefreshTokenDto RefreshTokenDto
);
