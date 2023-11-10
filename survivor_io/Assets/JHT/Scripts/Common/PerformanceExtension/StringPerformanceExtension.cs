using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 쉬운 사용을 위해 namespace 사용안함
namespace JHT.Scripts.Common.PerformanceExtension
{
    public static class StringPerformanceExtension
    {
        private static readonly Dictionary<Type, IDictionary> CachedTypeDictionaries = new();

        public static void ClearCache()
        {
            CachedTypeDictionaries.Clear();
        }

        public static void ClearCache<TType>()
            where TType : unmanaged
        {
            CachedTypeDictionaries.Remove(typeof(TType));
        }

        public static void ClearNumericCache()
        {
            ClearCache<ushort>();
            ClearCache<short>();
            ClearCache<int>();
            ClearCache<uint>();
            ClearCache<long>();
            ClearCache<ulong>();
            ClearCache<char>();
        }

        public static string ToStringCached<TType>(this TType cachingValue)
            where TType : unmanaged
        {
            var enumType = typeof(TType);
            if (false == CachedTypeDictionaries.TryGetValue(enumType, out var iDict))
            {
                iDict = new Dictionary<TType, string>();
                CachedTypeDictionaries[enumType] = iDict;
            }

            if (iDict is not Dictionary<TType, string> container)
            {
                Debug.LogError("type error");
                return cachingValue.ToString();
            }

            if (false == container.TryGetValue(cachingValue, out var stringValue))
            {
                stringValue = cachingValue.ToString();
                container.Add(cachingValue, stringValue);
            }
        
            return stringValue;
        }
    }
}