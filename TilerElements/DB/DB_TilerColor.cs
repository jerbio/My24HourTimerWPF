using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
using System.ComponentModel.DataAnnotations.Schema;

namespace TilerElements.DB
{
    public class DB_TilerColor:TilerColor
    {
        public static DB_TilerColor ConvertToPersistable(TilerColor nowProfile)
        {
            DB_TilerColor retValue = new DB_TilerColor()
            {
                ID = nowProfile.Id,
                ColorSelection = nowProfile.UserColorSelection,
                G = nowProfile.G,
                B = nowProfile.B,
                R = nowProfile.R,
                O = nowProfile.O
            };
            retValue.Id = nowProfile.Id;
            return retValue;
        }
    }
}
