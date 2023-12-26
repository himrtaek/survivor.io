using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;


namespace JHT.Scripts.Common.PerformanceExtension
{
    public static class EnumPerformanceExtension
    {
        public static TType AsType<TEnum, TType>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
            where TType : unmanaged
        {
            if (Unsafe.SizeOf<TEnum>() != Unsafe.SizeOf<TType>())
            {
                Debug.LogError("type mismatch");
                throw new Exception();
            }

            TType value = Unsafe.As<TEnum, TType>(ref enumValue);
            return value;
        }

        public static long AsInteger<TEnum>(this TEnum enumValue)
            where TEnum : unmanaged, Enum
        {
            long value;
            if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<int>())
            {
                value = Unsafe.As<TEnum, int>(ref enumValue);
            }
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<byte>())
            {
                value = Unsafe.As<TEnum, byte>(ref enumValue);
            }
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<long>())
            {
                value = Unsafe.As<TEnum, long>(ref enumValue);
            }
            else if (Unsafe.SizeOf<TEnum>() == Unsafe.SizeOf<short>())
            {
                value = Unsafe.As<TEnum, short>(ref enumValue);
            }
            else
            {
                throw new Exception("type mismatch");
            }
            return value;
        }

        public static bool HasFlagNonAlloc<TEnum>(this TEnum targetEnum, TEnum checkEnum)
            where TEnum : unmanaged, Enum
        {
            var lhsV = targetEnum.AsInteger();
            var rhsV = checkEnum.AsInteger();

            return (lhsV & rhsV) == rhsV;
        }

        #region GetValues Cache
    
        private static readonly Dictionary<Type, Array> CachedValuesDictionary = new();

        public static void ClearValuesCache()
        {
            CachedValuesDictionary.Clear();
        }
    
        public static void ClearValuesCache<TEnum>()
            where TEnum : unmanaged, Enum
        {
            CachedValuesDictionary.Remove(typeof(TEnum));
        }

        public static Array GetValuesCached<TEnum>()
            where TEnum : unmanaged, Enum
        {
            var enumType = typeof(TEnum);
            if (false == CachedValuesDictionary.TryGetValue(enumType, out var valuesList))
            {
                valuesList = Enum.GetValues(enumType);
                CachedValuesDictionary[enumType] = valuesList;
            }

            return valuesList;
        }
    
        #endregion // GetValues Cache
    }
}