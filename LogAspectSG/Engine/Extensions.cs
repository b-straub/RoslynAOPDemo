using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace LogAspectSG.Engine
{
    internal static class Extensions
    {
        public static bool True(this bool? value)
        {
            return value.GetValueOrDefault(false);
        }

        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source) where T : class
        {
            return source.Where(x => x is not null)!;
        }

        public static string AccessToString(this INamedTypeSymbol symbol)
        {
            return symbol.DeclaredAccessibility.ToString().ToLowerInvariant();
        }

        public static string CutBefore(this string value, char cut)
        {
            var position = value.IndexOf(cut);

            if (position >= 0)
            {
                return value[..position];
            }

            return value;
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}
