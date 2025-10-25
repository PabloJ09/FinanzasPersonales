using FinanzasPersonales.Database;
using FinanzasPersonales.Models;
using MongoDB.Driver;

namespace FinanzasPersonales.Services
{
    public class UsuarioService
    {
        private readonly IMongoCollection<Usuario> _usuarios;

        public UsuarioService(MongoDBContext context)
        {
            _usuarios = context.Usuarios;
        }

        // Crear usuario (con hash seguro)
        public async Task CrearUsuarioAsync(Usuario usuario)
        {
            var existe = await _usuarios.Find(u => u.Email == usuario.Email).FirstOrDefaultAsync();
            if (existe != null)
                throw new Exception("El correo ya está registrado.");

            // 🔐 Hashear la contraseña usando el método de la clase Usuario
            usuario.SetPassword(usuario.PasswordHash);

            await _usuarios.InsertOneAsync(usuario);
        }


        // Obtener usuario por correo
        public async Task<Usuario?> ObtenerPorEmailAsync(string email) =>
            await _usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();

        // Validar login
        public async Task<bool> ValidarCredencialesAsync(string email, string password)
        {
            var usuario = await ObtenerPorEmailAsync(email);
            if (usuario == null) return false;

            // Usamos el método de Usuario que compara hash
            return usuario.VerifyPassword(password);
        }

    }
}

