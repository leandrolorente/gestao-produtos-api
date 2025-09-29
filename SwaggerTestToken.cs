using System;
using System.Text;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

class SwaggerTestToken
{
    static void Main()
    {
        // Dados de exemplo para o token
        var secretKey = "minha-chave-secreta-super-segura-para-jwt-token-com-256-bits";
        var issuer = "gestao-produtos-api";
        var audience = "gestao-produtos-client";
        
        // Claims do usuário
        var claims = new[]
        {
            new Claim("id", "507f1f77bcf86cd799439011"),
            new Claim("name", "Admin Test"),
            new Claim("email", "admin@test.com"),
            new Claim("role", "admin"),
            new Claim("department", "IT")
        };

        // Criação do token
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.Now.AddHours(24),
            signingCredentials: creds
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        
        Console.WriteLine("=== TOKEN JWT PARA SWAGGER ===");
        Console.WriteLine();
        Console.WriteLine("Bearer " + tokenString);
        Console.WriteLine();
        Console.WriteLine("=== INSTRUÇÕES ===");
        Console.WriteLine("1. Copie o token acima (incluindo 'Bearer ')");
        Console.WriteLine("2. No Swagger, clique no botão 'Authorize' 🔒");
        Console.WriteLine("3. Cole o token no campo 'Value'");
        Console.WriteLine("4. Clique em 'Authorize'");
        Console.WriteLine("5. Agora você pode testar os endpoints protegidos!");
        Console.WriteLine();
        Console.WriteLine("Token válido por 24 horas");
    }
}