using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Security.Claims;
using Microsoft.AspNet.Identity;

namespace TilerElements
{
    public class TilerUserGroup 
    {
        protected string _Id = Guid.NewGuid().ToString();
        protected ICollection<TilerUser> _Users;

        public TilerUserGroup()
        {

        }

        public TilerUserGroup(IEnumerable<TilerUser> users):this()
        {
            _Users = users.ToList();
        }

        virtual public string getId
        {
            get
            {
                return _Id;
            }
        }

        virtual public ICollection<TilerUser> getUsers
        {
            get
            {
                return _Users;
            }
        }
    }
}
