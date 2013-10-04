using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Owin.Extensions
{
    public static class DictionaryExtensions
    {
        public static void AddValue(this IDictionary<string, string[]> dictionary, string key, string value) {
            string[] items;
            if (dictionary.TryGetValue(key, out items)) {
                Array.Resize(ref items, items.Length + 1);
            }
            else {
                items = new string[1];
            }
            items[items.Length - 1] = value;
            dictionary[key] = items;
        }

        public static void AddValues(this IDictionary<string, string[]> dictionary, string key, IEnumerable<string> values) {
            string[] items;
            List<string> list = values.ToList();
            if (dictionary.TryGetValue(key, out items)) {
                Array.Resize(ref items, items.Length + list.Count);
            }
            else {
                items = new string[list.Count];
            }
            list.CopyTo(items, items.Length);
            dictionary[key] = items;
        }

        public static T GetNestedValueOrDefault<T>(this IDictionary<string, object> dictionary, string key, string subkey) {
            return GetNestedValueOrDefault(dictionary, key, subkey, default(T));
        }

        public static T GetNestedValueOrDefault<T>(this IDictionary<string, object> dictionary, string key, string subkey, T defaultValue) {
            var subDictionary = dictionary.GetValueOrDefault<IDictionary<string, object>>(key);
            return subDictionary == null ? defaultValue : subDictionary.GetValueOrDefault(subkey, defaultValue);
        }

        public static T GetValue<T>(this IDictionary<string, object> dictionary, string key) {
            return (T)dictionary[key];
        }

        public static T GetValueOrCreate<T>(this IDictionary<string, object> dictionary, string key, Func<T> create) {
            object value;
            if (!dictionary.TryGetValue(key, out value)) {
                value = create();
                dictionary.Add(key, value);
            }
            return (T)value;
        }

        public static T GetValueOrDefault<T>(this IDictionary<string, object> dictionary, string key) {
            return GetValueOrDefault(dictionary, key, default(T));
        }

        public static T GetValueOrDefault<T>(this IDictionary<string, object> dictionary, string key, T defaultValue) {
            object value;
            if (dictionary.TryGetValue(key, out value)) {
                return (T)value;
            }
            return defaultValue;
        }

        /// <summary>
        /// Sets the value if it is not null, with the specified key.
        /// 'null' values result in removing any item with the specified key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SetValue<T>(this IDictionary<string, object> dictionary, string key, T value) {
            if (value == null) {
                dictionary.Remove(key);
            }
            else {
                dictionary[key] = value;
            }
        }
    }
}