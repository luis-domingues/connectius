using FluentResults;

namespace Connectius.Application.Services.Authentication;

public interface IAuthenticationService
{
    AuthenticationResult Login(string email, string password);
    Result<AuthenticationResult> Register(string displayName, string username, string email, string password);
}