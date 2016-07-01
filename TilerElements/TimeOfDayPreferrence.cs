using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class TimeOfDayPreferrence
    {
        public enum DaySection {Sleep,Morning,Afternoon,Evening, None,Disabled}
        List<Tuple<int, DaySection, bool>> PreferenceOrder = new List<Tuple<int, DaySection, bool>>(new[] {
            new Tuple<int, DaySection, bool>(0, DaySection.None, false),
            new Tuple<int, DaySection, bool>(1, DaySection.Morning, false),
            new Tuple<int, DaySection, bool>(2, DaySection.Afternoon, false),
            new Tuple<int, DaySection, bool>(3, DaySection.Evening, false),
            new Tuple<int, DaySection, bool>(4, DaySection.Sleep , false)
        });
        //TilerEvent ControlEvent;
        public TimeOfDayPreferrence()
        {
        }

        internal void InitializeGrouping(TilerEvent ControlEvent)
        {
            return;
        }
        public DaySection getCurrentDayPreference()
        {
            if(PreferenceOrder.Count>0)
            {
                return PreferenceOrder.First().Item2;
            }
            else
            {
                return DaySection.Disabled;
            }
        }

        public void setCurrentdayPreference(DaySection preferredSection)
        {
            Tuple <int, DaySection, bool> preferredOption = PreferenceOrder.SingleOrDefault(preferenee => preferenee.Item2 == preferredSection);
            PreferenceOrder.Remove(preferredOption);
            PreferenceOrder.Insert(0, preferredOption);
        }

        public void rejectCurrentPreference()
        {
            if(PreferenceOrder.Count>0)
            {
                PreferenceOrder.RemoveAt(0);
            }
        }
    }
}
