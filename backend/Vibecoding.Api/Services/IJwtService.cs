using Vibecoding.Api.Models;

namespace Vibecoding.Api.Services;

public interface IJwtService
{
    string CreateToken(AppUser user);
}
