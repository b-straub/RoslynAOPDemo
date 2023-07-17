
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.IO;
using System.Linq;
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
            var tas = record.MakeTypeArguments();

            if (record.Method.IsStatic)
            {
                sb.Append($@"
        [InterceptsLocation(@""{record.Path}"", {record.Line + 1}, {record.Character + 1})]
        {record.Method.ContainingType.AccessToString()} static {returnType} Intercept{record.Method.Name}_{record.Line}{record.Character}({record.MakeParameters()})
        {{
            Console.WriteLine($""Call {record.Method.Name} in {Path.GetFileName(record.Path)} Line {record.Line + 1} Character {record.Character + 1}"");
            {record.MakeMethod(returnType == "void", true, tas.TYPE, tas.METHOD)}
        }}
");
            }
            else
            {
                sb.Append($@"
        [InterceptsLocation(@""{record.Path}"", {record.Line + 1}, {record.Character + 1})]
        {record.Method.ContainingType.AccessToString()} static {returnType} Intercept{record.Method.Name}_{record.Line}{record.Character}(this {record.Method.ContainingType.Name}{tas.TYPE} type, {record.MakeParameters()})
        {{
            Console.WriteLine($""Call {record.Method.Name} in {Path.GetFileName(record.Path)} Line {record.Line + 1} Character {record.Character + 1}"");
            {record.MakeMethod(returnType == "void", false, tas.TYPE, tas.METHOD)}
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

        public static (string TYPE, string METHOD) MakeTypeArguments(this InterceptorRecord record)
        {
            string taT = string.Empty;

            if (record.Method.ContainingType.TypeArguments.Any())
            {
                taT += "<";

                foreach(var ta in record.Method.ContainingType.TypeArguments)
                {
                    taT += $"{ta}, ";
                }

                taT = taT.TrimEnd(trim);
                taT += ">";
            }

            string taM = string.Empty;

            if (record.Method.TypeArguments.Any())
            {
                taM += "<";

                foreach (var ta in record.Method.TypeArguments)
                {
                    taM += $"{ta}, ";
                }

                taM = taM.TrimEnd(trim);
                taM += ">";
            }

            return (taT, taM);
        }


        public static string MakeMethod(this InterceptorRecord record, bool isVoid, bool isStatic, string ta, string tc)
        {
            StringBuilder sb = new();

            if (!isVoid)
            {
                sb.Append("return ");
            }

            if (isStatic)
            {
                sb.Append(record.Method.ContainingType.Name);
                sb.Append(ta);
            }
            else
            {
                sb.Append("type");
            }

            sb.Append('.');
            sb.Append(record.Method.Name);
            sb.Append(tc);
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
