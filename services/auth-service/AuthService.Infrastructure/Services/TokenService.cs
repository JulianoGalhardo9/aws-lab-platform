using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using AuthService.Application.Interfaces;
using AuthService.Domain.Entities;

namespace AuthService.Infrastructure.Services;

public class TokenService : ITokenService
{
    private readonly RsaSecurityKey _privateKey;

    public TokenService()
    {
        var privateKeyPath = Path.Combine(AppContext.BaseDirectory, "Certificates", "private_key.pem");
        
        if (!File.Exists(privateKeyPath))
            throw new FileNotFoundException($"Chave privada não encontrada no caminho: {privateKeyPath}");

        var rsa = RSA.Create();
        var pemContent = File.ReadAllText(privateKeyPath);
        rsa.ImportFromPem(pemContent);

        _privateKey = new RsaSecurityKey(rsa);
    }

    public string GenerateAccessToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var signingCredentials = new SigningCredentials(_privateKey, SecurityAlgorithms.RsaSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(15),
            Issuer = "auth-service",
            Audience = "aws-lab-platform",
            SigningCredentials = signingCredentials
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        
        return Convert.ToBase64String(randomNumber);
    }
}