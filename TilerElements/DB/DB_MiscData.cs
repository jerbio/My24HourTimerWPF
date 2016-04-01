using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;
namespace TilerElements.DB
{
    public class DB_MiscData:MiscData
    {


        #region property

        public string NoteData
        {
            get
            {
                return UserTypedData;
            }
            set
            {
                UserTypedData = value;
            }
        }


        public int SourceOfdata
        {
            get
            {
                return Type;
            }
            set
            {
                Type = value;
            }
        }

        #endregion
    }
}
