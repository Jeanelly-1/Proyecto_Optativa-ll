using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ETLService.Security.Model
{
    public class UserModel
    {
        public string username { get; set; }
        public string password { get; set; }
        public List<Permissions> Permissions { get; set; } = new List<Permissions>();
    }
}