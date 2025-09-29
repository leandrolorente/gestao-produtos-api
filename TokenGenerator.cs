using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var jwtSecret = "MinhaChaveSecretaSuperSeguraParaJWT2024!@#$%^&*()_+";
var issuer = "GestaoProdutosAPI";
var audience = "GestaoProdutosApp";
var expirationHours = 24;

var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));
var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

var claims = new[]
{
    new Claim(ClaimTypes.NameIdentifier, "507f1f77bcf86cd799439011"),
    new Claim(ClaimTypes.Name, "Admin Test"),
    new Claim(ClaimTypes.Email, "admin@test.com"),
    new Claim(ClaimTypes.Role, "Admin"),
    new Claim("Department", "IT")
};

var token = new JwtSecurityToken(
    issuer: issuer,
    audience: audience,
    claims: claims,
    expires: DateTime.UtcNow.AddHours(expirationHours),
    signingCredentials: creds
);

var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

Console.WriteLine("=== TOKEN JWT VÁLIDO PARA SWAGGER ===");
Console.WriteLine();
Console.WriteLine($"Bearer {tokenString}");
Console.WriteLine();
Console.WriteLine("=== COPIE ESTE TOKEN COMPLETO ===");
Console.WriteLine($"Bearer {tokenString}");
Console.WriteLine();
Console.WriteLine("Token válido até: " + DateTime.UtcNow.AddHours(expirationHours).ToString("dd/MM/yyyy HH:mm:ss UTC"));