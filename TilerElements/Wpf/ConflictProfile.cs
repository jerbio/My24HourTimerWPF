using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TilerElements.Wpf
{
    public class ConflictProfile
    {
        protected bool ConflictFlag;
        protected HashSet<string> ConflictingEvents;
        protected int TypeOfConflict;
        protected string ID = Guid.NewGuid().ToString();
        public ConflictProfile(int conflictType=0,bool conflictFlag=false)
        {
            ConflictFlag = conflictFlag;
            ConflictingEvents = new HashSet<string>();
            this.TypeOfConflict = conflictType;
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

        public ConflictProfile()
        {
            ConflictFlag = false;
            ConflictingEvents = new HashSet<string>();

        }

        public ConflictProfile CreateCopy()
        {
            ConflictProfile retValue = new ConflictProfile();
            retValue.ConflictFlag = this.ConflictFlag;
            retValue.ConflictingEvents = new HashSet<string>(ConflictingEvents);
            retValue.TypeOfConflict = TypeOfConflict;
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
            bool RetValue = ConflictingEvents.Add(eventID);
            ConflictCount = ConflictingEvents.Count;
            return RetValue;
        }

        public bool RemoveConflictingEvents(string eventID)
        {
            bool retValue= ConflictingEvents.Remove(eventID);
            if (ConflictingEvents.Count < 1)
            {
                ConflictFlag = false;
            }
            ConflictCount = ConflictingEvents.Count;
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



        public IEnumerable<string> getConflictingEventIDs()
        {
            return ConflictingEvents;
        }


        #region properties
        virtual public int ConflictType
        {
            get
            {
                return TypeOfConflict;
            }
            set
            {
                TypeOfConflict = value;
            }
        }

        virtual public int ConflictCount { get; set; } = 0;

        virtual public bool Flag
        {
            get
            {
                return ConflictFlag;
            }
            set
            {
                ConflictFlag = value;
            }
        }

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
                    throw new Exception("Invalid id for Conflict");
                }

            }
        }


        #endregion


    }
}
