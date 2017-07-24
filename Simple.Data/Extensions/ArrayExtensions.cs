using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shitty.Data.Extensions
{
    public static class ArrayExtensions
    {
        public static T[] Append<T>(this T[] array, T newItem)
            where T : class
        {
            if (array.Length == 0) return new[] {newItem};
            var newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = newItem;
            return newArray;
        }

        public static T[] ReplaceOrAppend<T>(this T[] array, T newItem)
            where T : class
        {
            if (array.Length == 0) return new[] { newItem };
            var existingItem = array.FirstOrDefault(item => item.GetType() == newItem.GetType());
            if (existingItem != null)
            {
                var replacedArray = (T[])array.Clone();
                int index = Array.IndexOf(array, existingItem);
                replacedArray[index] = newItem;
                return replacedArray;
            }
            var newArray = new T[array.Length + 1];
            array.CopyTo(newArray, 0);
            newArray[array.Length] = newItem;
            return newArray;
        }

        public static T[] Replace<T>(this T[] array, int index, T newItem)
        {
            var newArray = (T[])array.Clone();
            newArray[index] = newItem;
            return newArray;
        }
    }
}
