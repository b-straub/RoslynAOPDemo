using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LogAspectSG.Engine
{
    internal static class SourceDump
    {
        public static string DumpNamespace(this IEnumerable<InterceptorRecord> records, string usedNamespace)
        {
            StringBuilder sb = new();

            var usedRecords = records.Where(r => r.Method.ContainingType.ContainingNamespace.Name == usedNamespace);

            var usings = DefaultUsings(usedRecords);

            _ = sb.Append(DumpNamespaceFirst(usedNamespace, usings));

            foreach (var record in usedRecords)
            {
                _ = sb.Append(record.DumpRecord());
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

        private static IEnumerable<string> DefaultUsings(IEnumerable<InterceptorRecord> records)
        {
            List<string> usings = new()
            {
                "System.Runtime.CompilerServices"
            };

            var recordUsings = records.Select(r => r.Method.ContainingType.ContainingNamespace.Name);

            usings.AddRange(recordUsings);

            return usings.Distinct();
        }
    }
}
