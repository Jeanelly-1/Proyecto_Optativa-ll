using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ETLService.Security.Model;

namespace ETLService.Security
{
    /// <summary>
    /// Middleware de autenticación que intercepta cada request.
    /// Valida el token del usuario contra la BD antes de que llegue al controlador.
    /// Las rutas públicas (Login, RecoveryPassword, Swagger) pasan sin autenticación.
    /// </summary>
    public class AuthMiddleware
    {
        private readonly RequestDelegate _next;

        // Rutas que no requieren autenticación
        private static readonly string[] _publicPaths = new[]
        {
            "/api/security/login",
            "/api/security/recoverypassword",
            "/swagger",
            "/favicon.ico"
        };

        public AuthMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, DbService dbService)
        {
            string path = context.Request.Path.Value?.ToLower() ?? "";

            // Permitir rutas públicas sin autenticación
            if (_publicPaths.Any(p => path.StartsWith(p)))
            {
                await _next(context);
                return;
            }

            // Intentar obtener el token del header Authorization (Bearer token)
            string? token = null;
            string? authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }

            // Si no hay token en el header, intentar obtenerlo de la sesión
            if (string.IsNullOrEmpty(token))
            {
                token = context.Session.GetString("sessionKey");
            }

            // Si no hay token en ningún lado, rechazar
            if (string.IsNullOrEmpty(token))
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"AuthVal\":false,\"Message\":\"Token de autenticación no proporcionado\"}");
                return;
            }

            // Validar el token contra la BD
            int? userId = dbService.GetUserByToken(token);
            if (userId == null)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"AuthVal\":false,\"Message\":\"Token inválido o expirado\"}");
                return;
            }

            // Obtener los permisos del usuario desde la BD
            List<string> permissions = dbService.GetUserPermissions(userId.Value);

            // Inyectar datos del usuario en HttpContext.Items para uso posterior
            context.Items["UserId"] = userId.Value;
            context.Items["UserToken"] = token;
            context.Items["UserPermissions"] = permissions;

            // Continuar al siguiente middleware / controlador
            await _next(context);
        }
    }
}
