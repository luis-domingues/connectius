using Connectius.Application.Common.Errors;
using Connectius.Application.Services.Authentication;
using Connectius.Contracts.Authentication;
using FluentResults;
using Microsoft.AspNetCore.Mvc;

namespace Connectius.Presentation.Controllers;

[ApiController]
[Route("auth")]
public class AuthenticationController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthenticationController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }
    
    [HttpPost("register")]
    public IActionResult Register(RegisterRequest request)
    {
        Result<AuthenticationResult> registerResult = _authenticationService.Register(
            request.DisplayName, 
            request.Username, 
            request.Email, 
            request.Password);

        if (registerResult.IsSuccess)
        {
            return Ok(MapAuthResult(registerResult.Value));
        }

        var firstError = registerResult.Errors[0];
        
        if (firstError is DuplicateEmailError)
        {
            return Problem(statusCode: StatusCodes.Status409Conflict, detail: "E-mail já existente");
        }

        return Problem();
    }

    private static AuthenticationResponse MapAuthResult(AuthenticationResult authResult)
    {
        return new AuthenticationResponse(
            authResult.user.Id,
            authResult.user.DisplayName,
            authResult.user.Username,
            authResult.user.Email,
            authResult.Token
            );
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequest request)
    {
        var authResult = _authenticationService.Login(
            request.Email, 
            request.Password);

        var response = new AuthenticationResponse(
            authResult.user.Id,
            authResult.user.DisplayName,
            authResult.user.Username,
            authResult.user.Email,
            authResult.Token);
        
        return Ok(response);
    }
}