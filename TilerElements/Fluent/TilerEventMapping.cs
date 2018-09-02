using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements.Fluent
{
    public class TilerEventMapping : EntityTypeConfiguration<TilerEvent>
    {
        public TilerEventMapping()
        {
            //this.HasRequired(tilerEvent => tilerEvent.Name).WithRequiredPrincipal(name => name.AssociatedEvent);
            //this.HasRequired(tilerEvent => tilerEvent.ProfileOfNow_EventDB).WithRequiredPrincipal(nowProfile => nowProfile.AssociatedEvent);
        }

    }
}
