using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class UserList
    {
        public string id { get; set; }
        public List<User> Users { get; set; }

        public UserList()
        {
            id = "";
            Users = new List<User>();
        }

        public UserList(string id, List<User> users)
        {
            this.id = id;
            Users = users;
        }
    }
}