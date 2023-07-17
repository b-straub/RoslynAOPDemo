
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Text;

namespace LogAspectSG.Engine
{
    internal record InterceptorRecordBase(IMethodSymbol Method, IdentifierNameSyntax Identifier);

    internal record InterceptorRecord(IMethodSymbol Method, IdentifierNameSyntax Identifier, bool InMethod, string Path, int Line, int Character) :
        InterceptorRecordBase(Method, Identifier);

    internal static class RecordExtensions
    {
        public static string DumpRecord(this InterceptorRecord record)
        {
            StringBuilder sb = new();
            var returnType = record.Method.ReturnType.ToString();

            if (record.Method.IsStatic)
            {
                sb.Append($@"
        [InterceptsLocation(@""{record.Path}"", {record.Line + 1}, {record.Character + 1})]
        {record.Method.ContainingType.AccessToString()} static {returnType} Intercept{record.Method.Name}_{record.Line}{record.Character}({record.MakeParameters()})
        {{
            Console.WriteLine($""Call {record.Method.Name} in {Path.GetFileName(record.Path)} Line {record.Line + 1} Character {record.Character + 1}"");
            {record.MakeMethod(returnType == "void", true)}
        }}
");
            }
            else
            {
                sb.Append($@"
        [InterceptsLocation(@""{record.Path}"", {record.Line + 1}, {record.Character + 1})]
        {record.Method.ContainingType.AccessToString()} static {returnType} Intercept{record.Method.Name}_{record.Line}{record.Character}(this {record.Method.ContainingType.Name} type, {record.MakeParameters()})
        {{
            Console.WriteLine($""Call {record.Method.Name} in {Path.GetFileName(record.Path)} Line {record.Line + 1} Character {record.Character + 1}"");
            {record.MakeMethod(returnType == "void", false)}
        }}
");
            }

            return sb.ToString();
        }

        private static readonly char[] trim = new[] { ' ', ',' };

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


        public static string MakeMethod(this InterceptorRecord record, bool isVoid, bool isStatic)
        {
            StringBuilder sb = new();

            if (!isVoid)
            {
                sb.Append("return ");
            }

            if (isStatic)
            {
                sb.Append(record.Method.ContainingType.Name);
            }
            else
            {
                sb.Append("type");
            }

            sb.Append('.');
            sb.Append(record.Method.Name);
            sb.Append('(');

            StringBuilder sbp = new();

            foreach (var parameter in record.Method.Parameters)
            {
                sbp.Append(parameter.Name);
                sbp.Append(", ");
            }

            sb.Append(sbp.ToString().TrimEnd(trim));

            sb.Append(");");

            return sb.ToString();
        }
    }
}
