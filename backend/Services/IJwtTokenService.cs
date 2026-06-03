using UptimeLab.Api.Models;

namespace UptimeLab.Api.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}
