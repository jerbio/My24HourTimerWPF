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
    public class GoogleTilerUser : ThirdPartyTilerUser
    {
        protected GoogleTilerUser()
        {

        }

        public GoogleTilerUser(string id)
        {
            Id = id;
            CalendarType = ThirdPartyControl.CalendarTool.google.ToString();
        }
    }
}