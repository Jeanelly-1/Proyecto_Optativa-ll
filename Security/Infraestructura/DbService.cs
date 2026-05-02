using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Data.SqlClient;
using ETLService.Security.Model;

namespace ETLService.Security
{
    /// <summary>
    /// Servicio de acceso a la base de datos para operaciones de seguridad.
    /// Usa ADO.NET (Microsoft.Data.SqlClient) para consultar las tablas de seguridad.
    /// </summary>
    public class DbService
    {
        private readonly string _connectionString;

        public DbService(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Genera el hash SHA256 de una contraseña en formato hexadecimal (mayúsculas).
        /// Debe coincidir con UPPER(CONVERT(NVARCHAR(255), HASHBYTES('SHA2_256', N'...'), 2)) de SQL Server.
        /// </summary>
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            byte[] bytes = sha256.ComputeHash(Encoding.Unicode.GetBytes(password));
            var sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Busca un usuario por nombre y contraseña (la contraseña se hashea antes de comparar).
        /// Retorna el Id_User si es válido, o null si las credenciales son inválidas.
        /// </summary>
        public int? GetUserByCredentials(string username, string password)
        {
            string hashedPassword = HashPassword(password);

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT Id_User 
                FROM Security_Users 
                WHERE Nombres = @Username 
                  AND Password = @Password 
                  AND Estado = 'Activo'";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);
            command.Parameters.AddWithValue("@Password", hashedPassword);

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : null;
        }

        /// <summary>
        /// Busca un usuario por su token activo (no expirado).
        /// Retorna el Id_User si el token es válido, o null si no existe o expiró.
        /// </summary>
        public int? GetUserByToken(string token)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT Id_User 
                FROM Security_Users 
                WHERE Token = @Token 
                  AND Token_Expiration_Date > GETDATE()
                  AND Estado = 'Activo'";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Token", token);

            var result = command.ExecuteScalar();
            return result != null ? Convert.ToInt32(result) : null;
        }

        /// <summary>
        /// Obtiene la información básica del usuario por su Id.
        /// </summary>
        public UserModel? GetUserById(int userId)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT Nombres, Mail
                FROM Security_Users 
                WHERE Id_User = @UserId AND Estado = 'Activo'";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new UserModel
                {
                    username = reader["Nombres"]?.ToString() ?? ""
                };
            }
            return null;
        }

        /// <summary>
        /// Obtiene la lista de permisos (como strings) del usuario, consultando:
        /// Security_Users_Roles -> Security_Permissions_Roles -> Security_Permissions
        /// </summary>
        public List<string> GetUserPermissions(int userId)
        {
            var permissions = new List<string>();

            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT DISTINCT sp.Descripcion
                FROM Security_Users_Roles sur
                INNER JOIN Security_Permissions_Roles spr ON sur.Id_Role = spr.Id_Role
                INNER JOIN Security_Permissions sp ON spr.Id_Permission = sp.Id_Permission
                WHERE sur.Id_User = @UserId
                  AND sur.Estado = 'Activo'
                  AND spr.Estado = 'Activo'
                  AND sp.Estado = 'Activo'";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@UserId", userId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var desc = reader["Descripcion"]?.ToString();
                if (!string.IsNullOrEmpty(desc))
                {
                    permissions.Add(desc);
                }
            }

            return permissions;
        }

        /// <summary>
        /// Guarda el token de sesión y su fecha de expiración en la tabla Security_Users.
        /// </summary>
        public void SaveUserToken(int userId, string token, DateTime expiration)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Security_Users 
                SET Token = @Token, 
                    Token_Date = GETDATE(), 
                    Token_Expiration_Date = @Expiration
                WHERE Id_User = @UserId";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Token", token);
            command.Parameters.AddWithValue("@Expiration", expiration);
            command.Parameters.AddWithValue("@UserId", userId);

            command.ExecuteNonQuery();
        }

        /// <summary>
        /// Limpia el token de un usuario (logout). Pone Token en NULL.
        /// </summary>
        public bool ClearUserToken(string token)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                UPDATE Security_Users 
                SET Token = NULL, 
                    Token_Date = NULL, 
                    Token_Expiration_Date = NULL
                WHERE Token = @Token";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Token", token);

            return command.ExecuteNonQuery() > 0;
        }

        /// <summary>
        /// Verifica si un usuario existe por su nombre (para recuperación de contraseña).
        /// </summary>
        public bool UserExists(string username)
        {
            using var connection = new SqlConnection(_connectionString);
            connection.Open();

            string query = @"
                SELECT COUNT(1) 
                FROM Security_Users 
                WHERE Nombres = @Username AND Estado = 'Activo'";

            using var command = new SqlCommand(query, connection);
            command.Parameters.AddWithValue("@Username", username);

            return Convert.ToInt32(command.ExecuteScalar()) > 0;
        }
    }
}
