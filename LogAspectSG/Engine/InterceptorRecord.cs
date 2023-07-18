
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace LogAspectSG.Engine
{
    internal record InterceptorRecordBase(IMethodSymbol Method, SimpleNameSyntax NameSyntax);

    internal record InterceptorRecord(IMethodSymbol Method, SimpleNameSyntax NameSyntax, bool InMethod, string Path, int Line, int Character) :
        InterceptorRecordBase(Method, NameSyntax);

    internal static class RecordExtensions
    {
        public static string DumpRecord(this InterceptorRecord record)
        {
            StringBuilder sb = new();
            var returnType = record.Method.ReturnType.ToString();
            var taT = MakeTypeArguments(record.Method.ContainingType.TypeArguments);

            if (record.Method.IsStatic)
            {
                sb.Append($@"
        [InterceptsLocation(@""{record.Path}"", {record.Line + 1}, {record.Character + 1})]
        {record.Method.ContainingType.AccessToString()} static {returnType} Intercept{record.Method.Name}_{record.Line}{record.Character}({record.MakeParameters()})
        {{
            {record.MakeMethod(returnType == "void", true)}
        }}
");
            }
            else
            {
                sb.Append($@"
        [InterceptsLocation(@""{record.Path}"", {record.Line + 1}, {record.Character + 1})]
        {record.Method.ContainingType.AccessToString()} static {returnType} Intercept{record.Method.Name}_{record.Line}{record.Character}(this {record.Method.ContainingType.Name}{taT} type, {record.MakeParameters()})
        {{
            {record.MakeMethod(returnType == "void", false)}
        }}
");
            }

            return sb.ToString();
        }

        private static readonly char[] trim = new[] { ' ', ',', '.' };

        public static string MakeParameters(this InterceptorRecord record)
        {
            StringBuilder sb = new();

            foreach (var parameter in record.Method.Parameters)
            {
                sb.Append(parameter.ToDisplayString());
                sb.Append(", ");
            }

            return sb.ToString().TrimEnd(trim);
        }

        public static string MakeStaticType(this InterceptorRecord record)
        {
            var typeSymbol = record.Method.ContainingType;
            var typeName = string.Empty;

            while (typeSymbol is not null && typeSymbol.ContainingNamespace.Equals(record.Method.ContainingType.ContainingNamespace, SymbolEqualityComparer.Default))
            {
                var tn = typeSymbol.Name;
                string taT = MakeTypeArguments(typeSymbol.TypeArguments);
                tn += taT;
                tn += ".";

                typeName = tn + typeName;

                typeSymbol = typeSymbol.ContainingType;
            }

            return typeName.TrimEnd(trim);
        }

        private static string MakeTypeArguments(this ImmutableArray<ITypeSymbol> typeArguments)
        {
            string taT = string.Empty;

            if (typeArguments.Any())
            {
                taT += "<";

                foreach (var ta in typeArguments)
                {
                    taT += $"{ta}, ";
                }

                taT = taT.TrimEnd(trim);
                taT += ">";
            }

            return taT;
        }

        public static string MakeMethod(this InterceptorRecord record, bool isVoid, bool isStatic)
        {
            StringBuilder sbm = new();
            var returnType = record.Method.ReturnType.ToString();

            if (!isVoid)
            {
                sbm.Append("result = ");
            }

            if (isStatic)
            {
                sbm.Append(record.MakeStaticType());
            }
            else
            {
                sbm.Append("type");
            }

            sbm.Append('.');
            sbm.Append(record.Method.Name);
            string taM = MakeTypeArguments(record.Method.TypeArguments);
            sbm.Append(taM);
            sbm.Append('(');

            StringBuilder sbp = new();

            foreach (var parameter in record.Method.Parameters)
            {
                sbp.Append(parameter.Name);
                sbp.Append(", ");
            }

            sbm.Append(sbp.ToString().TrimEnd(trim));

            sbm.Append(");");

            StringBuilder sb = new();

            if (!isVoid)
            {
                sb.Append($@"
            {returnType} result = null;
");
            }

            sb.Append($@"
            try
            {{
                {sbm}
                Console.WriteLine($""Success for {record.Method.Name} in {Path.GetFileName(record.Path)} Line {record.Line + 1} Character {record.Character + 1}"");
            }}
            catch(Exception e)
            {{
                Console.WriteLine($""Error {{e.Message}} for {record.Method.Name} in {Path.GetFileName(record.Path)} Line {record.Line + 1} Character {record.Character + 1}"");
                throw;
            }}
");

            if (!isVoid)
            {
                sb.Append(@"
            return result;");
            }

            return sb.ToString();
        }
    }
}
