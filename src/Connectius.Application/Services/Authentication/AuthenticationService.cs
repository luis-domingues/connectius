using Connectius.Application.Common.Errors;
using Connectius.Application.Common.Interfaces.Authentication;
using Connectius.Application.Common.Interfaces.Persistence;
using Connectius.Domain.Entities;
using FluentResults;

namespace Connectius.Application.Services.Authentication;

public class AuthenticationService : IAuthenticationService
{
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUserRepository _userRepository;

    public AuthenticationService(IJwtTokenGenerator jwtTokenGenerator, IUserRepository userRepository)
    {
        _jwtTokenGenerator = jwtTokenGenerator;
        _userRepository = userRepository;
    }
    
    public AuthenticationResult Login(string email, string password)
    {
        //validar se o usuário existe
        if (_userRepository.GetUserByEmail(email) is not User user)
        {
            throw new Exception("E-mail já existente");
        }
        
        //validar se a senha está correta
        if (user.Password != password)
        {
            throw new Exception("Senha inválida");
        }
        
        //criar token
        var token = _jwtTokenGenerator.GenerateToken(user);
        
        return new AuthenticationResult(
            user,
            token);
    }

    public Result<AuthenticationResult> Register(
        string displayName, 
        string username, 
        string email, 
        string password)
    {
        // verificar se o usuário existe
        if (_userRepository.GetUserByEmail(email) is not null)
        {
            return Result.Fail<AuthenticationResult>(new[]
            {
                new DuplicateEmailError()
            });
        }
        
        // criar usuário
        var user = new User
        {
            DisplayName = displayName,
            Username = username,
            Email = email,
            Password = password
        };
        
        _userRepository.Add(user);
        
        var token = _jwtTokenGenerator.GenerateToken(user);
        
        return new AuthenticationResult(
            user,
            token);
    }
}
