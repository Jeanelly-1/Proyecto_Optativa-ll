using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ETLService.Security;
using ETLService.Security.Model;
using Microsoft.AspNetCore.Mvc;

namespace ETLService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private static List<User> _users = new List<User>
        {
            new User {  Name = "Maria", Age = 30 },
            new User {  Name = "Bob", Age = 25 }
        };

        [HttpGet]
        [AuthController(Permissions.CanViewUsers)]
        public ActionResult<IEnumerable<User>> GetUsers()
        {
            return Ok(_users);
        }

        [HttpGet("{name}")]
        [AuthController(Permissions.CanViewUsers)]
        public ActionResult<User> GetUserById(string name)
        {
            var user = _users.FirstOrDefault(u => u.Name == name);
            if (user == null)
            {
                return NotFound();
            }
            return Ok(user);
        }

        [HttpPost]
        [AuthController(Permissions.CanEditUsers)]
        public ActionResult<User> CreateUser(User user)
        {
            _users.Add(user);
            return Ok(user);
        }

        [HttpPut]
         [AuthController(Permissions.CanEditUsers)]
        public IActionResult UpdateUser(User user)
        {
            var existingUser = _users.FirstOrDefault(u => user.Name == u.Name);
            if (existingUser == null)
            {
                return NotFound();
            }

            existingUser.Name = user.Name;
            existingUser.Age = user.Age;

            return Ok();
        }

        [HttpDelete]
         [AuthController(Permissions.CanDeleteUsers)]
        public IActionResult DeleteUser(User userParam)
        {
            var user = _users.FirstOrDefault(u => u.Name == userParam.Name);
            if (user == null)
            {
                return NotFound();
            }

            _users.Remove(user);
            return Ok();
        }
    }

    public class User
    {
        public string? Name { get; set; }
        public int? Age { get; set; }

    }

}
