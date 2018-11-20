using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    /// <summary>
    /// Reperesents the Procrastination parameters
    /// </summary>
    public class Procrastination
    {
        protected string _Id { get; set; }
        protected DateTimeOffset FromTime;//Time from which an event was procrastinated
        //TimeSpan Duration;//Span of procrastination
        protected DateTimeOffset BeginTIme;//Next time for a possible calculation of a new schedule
        protected int SectionOfDay;// stores the section of day from which it was procrastinated

        protected Procrastination()
        { 
        
        }

        public Procrastination(DateTimeOffset From, TimeSpan Span)
        {
            FromTime = From;
            //Duration = Span;
            BeginTIme = FromTime.Add(Span);
        }


        public void reset()
        {
            FromTime = new DateTimeOffset();
            BeginTIme = new DateTimeOffset();
        }

        public DateTimeOffset PreferredStartTime
        {
            get
            {
                return BeginTIme;
            }
        }

        public DateTimeOffset DislikedStartTime
        {
            get
            {
                return FromTime;
            }
        }

        virtual public string getId
        {
            get
            {
                return _Id;
            }
        }

        public DayOfWeek DislikedDayOfWeek
        { 
            get
            {
                return FromTime.DayOfWeek;
            }
        }

        public int DislikedDaySection
        {
            get
            {
                return SectionOfDay;
            }
        }

        public ulong DislikedDayIndex(ReferenceNow now)
        {
            return now.getDayIndexFromStartOfTime(FromTime);
        }

        public ulong PreferredDayIndex(ReferenceNow now)
        {
            return now.getDayIndexFromStartOfTime(BeginTIme);
        }

        public Procrastination CreateCopy(string id = "")
        {
            Procrastination retValue = new Procrastination(this.FromTime,BeginTIme-FromTime);
            if(string.IsNullOrEmpty(id))
            {
                retValue._Id = this.getId;
            }
            else
            {
                retValue._Id = id;
            }
            
            return retValue ;
        }

    }
}
