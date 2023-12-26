﻿using System.Collections.Generic;


namespace JHT.Scripts.Common.CollectionExtension
{
    public static class CollectionExtension
    {
        private static System.Random rnd = new ();
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }
}