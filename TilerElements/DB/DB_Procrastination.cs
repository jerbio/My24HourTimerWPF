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

        public string Id
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
                    throw new Exception("Invalid id for procrastination ID");
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
    }
}