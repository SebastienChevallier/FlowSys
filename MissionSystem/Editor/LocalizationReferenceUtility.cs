using System;

namespace GAME.MissionSystem.Editor
{
    public static class LocalizationReferenceUtility
    {
        public const string DefaultTable = "PLS_Strings";

        public static string BuildReference(string tableName, string key)
        {
            if (string.IsNullOrEmpty(tableName) || tableName == DefaultTable)
                return key;
            return $"{tableName}/{key}";
        }

        public static string GetKey(string reference)
        {
            if (string.IsNullOrEmpty(reference))
                return string.Empty;

            if (reference.Contains("/"))
            {
                var parts = reference.Split('/');
                return parts.Length == 2 ? parts[1] : reference;
            }

            return reference;
        }

        public static string GetTableName(string reference, string defaultTable = DefaultTable)
        {
            if (string.IsNullOrEmpty(reference))
                return defaultTable;

            if (reference.Contains("/"))
            {
                var parts = reference.Split('/');
                return parts.Length == 2 ? parts[0] : defaultTable;
            }

            return defaultTable;
        }

        public static string GetDisplayLabel(string reference, string fallback = "")
        {
            string key = GetKey(reference);
            return string.IsNullOrEmpty(key) ? fallback : key;
        }
    }
}
