using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class TilerUserLogin: IdentityUserLogin<string>
    {
        [Key]
        public override string UserId
        {
            get
            {
                return base.UserId;
            }

            set
            {
                base.UserId = value;
            }
        }
        [ForeignKey("UserId")]
        public virtual TilerUser User { get; set; }
    }
}
