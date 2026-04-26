using Microsoft.AspNetCore.Mvc;
using ETLService.Security;
using ETLService.Security.Model;

namespace API.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class SecurityController : ControllerBase
    {
        [HttpPost]
        public object Login(UserModel Inst)
        {
            HttpContext.Session.SetString("sessionKey", Guid.NewGuid().ToString());
            return AuthNetCore.Login(Inst, HttpContext.Session.GetString("sessionKey"));
        }
        [HttpPost]
        public object LogOut()
        {
            return AuthNetCore.ClearSeason(HttpContext.Session.GetString("sessionKey"));
        }
        [HttpPost]
        public object RecoveryPassword(UserModel Inst)
        {
            return AuthNetCore.RecoveryPassword(Inst.username);
        }
            

    }
}
