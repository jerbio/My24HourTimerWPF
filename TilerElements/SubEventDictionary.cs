using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class SubEventDictionary : IDictionary<EventID, SubCalendarEvent>, ICollection<SubCalendarEvent>
    {
        Dictionary<EventID, SubCalendarEvent> Data = new Dictionary<EventID, SubCalendarEvent>();
        public SubEventDictionary()
        {

        }
        public SubEventDictionary(ICollection<SubCalendarEvent> data)
        {
            Data = data.ToDictionary(subEvent => subEvent.SubEvent_ID, subEvent => subEvent);
        }
        public SubCalendarEvent this[EventID key] { get => Data[key]; set => Data[key] = value; }

        public ICollection<EventID> Keys => Data.Keys;

        public ICollection<SubCalendarEvent> Values => Data.Values;

        public int Count => Data.Count;

        public bool IsReadOnly => false;

        public void Add(EventID key, SubCalendarEvent value)
        {
            Data.Add(key, value);
        }

        public void Add(KeyValuePair<EventID, SubCalendarEvent> item)
        {
            Data.Add(item.Key, item.Value);
        }

        public void Add(SubCalendarEvent item)
        {
            Data.Add(item.SubEvent_ID, item);
        }

        public void Clear()
        {
            Data = new Dictionary<EventID, SubCalendarEvent>();
        }

        public bool Contains(KeyValuePair<EventID, SubCalendarEvent> item)
        {
            return Data.ContainsKey(item.Key) && Data[item.Key] == item.Value;
        }

        public bool Contains(SubCalendarEvent item)
        {
            return Data.ContainsKey(item.SubEvent_ID) && Data[item.SubEvent_ID] == item;
        }

        public bool ContainsKey(EventID key)
        {
            return Data.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<EventID, SubCalendarEvent>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(SubCalendarEvent[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<EventID, SubCalendarEvent>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public bool Remove(EventID key)
        {
            return Data.Remove(key);
        }

        public bool Remove(KeyValuePair<EventID, SubCalendarEvent> item)
        {
            return Data.Remove(item.Key);
        }

        public bool Remove(SubCalendarEvent item)
        {
            return Data.Remove(item.SubEvent_ID);
        }

        public bool TryGetValue(EventID key, out SubCalendarEvent value)
        {
            return Data.TryGetValue(key,out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.Values.GetEnumerator();
        }

        IEnumerator<SubCalendarEvent> IEnumerable<SubCalendarEvent>.GetEnumerator()
        {
            return Data.Values.GetEnumerator();
        }

        public Dictionary<EventID, SubCalendarEvent> getData => Data;
    }
}
