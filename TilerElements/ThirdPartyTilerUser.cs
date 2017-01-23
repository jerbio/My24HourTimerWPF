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
    abstract public class ThirdPartyTilerUser : TilerUser
    {
        protected ThirdPartyTilerUser()
        {

        }

        public ThirdPartyTilerUser(string id)
        {
            Id = id;
            CalendarType = ThirdPartyControl.CalendarTool.google.ToString();
        }

        public override string Id
        {
            get
            {
                return base.Id;
            }

            set
            {
                _Id = value;
            }
        }
    }
}