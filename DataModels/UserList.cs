using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AikidoLive.DataModels
{
    public class UserList
    {
        public string Id { get; set; }
        public List<User> Users { get; set; }

        UserList()
        {
            Id = "";
            Users = new List<User>();
        }

        UserList(string id, List<User> users)
        {
            Id = id;
            Users = users;
        }
    }
}