using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
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
        protected DateTimeOffset _FromTime;//Time from which an event was procrastinated
        protected DateTimeOffset _BeginTIme;//Next time for a possible calculation of a new schedule
        protected int _SectionOfDay;// stores the section of day from which it was procrastinated

        protected Procrastination()
        { 
        
        }

        public Procrastination(DateTimeOffset From, TimeSpan Span)
        {
            _FromTime = From;
            _BeginTIme = _FromTime.Add(Span);
        }


        public void reset()
        {
            _FromTime = new DateTimeOffset();
            _BeginTIme = new DateTimeOffset();
        }

        public DateTimeOffset PreferredStartTime
        {
            get
            {
                return _BeginTIme;
            }
        }

        public DateTimeOffset DislikedStartTime
        {
            get
            {
                return _FromTime;
            }
        }
        [ForeignKey("Id")]
        public CalendarEvent Event { get; set; }

        virtual public string Id
        {
            set
            {
                _Id = value;
            }
            get
            {
                return _Id;
            }
        }

        public DayOfWeek DislikedDayOfWeek
        { 
            get
            {
                return _FromTime.DayOfWeek;
            }
        }

        public int DislikedDaySection
        {
            get
            {
                return _SectionOfDay;
            }
        }

        public ulong DislikedDayIndex
        {
            get 
            {
                return ReferenceNow.getDayIndexFromStartOfTime(_FromTime);
            }
        }

        public ulong PreferredDayIndex
        {
            get
            {
                return ReferenceNow.getDayIndexFromStartOfTime(_BeginTIme);
            }
        }

        public Procrastination CreateCopy(string id = "")
        {
            Procrastination retValue = new Procrastination(this._FromTime,_BeginTIme-_FromTime);
            if(string.IsNullOrEmpty(id))
            {
                retValue._Id = this.Id;
            }
            else
            {
                retValue._Id = id;
            }
            
            return retValue ;
        }
        public DateTimeOffset FromTime
        {
            set
            {
                _FromTime = value;
            }
            get
            {
                return _FromTime;
            }
        }
        public DateTimeOffset BeginTIme
        {
            set
            {
                _BeginTIme = value;
            }
            get
            {
                return _BeginTIme;
            }
        }
        public int SectionOfDay
        {
            set
            {
                _SectionOfDay = value;
            }
            get
            {
                return SectionOfDay;
            }
        }
    }
}
