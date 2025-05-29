using System;

namespace PulseERP.Domain.Interfaces.Services;

public interface ITokenGenerator
{
    string GenerateToken();
}
