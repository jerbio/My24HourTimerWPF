using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TilerElements
{
    public class SubEventDictionary<TKey, TValue> : IDictionary<string, TValue>, ICollection<TValue>, IEnumerable<TValue>
        where TValue : IHasId
    {
        Dictionary<string, TValue> Data = new Dictionary<string, TValue>();
        public SubEventDictionary()
        {

        }
        public SubEventDictionary(IEnumerable<TValue> data)
        {
            foreach (TValue value in data)
            {
                this.Add(value);
            }
        }

        public TValue this[string key] { get => Data[key]; set => Data[key] = value; }

        public ICollection<string> Keys => Data.Keys;

        public ICollection<TValue> Values => Data.Values;

        public int Count => Data.Count;

        public bool IsReadOnly => false;

        public void Add(string key, TValue value)
        {
            Data.Add(key, value);
        }

        public void Add(TKey key, TValue value)
        {
            Data.Add(key.ToString(), value);
        }

        public void Add(KeyValuePair<string, TValue> item)
        {
            Data.Add(item.Key, item.Value);
        }

        public void Add(TValue item)
        {
            if(!(item is Repetition))
            {
                Data.Add(item.Id, item);
            }
            else
            {
                this.Add(item as Repetition);
            }
        }

        public void Add(Repetition repetition)
        {
            TValue value = (TValue)Convert.ChangeType(repetition, typeof(TValue));
            string weekDayString = repetition.weekDay.ToString();
            if (!Data.ContainsKey(weekDayString))
            {
                Data.Add(weekDayString, value);
            } else
            {
                Data[weekDayString] = value;
            }
            
        }

        public void Clear()
        {
            Data = new Dictionary<string, TValue>();
        }

        public bool Contains(KeyValuePair<string, TValue> item)
        {
            return Data.ContainsKey(item.Key) && Data[item.Key].Equals( item.Value);
        }

        public bool Contains(TValue item)
        {
            return Data.ContainsKey(item.Id) && Data[item.Id].Equals( item);
        }

        public bool ContainsKey(string key)
        {
            return Data.ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<string, TValue>> GetEnumerator()
        {
            return Data.GetEnumerator();
        }

        public bool Remove(string key)
        {
            return Data.Remove(key);
        }

        public bool Remove(KeyValuePair<string, TValue> item)
        {
            return Data.Remove(item.Key);
        }

        public bool Remove(TValue item)
        {
            return Data.Remove(item.Id);
        }

        public bool TryGetValue(string key, out TValue value)
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Data.Values.GetEnumerator();
        }

        IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator()
        {
            return Data.Values .GetEnumerator();
        }

        public Dictionary<string, TValue> Collection {
            get
            {
                return Data as Dictionary<string, TValue>;
            }

            set
            {
                Data = value as Dictionary<string, TValue>;
            }
        }
    }
}
