using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public class DB_NowProfile : NowProfile
    {

        public DB_NowProfile(NowProfile nowProfileFromCalculation): base(nowProfileFromCalculation.PreferredTime,nowProfileFromCalculation.isInitialized)
        {

        }
        public DB_NowProfile(DateTimeOffset preferredTimeData, bool InitializedData)
            : base(preferredTimeData, InitializedData)
        {
        
        }

        public bool hasBeenSet
        {
            get
            {
                return isInitialized;
            }
            set
            {
                this.Initialized = value;
            }
        }

        public DateTimeOffset BestStartTime
        {
            get
            {
                return PreferredTime;
            }
            set
            {
                this.TimePreferredForEvent = value;
            }
        }

        public static DB_NowProfile ConvertToPersistable(NowProfile nowProfile)
        {
            DB_NowProfile retValue = new DB_NowProfile(nowProfile);
            retValue.Id = nowProfile.Id;
            return retValue;
        }
    }
}