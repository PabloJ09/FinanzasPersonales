using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace FinanzasPersonales.Services
{
    public class UsuarioService
    {
        private readonly IMongoCollection<Usuario> _usuarios;
        private readonly IConfiguration _config;

        public UsuarioService(IMongoDBContext context, IConfiguration config)
        {
            _usuarios = context.Usuarios;
            _config = config;
        }

        // Registro: guarda password en formato salt:hash (ambos Base64)
    public async Task<Usuario> RegisterAsync(string username, string password, string role = "usuario")
        {
            // Normalizar username
            username = username?.Trim().ToLowerInvariant() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("El nombre de usuario no puede estar vacío", nameof(username));

            // Buscar usuario con FindAsync (más testeable que la extensión FirstOrDefaultAsync)
            var cursorExists = await _usuarios.FindAsync(u => u.Username == username);
            await cursorExists.MoveNextAsync();
            var exists = cursorExists.Current.FirstOrDefault();
            if (exists != null) throw new InvalidOperationException("Usuario ya existe");

            var pw = HashPassword(password);
            var user = new Usuario
            {
                Username = username,
                PasswordHash = pw,
                IsActive = false,
                Role = role
            };

            await _usuarios.InsertOneAsync(user);
            return user;
        }

        // Login: valida contraseña y estado, retorna token
        public async Task<string> LoginAsync(string username, string password)
        {
            username = username.Trim().ToLowerInvariant();
            var cursor = await _usuarios.FindAsync(u => u.Username == username);
            await cursor.MoveNextAsync();
            var user = cursor.Current.FirstOrDefault();
            if (user == null) throw new UnauthorizedAccessException("Credenciales inválidas");
            if (!user.IsActive) throw new UnauthorizedAccessException("Usuario no activado");

            if (!VerifyPassword(password, user.PasswordHash))
                throw new UnauthorizedAccessException("Credenciales inválidas");

            return GenerateJwtToken(user);
        }

        public async Task<bool> IsActiveAsync(string username)
        {
            username = username.Trim().ToLowerInvariant();
            var cursor = await _usuarios.FindAsync(u => u.Username == username);
            await cursor.MoveNextAsync();
            var user = cursor.Current.FirstOrDefault();
            return user?.IsActive ?? false;
        }

    // Utilities
    internal string HashPassword(string password)
        {
            // PBKDF2
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);
            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = derive.GetBytes(32);
            return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
        }

    internal bool VerifyPassword(string password, string stored)
        {
            var parts = stored.Split(':');
            if (parts.Length != 2) return false;
            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);
            using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var test = derive.GetBytes(32);
            return CryptographicOperations.FixedTimeEquals(test, hash);
        }

        private string GenerateJwtToken(Usuario user)
        {
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key no configurada");
            var issuer = _config["Jwt:Issuer"] ?? string.Empty;
            var audience = _config["Jwt:Audience"] ?? string.Empty;

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? "Usuario")
            };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
