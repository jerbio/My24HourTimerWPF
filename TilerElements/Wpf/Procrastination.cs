using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    /// <summary>
    /// Reperesents the Procrastination parameters
    /// </summary>
    public class Procrastination
    {
        protected DateTimeOffset FromTime;//Time from which an event was procrastinated
        //TimeSpan Duration;//Span of procrastination
        protected DateTimeOffset BeginTIme;//Next time for a possible calculation of a new schedule
        protected int SectionOfDay;// stores the section of day from which it was procrastinated
        protected string ID = Guid.NewGuid().ToString();
        protected Procrastination()
        { 
        
        }

        public Procrastination(DateTimeOffset From, TimeSpan Span)
        {
            FromTime = From;
            //Duration = Span;
            BeginTIme = FromTime.Add(Span);
        }

        #region properties

        virtual public string Id
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
                    Id = value;
                }
                else
                {
                    throw new Exception("Invalid id for procrastination");
                }

            }
        }

        virtual public DateTimeOffset PreferredStartTime
        {
            get
            {
                return BeginTIme;
            }
            set
            {
                BeginTIme = value;
            }
        }

        virtual public DateTimeOffset DislikedStartTime
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

        public DayOfWeek DislikedDayOfWeek
        { 
            get
            {
                return FromTime.DayOfWeek;
            }
        }

        virtual public int DislikedDaySection
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

        public ulong DislikedDayIndex
        {
            get 
            {
                return ReferenceNow.getDayIndexFromStartOfTime(FromTime);
            }
        }

        public ulong PreferredDayIndex
        {
            get
            {
                return ReferenceNow.getDayIndexFromStartOfTime(BeginTIme);
            }
        }
        #endregion

        #region functions
        public Procrastination CreateCopy()
        {
            Procrastination retValue = new Procrastination(this.FromTime, BeginTIme - FromTime);
            return retValue;
        }

        public void reset()
        {
            FromTime = new DateTimeOffset();
            BeginTIme = new DateTimeOffset();
        }
        #endregion
    }
}
