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


        static public DB_MiscData ConvertToPersistable(MiscData miscData)
        {
            DB_MiscData retValue = new DB_MiscData()
            {
                Id = miscData.Id,
                NoteData = miscData.UserNote,
                Type = miscData.TypeSelection
            };
            return retValue;
        }
        #endregion
    }
}
