using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TilerElements.Wpf;


namespace TilerElements.DB
{
    public class DB_Classification:Classification
    {

        public static DB_Classification ConvertToPersistable(Classification classification)
        {
            DB_Classification retValue = new DB_Classification()
            {
                Id = classification.Id,
                LeisureType = classification.LeisureType,
                Placement = classification.Placement,
                Succubus = classification.Succubus,
            };
            return retValue;
        }
    }
}
