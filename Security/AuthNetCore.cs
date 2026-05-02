using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ETLService.Security.Model;

namespace ETLService.Security
{
    public class AuthNetCore
    {
        private readonly DbService _dbService;

        public AuthNetCore(DbService dbService)
        {
            _dbService = dbService;
        }

        /// <summary>
        /// Autentica un usuario validando su token contra la BD.
        /// Retorna true si el token es válido y no ha expirado.
        /// </summary>
        internal bool Authenticate(string? sessionKey)
        {
            if (string.IsNullOrEmpty(sessionKey)) return false;
            return _dbService.GetUserByToken(sessionKey) != null;
        }

        /// <summary>
        /// Cierra la sesión eliminando el token del usuario en la BD.
        /// </summary>
        internal object ClearSeason(string? sessionKey)
        {
            if (!string.IsNullOrEmpty(sessionKey))
            {
                _dbService.ClearUserToken(sessionKey);
            }
            return true;
        }

        /// <summary>
        /// Inicia sesión: valida credenciales contra la BD, genera un token,
        /// lo guarda en la BD con fecha de expiración y lo retorna al cliente.
        /// </summary>
        internal object? Login(UserModel inst, string? sessionKey)
        {
            // Buscar usuario por credenciales en la BD
            int? userId = _dbService.GetUserByCredentials(inst.username, inst.password);
            if (userId == null)
            {
                return null; // Credenciales inválidas
            }

            // Generar un nuevo token
            string newToken = Guid.NewGuid().ToString();

            // Guardar el token en la BD con expiración de 40 minutos
            DateTime expiration = DateTime.Now.AddMinutes(40);
            _dbService.SaveUserToken(userId.Value, newToken, expiration);

            // Obtener datos del usuario para la respuesta
            var user = _dbService.GetUserById(userId.Value);

            // Obtener los permisos del usuario desde la BD
            var permissions = _dbService.GetUserPermissions(userId.Value);

            return new { SessionKey = newToken, User = user, Permissions = permissions };
        }

        /// <summary>
        /// Simula recuperación de contraseña verificando que el usuario exista en la BD.
        /// </summary>
        internal object RecoveryPassword(string username)
        {
            bool exists = _dbService.UserExists(username);
            if (!exists)
            {
                return false; // Usuario no encontrado
            }
            // En un caso real, se enviaría un correo con un enlace de recuperación.
            return true;
        }

        /// <summary>
        /// Verifica si un usuario tiene al menos uno de los permisos requeridos.
        /// Usa los permisos ya cargados en HttpContext.Items por el middleware,
        /// o consulta la BD directamente si no están disponibles.
        /// </summary>
        internal bool HavePermission(string? token, Permissions[] permissionsList, List<string>? cachedPermissions = null)
        {
            if (string.IsNullOrEmpty(token)) return false;

            List<string> userPermissions;

            if (cachedPermissions != null)
            {
                // Usar permisos ya cargados por el middleware
                userPermissions = cachedPermissions;
            }
            else
            {
                // Fallback: consultar directamente la BD
                int? userId = _dbService.GetUserByToken(token);
                if (userId == null) return false;
                userPermissions = _dbService.GetUserPermissions(userId.Value);
            }

            // Verificar si el usuario tiene al menos uno de los permisos requeridos
            return permissionsList.Any(rp => userPermissions.Contains(rp.ToString()));
        }

        /// <summary>
        /// Obtiene la información del usuario asociado al token.
        /// </summary>
        internal object? User(string? token)
        {
            if (string.IsNullOrEmpty(token)) return null;

            int? userId = _dbService.GetUserByToken(token);
            if (userId == null) return null;

            return _dbService.GetUserById(userId.Value);
        }

        /// <summary>
        /// Método público de conveniencia para autenticación.
        /// </summary>
        public bool Auth(string? sessionKey)
        {
            return Authenticate(sessionKey);
        }
    }
}
