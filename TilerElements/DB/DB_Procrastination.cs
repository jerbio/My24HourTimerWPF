using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TilerElements.Wpf;

namespace TilerElements.DB
{
    public class DB_Procrastination : Procrastination
    {
        public DB_Procrastination(DateTimeOffset FromTimeData, DateTimeOffset BeginTimeData, int DisLikedSection)
        {
            FromTime = FromTimeData;
            BeginTIme = BeginTimeData;
            SectionOfDay = DisLikedSection;
        }

        override public string Id
        {
            get
            {
                return ID;

            }
            set
            {
                Guid testValue;
                if (Guid.TryParse(value, out testValue))
                {
                    ID = value;
                }
                else
                {
                    throw new Exception("Invalid id for DB procrastination");
                }

            }
        }

        public int UnwanteDaySection
        {
            get
            {
                return SectionOfDay;
            }

            set
            {
                SectionOfDay = value;
            }
        }


        public DateTimeOffset UndesiredStart
        {
            get
            {
                return FromTime;
            }
            set
            {
                FromTime = value;
            }
        }
        public DateTimeOffset DesiredStart {
            get
            {
                return BeginTIme;
            }
            set
            {
                BeginTIme = value;
            }
        }

        public static DB_Procrastination ConvertToPersistable(Procrastination procrastinationProfile)
        {
            DB_Procrastination retValue = new DB_Procrastination(procrastinationProfile.DislikedStartTime, procrastinationProfile.PreferredStartTime, procrastinationProfile.DislikedDaySection)
            {

            };
            retValue.Id = procrastinationProfile.Id;
            return retValue;
        }
    }
}