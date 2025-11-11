using FinanzasPersonales.Common.Exceptions;
using FinanzasPersonales.Database;
using FinanzasPersonales.Database.Repositories;
using FinanzasPersonales.Models;
using Microsoft.Extensions.Configuration;
using FinanzasPersonales.Validators;
using FluentValidation;
using System.Security.Cryptography;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace FinanzasPersonales.Services;

/// <summary>
/// Servicio de dominio para Usuarios.
/// Principio: Single Responsibility - Maneja autenticación y usuario
/// Principio: Dependency Inversion - Depende de IRepository
/// </summary>
public interface IUsuarioService
{
    Task<Usuario> RegisterAsync(string username, string password, string role = "usuario");
    Task<string> LoginAsync(string username, string password);
    Task<bool> IsActiveAsync(string username);
}

public class UsuarioService : IUsuarioService
{
    private readonly IRepository<Usuario> _repository;
    private readonly IValidator<Usuario> _validator;
    private readonly IConfiguration _config;

    public UsuarioService(IRepository<Usuario> repository, IValidator<Usuario> validator, IConfiguration config)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }

    // Backwards-compatible constructor for tests/code that pass IMongoDBContext
    internal UsuarioService(IMongoDBContext context, IConfiguration config)
        : this(new MongoRepository<Usuario>(context.Usuarios), new Validators.UsuarioValidator(), config)
    {
    }

    public async Task<Usuario> RegisterAsync(string username, string password, string role = "usuario")
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("username cannot be empty", nameof(username));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("password cannot be empty", nameof(password));

        // Normalizar username
        username = username.Trim().ToLowerInvariant();

        // Verificar que el usuario no exista
        var exists = await _repository.ExistsAsync(u => u.Username == username);
        if (exists)
            throw new InvalidOperationException("El usuario ya existe");

        var hashedPassword = HashPassword(password);
        var user = new Usuario
        {
            Username = username,
            PasswordHash = hashedPassword,
            IsActive = false,
            Role = role
        };

        // Validar la entidad
        var validationResult = await _validator.ValidateAsync(user);
        if (!validationResult.IsValid)
        {
            var message = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new System.ComponentModel.DataAnnotations.ValidationException(message);
        }

        return await _repository.AddAsync(user);
    }

    public async Task<string> LoginAsync(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        // Tests expect UnauthorizedAccessException for empty/invalid credentials
        if (string.IsNullOrWhiteSpace(password))
            throw new UnauthorizedAccessException("Contraseña vacía");
        username = username.Trim().ToLowerInvariant();

        var user = await _repository.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
            throw new UnauthorizedAccessException("Credenciales inválidas");

        if (!user.IsActive)
            throw new UnauthorizedAccessException("Usuario no activado");

        if (!VerifyPassword(password, user.PasswordHash))
            throw new UnauthorizedAccessException("Credenciales inválidas");

        return GenerateJwtToken(user);
    }

    public async Task<bool> IsActiveAsync(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentNullException(nameof(username));

        username = username.Trim().ToLowerInvariant();
        var user = await _repository.FirstOrDefaultAsync(u => u.Username == username);
        return user?.IsActive ?? false;
    }

    internal string HashPassword(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[16];
        rng.GetBytes(salt);
        using var derive = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
        var hash = derive.GetBytes(32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    private bool VerifyPassword(string password, string stored)
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
            new Claim(ClaimTypes.Role, user.Role ?? "usuario")
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
