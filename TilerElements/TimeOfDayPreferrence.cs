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
        Stack<Tuple<int, DaySection, bool>> PreferenceOrder = new Stack<Tuple<int, DaySection, bool>>(new[] { new Tuple<int, DaySection, bool>(4, DaySection.Morning, false), new Tuple<int, DaySection, bool>(3, DaySection.Evening, false), new Tuple<int, DaySection, bool>(2, DaySection.Afternoon, false), new Tuple<int, DaySection, bool>(1, DaySection.Evening, false), new Tuple<int, DaySection, bool>(0, DaySection.None, false) });
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
                return PreferenceOrder.Peek().Item2;
            }
            else
            {
                return DaySection.Disabled;
            }
        }

        public void rejectCurrentPreference()
        {
            if(PreferenceOrder.Count>0)
            {
                PreferenceOrder.Pop();
            }
        }
    }
}
