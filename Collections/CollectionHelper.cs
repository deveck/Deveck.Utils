using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;

namespace Deveck.Utils.Collections
{
    /// <summary>
    /// Reads values from untyped collections
    /// </summary>
    public static class CollectionHelper
    {

        public static T ReadValue<T>(IDictionary col, string key)
        {
            if (col.Contains(key) == false)
                throw new KeyNotFoundException(string.Format("The specified key '{0}' was not found", key));
            return (T)col[key];
        }

        public static T ReadValue<T>(IDictionary col, string key, T defaultValue)
        {
            if (col.Contains(key))
                return (T)col[key];
            else
                return defaultValue;
        }
    }
}
