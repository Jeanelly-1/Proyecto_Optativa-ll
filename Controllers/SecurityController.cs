using Microsoft.AspNetCore.Mvc;
using ETLService.Security;
using ETLService.Security.Model;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        private readonly AuthNetCore _authNetCore;

        public SecurityController(AuthNetCore authNetCore)
        {
            _authNetCore = authNetCore;
        }

        [HttpPost]
        public object Login(UserModel Inst)
        {
            var result = _authNetCore.Login(Inst, null);
            if (result == null)
            {
                return Unauthorized(new { AuthVal = false, Message = "Credenciales inválidas" });
            }

            // Almacenar el token en la sesión HTTP también (compatibilidad)
            var token = result.GetType().GetProperty("SessionKey")?.GetValue(result)?.ToString();
            if (!string.IsNullOrEmpty(token))
            {
                HttpContext.Session.SetString("sessionKey", token);
            }

            return result;
        }

        [HttpPost]
        public object LogOut()
        {
            // Obtener el token del header o de la sesión
            string? token = null;
            string? authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
            if (string.IsNullOrEmpty(token))
            {
                token = HttpContext.Session.GetString("sessionKey");
            }

            var result = _authNetCore.ClearSeason(token);

            // Limpiar la sesión HTTP
            HttpContext.Session.Remove("sessionKey");

            return result;
        }

        [HttpPost]
        public object RecoveryPassword(UserModel Inst)
        {
            return _authNetCore.RecoveryPassword(Inst.username);
        }
    }
}
