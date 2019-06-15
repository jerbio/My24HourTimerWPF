using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements;
using TilerFront;
using TilerFront.Models;

namespace TilerTests
{
    public class TestAuthorizedUser: AuthorizedUser
    {
        async override public Task<UserAccount> getUserAccount(TilerDbContext db)
        {
            TilerUser User = new TilerUser() { UserName = UserName, Id = UserID };
            return new UserAccountTest(User);
        }
    }
}
