using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ETLService.Security.Model;

namespace ETLService.Security
{
    public class AuthNetCore
    {
        // Mock data de ejemplo
        private static readonly List<UserModel> _users = new List<UserModel>
        {
            new UserModel { username = "testuser", password = "testpassword", Permissions = { Permissions.CanViewUsers, Permissions.CanViewReports } },
            new UserModel { username = "admin", password = "admin", Permissions = { Permissions.CanViewUsers, Permissions.CanEditUsers, Permissions.CanDeleteUsers, Permissions.CanViewReports, Permissions.CanGenerateReports } }
        };

        // Mock de sesiones activas
        private static readonly Dictionary<string, UserModel> _activeSessions = new Dictionary<string, UserModel>();

        internal static bool Authenticate(string? sessionKey)
        {
            return !string.IsNullOrEmpty(sessionKey) && _activeSessions.ContainsKey(sessionKey);
        }

        internal static object ClearSeason(string? sessionKey)
        {
            if (!string.IsNullOrEmpty(sessionKey) && _activeSessions.ContainsKey(sessionKey))
            {
                _activeSessions.Remove(sessionKey);
            }
            return true;
        }

        internal static object Login(UserModel inst, string? sessionKey)
        {
            var user = _users.FirstOrDefault(u => u.username == inst.username && u.password == inst.password);
            if (user == null)
            {
                return null; // Credenciales inválidas
            }

            // Generar una nueva sessionKey simple para el ejemplo
            string newSessionKey = Guid.NewGuid().ToString();
            _activeSessions[newSessionKey] = user;
            return new { SessionKey = newSessionKey, User = user };
        }

        internal static object RecoveryPassword(string username)
        {
            // Simulación de recuperación de contraseña (en un escenario real, esto enviaría un correo, etc.)
            var user = _users.FirstOrDefault(u => u.username == username);
            if (user == null)
            {
                return false; // Usuario no encontrado
            }
            // En un caso real, se enviaría un correo con un enlace de recuperación o una contraseña temporal.
            return true; // Simula que la recuperación fue exitosa
        }

        public static bool Auth(string? sessionKey)
        {
            return Authenticate(sessionKey);
        }

        internal static bool HavePermission(string? token, Permissions[] permissionsList)
        {
            if (string.IsNullOrEmpty(token) || !_activeSessions.TryGetValue(token, out var user))
            {
                return false; // No hay sesión o token inválido
            }

            // Verificar si el usuario tiene al menos uno de los permisos requeridos
            return permissionsList.Any(rp => user.Permissions.Contains(rp));
        }

        internal static object User(string? token)
        {
            if (_activeSessions.TryGetValue(token, out var user))
            {
                // Aquí podrías devolver un objeto más detallado del usuario si fuera necesario
                // Para este ejemplo, devolvemos el mismo UserModel o una copia sin la contraseña
                return new UserModel { username = user.username };
            }
            return null; // Sesión no encontrada
        }
    }
}


