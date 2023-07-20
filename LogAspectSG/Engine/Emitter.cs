using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAspectSG.Engine
{
    internal static class Emitter
    {
        public static string DumpNamespace(this IEnumerable<InterceptorStore> stores, string usedNamespace)
        {
            StringBuilder sb = new();

            IEnumerable<InterceptorStore> usedStores = stores.Where(r => r.Method.ContainingType.ContainingNamespace.Name == usedNamespace);

            IEnumerable<string> usings = CreateUsings(usedStores);

            _ = sb.Append(DumpNamespaceFirst(usedNamespace, usings));

            foreach (InterceptorStore? store in usedStores)
            {
                _ = sb.Append(store.DumpStore());
            }
            _ = sb.Append(DumpNamespaceLast());

            return sb.ToString();
        }

        private static string DumpNamespaceFirst(string namespaceName, IEnumerable<string> usings)
        {
            StringBuilder sb = new();

            foreach (string? u in usings)
            {
                _ = sb.Append(@$"using {u};
");
            };

            _ = sb.Append($@"
#nullable enable

namespace System.Runtime.CompilerServices
{{
    #pragma warning disable CS9113
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    file sealed class InterceptsLocationAttribute(string filePath, int line, int character) : Attribute
    {{
    }} 
    #pragma warning restore CS9113
}}

namespace {namespaceName}
{{
    public static class Interceptors
    {{");
            return sb.ToString().Trim();
        }

        private static string DumpNamespaceLast()
        {
            StringBuilder sb = new();

            _ = sb.Append($@"
    }}
}}
");
            return sb.ToString();
        }

        private static IEnumerable<string> CreateUsings(IEnumerable<InterceptorStore> stores)
        {
            List<string> usings = new()
            {
                "System.Runtime.CompilerServices"
            };

            IEnumerable<string> recordUsings = stores.Select(r => r.Method.ContainingType.ContainingNamespace.Name);

            usings.AddRange(recordUsings);

            return usings.Distinct();
        }
    }
}
