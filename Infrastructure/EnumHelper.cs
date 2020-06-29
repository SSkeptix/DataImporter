using System;
using System.Linq;

namespace Infrastructure
{
    public static class EnumHelper
    {
        public static bool TryParse<T>(string s, StringComparison stringComparison, out T result)
            where T: Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            var valueQuery = values.Where(x => string.Equals(x.ToString(), s, stringComparison));    
            result = valueQuery.SingleOrDefault();
            return valueQuery.Any();
        }

        public static T Parse<T>(string s, StringComparison stringComparison)
            where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>();
            var valueQuery = values.Where(x => string.Equals(x.ToString(), s, stringComparison));

            return valueQuery.Any()
                ? valueQuery.Single()
                : throw new FormatException("Input string was not in a correct format");
        }
    }
}
