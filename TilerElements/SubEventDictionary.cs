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
        Dictionary<string, Object> Data = new Dictionary<string, Object>();
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

        public TValue this[string key] { get => (TValue)Data[key]; set => Data[key] = value; }

        public ICollection<string> Keys => Data.Keys;

        public ICollection<TValue> Values => Data.Values as ICollection<TValue>;

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
            Data.Add(item.Id, item);
        }

        public void Add(Repetition repetition)
        {
            Data.Add(repetition.weekDay.ToString(), repetition);
        }

        public void Clear()
        {
            Data = new Dictionary<string, Object>();
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
            return (Data as Dictionary<string, TValue>) .GetEnumerator();
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
            return (Data.Values as IEnumerable<TValue>) .GetEnumerator();
        }

        public Dictionary<string, TValue> Collection {
            get
            {
                return Data as Dictionary<string, TValue>;
            }

            set
            {
                Data = value as Dictionary<string, Object>;
            }
        }
    }
}
