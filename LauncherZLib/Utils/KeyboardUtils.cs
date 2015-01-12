using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace LauncherZLib.Utils
{
    public static class KeyboardUtils
    {
        
        private static readonly KeyConverter KeyConverter = new KeyConverter();

        public static Key GetKeyFromKeyEvent(KeyEventArgs e)
        {
            if (e.Key.Equals(Key.DeadCharProcessed))
            {
                return e.DeadCharProcessedKey;
            }
            if (e.Key.Equals(Key.System))
            {
                return e.SystemKey;
            }
            if (e.Key.Equals(Key.ImeProcessed))
            {
                return e.ImeProcessedKey;
            }
            return e.Key;
        }

        public static Key[] ParseKeyCombination(string str)
        {
            if (string.IsNullOrWhiteSpace(str))
                return new Key[0];

            return str.Split(new[] {"+"}, StringSplitOptions.RemoveEmptyEntries)
                .Select(s =>
                {
                    var converted = KeyConverter.ConvertFromString(s);
                    return converted != null ? (Key)converted : Key.None;
                })
                .ToArray();
        }

        public static string KeyCombinationToString(Key[] keys)
        {
            return keys == null ? "" : string.Join("+", keys);
        }
    }
}