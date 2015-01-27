using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements
{
    public class ConflictProfile
    {
        private bool ConflictFlag;
        HashSet<string> ConflictingEvents;
        int ConflictType;

        public ConflictProfile(int conflictType=0,bool conflictFlag=false)
        {
            ConflictFlag = conflictFlag;
            ConflictingEvents = new HashSet<string>();
            this.ConflictType = conflictType;
        }

        public ConflictProfile(string allIds)
        {
            ConflictingEvents = new HashSet<string>();
            ConflictFlag = false;
            if(!string.IsNullOrEmpty(allIds))
            {
                LoadConflictingIDs(allIds.Split(','));
            }

        }

        public ConflictProfile CreateCopy()
        {
            ConflictProfile retValue = new ConflictProfile();
            retValue.ConflictFlag = this.ConflictFlag;
            retValue.ConflictingEvents = new HashSet<string>(ConflictingEvents);
            retValue.ConflictType = ConflictType;
            return retValue;
        }

        public bool isConflicting()
        {
            return ConflictFlag;
        }
        
        public void UpdateConflictFlag(bool conflictStatus)
        {
            ConflictFlag=conflictStatus;
        }

        public bool AddConflictingEvents(string eventID)
        {
            ConflictFlag = true;
            return ConflictingEvents.Add(eventID);
        }

        public bool RemoveConflictingEvents(string eventID)
        {
            bool retValue= ConflictingEvents.Remove(eventID);
            if (ConflictingEvents.Count < 1)
            {
                ConflictFlag = false;
            }
            return retValue;
        }

        public void LoadConflictingIDs(IEnumerable<string> EventIDs)
        {
            string []myArrayOfStrings=EventIDs.ToArray();
            //ConflictFlag = false;
            int i=0;
            for (; i < myArrayOfStrings.Length;i++ )
            {
                ConflictingEvents.Add(myArrayOfStrings[i]);
                ConflictFlag = true;
            }
        }

        public int conflictType
        {
            get
            {
                return ConflictType;
            }
        }

        public IEnumerable<string> getConflictingEventIDs()
        {
            return ConflictingEvents;
        }

        

    }
}
